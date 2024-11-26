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
                    _gameServer = new TcpClient();
                    await _gameServer.ConnectAsync(
                        IPEndPoint.Parse(
                            command.Data.Options.First().Value.ToString()
                        )
                    );

                    await command.RespondAsync($"Succeslfully connect to: {command.Data.Options.First().Value.ToString()}");
                    break;

                case "ping-game":
                    await command.DeferAsync();

                    var stream = _gameServer.GetStream();
                    var gameCommand = "ping";
                    var buffer = Encoding.UTF8.GetBytes(gameCommand);
                    await stream.WriteAsync(buffer, 0, buffer.Length);

                    while (_gameServer.Available == 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                    var res = new byte[_gameServer.Available];
                    await stream.ReadAsync(res, 0, res.Length);
                    var resDec = Encoding.UTF8.GetString(res);

                    await command.ModifyOriginalResponseAsync(prop =>
                        prop.Content = $"Game respondeded with: {resDec}");
                    break;
            }
        }

    }
}
