using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APG.Discord.Server;
using APG.Server.Discord.Commands;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace APG.Server.Discord
{
    internal class DiscordBot
    {
        private ServerManager _serverManager;

        private DiscordSocketClient _client;
        private string _token;

        private Dictionary<Tuple<ulong, ulong>, BotClient>
            _instances = new Dictionary<Tuple<ulong, ulong>, BotClient>();

        public DiscordBot(string token, ServerManager serverManager)
        {
            _serverManager = serverManager;

            _client = new DiscordSocketClient();
            _token = token;

            _client.Log += Log;
            _client.Ready += Ready;
            _client.SlashCommandExecuted += SlashCommandHandler;
        }

        public async void Start()
        {
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
            Console.WriteLine($"Discord Bot is Online");
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task Ready()
        {
            List<SlashCommandBuilder> commands = new List<SlashCommandBuilder>
            {
                new SlashCommandBuilder()
                    .WithName(Command.HOST_JOIN)
                    .WithDescription("Host Join")
                    .AddOption("code",ApplicationCommandOptionType.String, "Host Code", isRequired:true),
                new SlashCommandBuilder()
                    .WithName(Command.CLIENT_JOIN)
                    .WithDescription($"Client Join")
                    .AddOption("user", ApplicationCommandOptionType.User, "Host" ,isRequired:true)
            };

            try
            {
                foreach (var command in commands)
                    await _client.CreateGlobalApplicationCommandAsync(command.Build());
            }
            catch (HttpException ex)
            {
                var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
                Console.WriteLine(json);
            }

            Console.WriteLine($"Application is online and ready to roll!");
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            var guild = _client.GetGuild(command.GuildId.Value);
            var guildUser = guild.GetUser(command.User.Id);

            switch (command.Data.Name)
            {
                case Command.HOST_JOIN:
                    var rawCode = command.Data.Options.First().Value.ToString();

                    if (!Guid.TryParse(rawCode, out var code))
                    {
                        await command.RespondAsync("Invalid code format!");
                        return;
                    }
                    
                    var unityClient = _serverManager.GetClient(code);
                    if (unityClient == null)
                    {
                        await command.RespondAsync("Server not found!");
                        return;
                    }
                    unityClient.Activate();

                    await command.RespondAsync(
                        $"Found server for game {unityClient.GameName}. Creating necessary channels...."
                        );

                    //Channel Creation

                    var categoryChannel = 
                        await guild.CreateCategoryChannelAsync(
                            $"{unityClient.GameName} @{guildUser.DisplayName}"
                            );
                    await categoryChannel.AddPermissionOverwriteAsync(guild.EveryoneRole,
                        OverwritePermissions.DenyAll(categoryChannel));
                    await categoryChannel.AddPermissionOverwriteAsync(
                        command.User,
                        new OverwritePermissions(readMessageHistory: PermValue.Allow, sendMessages:PermValue.Allow)
                        );
                    var chatChannel = await guild.CreateTextChannelAsync(
                        $"chat",
                        tc =>
                        {
                            tc.CategoryId = categoryChannel.Id;
                        }
                        );
                    var voiceChannel = await guild.CreateVoiceChannelAsync(
                        "voice", 
                        vc => vc.CategoryId = categoryChannel.Id
                        );

                    //Save
                    Tuple<ulong, ulong> instanceID = new Tuple<ulong, ulong>(guild.Id, chatChannel.GuildId);
                    BotClient client = new BotClient(guild.Id, categoryChannel.Id, chatChannel.GuildId,guildUser.Id , unityClient);
                    _instances.Add(instanceID,client);

                    await command.Channel.SendMessageAsync($"{guildUser.DisplayName} is hosting a game session of \"{unityClient.GameName}\". Join him by using the \"/join @{guildUser.DisplayName}\" command!");

                    break;

                case Command.CLIENT_JOIN:
                    var rawHost = command.Data.Options.First().Value;

                    if (!(rawHost is SocketUser))
                    {
                        await command.RespondAsync("User not found!");
                        return;
                    }
                    var host = rawHost as SocketUser;
                    var guildHost = guild.GetUser(host.Id);

                    var instance =
                        _instances.FirstOrDefault(i =>
                            i.Key.Item1 == command.GuildId &&
                            i.Value.HostId == host.Id).Value;

                    if (instance == null)
                    {
                        await command.RespondAsync("User is not hosting any game!");
                        return;
                    }

                    await guild.GetCategoryChannel(instance.CategoryID)
                        .AddPermissionOverwriteAsync(
                            command.User,
                            new OverwritePermissions(
                                viewChannel: PermValue.Allow,
                                readMessageHistory: PermValue.Allow, 
                                sendMessages: PermValue.Allow
                                )
                        );

                    await command.RespondAsync($"You've successfully joined {guildHost.DisplayName}");

                    break;
            }
        }

    }
}
