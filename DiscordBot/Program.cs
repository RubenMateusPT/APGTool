using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DiscordBot
{
    internal class Program
    {
        private static DiscordSocketClient _client;
        private static TcpClient _gameServer;
        private static Task streamThread;
        private static CancellationTokenSource cancelationToken;
        private static SocketSlashCommand pingCommand;
        private static SocketSlashCommand doorRequestCommand;
        private static ulong GuildID;
        private static ulong TextChatID;
        private static bool? playerWonGame;

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            _client = new DiscordSocketClient();

            _client.Log += Log;
            _client.Ready += Ready;
            _client.SlashCommandExecuted += SlashCommandHandler;

            //  You can assign your bot token to a string, and pass that in to connect.
            //  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
            var token = builder.GetSection("BotToken").Value;

            // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            // var token = File.ReadAllText("token.txt");
            // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private static async Task Ready()
        {
            List<SlashCommandBuilder> commands = new List<SlashCommandBuilder>();

            commands.Add(new SlashCommandBuilder().WithName("ping").WithDescription("Pings the bot"));
            commands.Add(new SlashCommandBuilder().WithName("bot-ip").WithDescription("Gets bot IP"));
            commands.Add(new SlashCommandBuilder().WithName("clear").WithDescription("Clears text channel"));
            commands.Add(new SlashCommandBuilder().WithName("connect").WithDescription("Connect to a game instance").AddOption("ip", ApplicationCommandOptionType.String,"The ip and port of game", isRequired:true));
            commands.Add(new SlashCommandBuilder().WithName("ping-game").WithDescription("Ping the game server"));

            commands.Add(new SlashCommandBuilder().WithName("give-life").WithDescription("Give a life to player"));
            commands.Add(new SlashCommandBuilder().WithName("slow-down-time").WithDescription("Slow down time for a brief moment"));
            commands.Add(new SlashCommandBuilder().WithName("open-door").WithDescription("Open the castles door"));

            try
            {
                foreach (var command in commands)
                {
                    await _client.CreateGlobalApplicationCommandAsync(command.Build());
                }
            }
            catch (ApplicationCommandException exception)
            {
                // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                Console.WriteLine(json);
            }
        }

        private static async Task SlashCommandHandler(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "ping":
                    await command.RespondAsync("pong");
                    break;

                case "bot-ip":
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    await command.RespondAsync($"Local IP: {host.AddressList.First(h => h.AddressFamily == AddressFamily.InterNetwork).ToString()}");
                    break;

                case "clear":
                    await command.RespondAsync("Deleting text channel messages");
                    var messages = await command.Channel.GetMessagesAsync().FlattenAsync();
                    foreach (var message in messages)
                    {
                        await command.Channel.DeleteMessageAsync(message);
                    }
                    break;
                case "connect":
                    if (_gameServer != null)
                    {
                        if (cancelationToken != null)
                        {
                            cancelationToken.Cancel();
                        }
                        _gameServer.Dispose();
                        playerWonGame = null;
                    }

                    _gameServer = new TcpClient();

                    try
                    {
                        await _gameServer.ConnectAsync(
                            IPEndPoint.Parse(
                                command.Data.Options.First().Value.ToString()
                            )
                        );

                        _gameServer.ReceiveBufferSize = int.MaxValue;
                        GuildID = command.GuildId.Value;
                        TextChatID = command.ChannelId.Value;

                        cancelationToken = new CancellationTokenSource();
                        streamThread = Task.Run(async () =>
                        {
                            while (_gameServer.Connected)
                            {
                                if (cancelationToken.Token.IsCancellationRequested)
                                {
                                    cancelationToken.Token.ThrowIfCancellationRequested();
                                    break;
                                }
                                else if (_gameServer.GetStream().CanRead)
                                {
                                    var bytes = new byte[_gameServer.Available];
                                    await _gameServer.GetStream().ReadAsync(bytes, 0, bytes.Length);
                                    var command = Encoding.UTF8.GetString(bytes);

                                    if (bytes.Length > 10000) //image
                                    {
                                        if (playerWonGame.HasValue)
                                        {
                                            var endMessage = playerWonGame.Value
                                                ? "YES! The player was able to beat the level with your help!"
                                                : "Too bad, the player was not able to win, can you help him?";

                                            var ms = new MemoryStream(bytes);
                                            var image = new Image(ms);
                                            await _client.GetGuild(GuildID).GetTextChannel(TextChatID)
                                                .SendFileAsync(image.Stream, $"{(playerWonGame.Value ? "win" : "lost")}.png", text: endMessage);
                                            continue;
                                        }
                                        else
                                        {
                                            var ms = new MemoryStream(bytes);
                                            var image = new Image(ms);
                                            await _client.GetGuild(GuildID).GetTextChannel(TextChatID)
                                                .SendFileAsync(image.Stream, $"decision.png", text: "The player needs help! Please someone lower the bridge for him!");
                                            continue;
                                        }

  
                                    }
                                    else if (command.Contains("endgame"))
                                    {
                                        playerWonGame = command.Split(':').Last() == "y";
                                        continue;
                                    }
                                    else if (command.Contains("doorRequest"))
                                    {
                                        playerWonGame = null;
                                    }

                                    switch (command)
                                    {
                                        case "pong":
                                            await pingCommand.ModifyOriginalResponseAsync(prop =>
                                                prop.Content = "Game said: Pong!");
                                            break;

                                        case "denyDoor":
                                            await doorRequestCommand.ModifyOriginalResponseAsync(prop =>
                                                prop.Content = $"Game said: I dont know what door you are talking about"); ;
                                            break;

                                        case "openDoor":
                                            await doorRequestCommand.ModifyOriginalResponseAsync(prop =>
                                                prop.Content = $"Game said: Thank you for opening the door!"); ;
                                            break;
                                    }
                                }
                            }
                        }, cancelationToken.Token);

                        await command.RespondAsync($"Succeslfully connect to: {command.Data.Options.First().Value.ToString()}");

                    }
                    catch (Exception exception)
                    {
                        _gameServer.Dispose();
                        await command.RespondAsync($"Failed to connect to: {command.Data.Options.First().Value.ToString()}");
                    }
                    break;



                case "ping-game":
                    pingCommand = command;
                    await command.DeferAsync();

                    var gameCommand = "ping";
                    var buffer = Encoding.UTF8.GetBytes(gameCommand);
                    await _gameServer.GetStream().WriteAsync(buffer, 0, buffer.Length); 
                    break;

                case "give-life":
                    await command.DeferAsync();

                    var glb = Encoding.UTF8.GetBytes("givelife");
                    await _gameServer.GetStream().WriteAsync(glb, 0, glb.Length);

                    await command.ModifyOriginalResponseAsync(prop =>
                        prop.Content = $"Gave player a Life!");
                    break;

                case "slow-down-time":
                    await command.DeferAsync();

                    var sdtb = Encoding.UTF8.GetBytes("slowtime");
                    await _gameServer.GetStream().WriteAsync(sdtb, 0, sdtb.Length);

                    await command.ModifyOriginalResponseAsync(prop =>
                        prop.Content = $"Activated Slow Time");
                    break;

                case "open-door":
                    doorRequestCommand = command;
                    await command.DeferAsync();

                    var odb = Encoding.UTF8.GetBytes("opendoor");
                    await _gameServer.GetStream().WriteAsync(odb, 0, odb.Length);
                    break;

            }
        }
    }
}
