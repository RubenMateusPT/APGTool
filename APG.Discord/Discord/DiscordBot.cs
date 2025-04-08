using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using APG.Common.Discord;
using APG.Common.Packets.Types;
using APG.Discord.Server;
using APG.Server.Discord.Commands;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using Image = System.Drawing.Image;

namespace APG.Server.Discord
{
    internal class DiscordBot
    {
        private ServerManager _serverManager;

        public DiscordSocketClient Client { get => _client;}
        private DiscordSocketClient _client;
        private string _token;

        private Dictionary<Tuple<ulong, ulong>, BotClient>
            _instances = new Dictionary<Tuple<ulong, ulong>, BotClient>();

        //Users (should be its own class)
        private string AVATAR_BASE_FOLDER;

        public DiscordBot(string token, ServerManager serverManager)
        {
            _serverManager = serverManager;

            _client = new DiscordSocketClient();
            _token = token;

            _client.Log += Log;
            _client.Ready += Ready;
            _client.SlashCommandExecuted += SlashCommandHandler;

            AVATAR_BASE_FOLDER = $"{Environment.CurrentDirectory}/Avatars";
            if (!Directory.Exists(AVATAR_BASE_FOLDER))
                Directory.CreateDirectory(AVATAR_BASE_FOLDER);
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
            //var cms = await _client.GetGlobalApplicationCommandsAsync();
            //foreach (var c in cms)
            //{
            //    await c.DeleteAsync();
            //}

            List<SlashCommandBuilder> commands = new List<SlashCommandBuilder>
            {
                new SlashCommandBuilder()
                    .WithName(Command.DELETE_SERVER)
                    .WithDescription("Cleans up server")
                    .WithDefaultMemberPermissions(GuildPermission.Administrator),
                new SlashCommandBuilder()
                    .WithName(Command.DELETE_CATEGORY)
                    .WithDescription("Removes Category and its children")
                    .WithDefaultMemberPermissions(GuildPermission.Administrator),

                new SlashCommandBuilder()
                    .WithName(Command.HOST_JOIN)
                    .WithDescription("Host Join")
                    .AddOption("code",ApplicationCommandOptionType.String, "Host Code", isRequired:true),
                new SlashCommandBuilder()
                    .WithName(Command.CLIENT_JOIN)
                    .WithDescription($"Client Join")
                    .AddOption("user", ApplicationCommandOptionType.User, "Host" ,isRequired:true),
                new SlashCommandBuilder()
                    .WithName(Command.SEND_GAME_COMMAND)
                    .WithDescription("Sends a command to the game")
                    .AddOption("commnad",ApplicationCommandOptionType.String, "the command to send",isRequired:true)
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

            switch (command.Data.Name)
            {
                case Command.DELETE_SERVER:
                    foreach (var channel in guild.Channels)
                    {
                        await channel.DeleteAsync();
                    }

                    await guild.CreateTextChannelAsync("main");

                    break;

                case Command.DELETE_CATEGORY:
                    var client = GetClient(command);
                    if (client != null)
                    {
                        var category = guild.GetCategoryChannel(client.CategoryID);

                        foreach (var channel in category.Channels)
                        {
                            await channel.DeleteAsync();
                        }

                        await category.DeleteAsync();
                    }

                    break;

                case Command.HOST_JOIN:
                    await HostJoin(command);
                    break;

                case Command.CLIENT_JOIN:
                    await ClientJoin(command);
                    break;

                case Command.SEND_GAME_COMMAND:
                    await SendGameCommand(command);
                    break;
            }
        }

        private async Task HostJoin(SocketSlashCommand command)
        {
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
            var guild = _client.GetGuild(command.GuildId.Value);
            var guildUser = guild.GetUser(command.User.Id);

            var categoryChannel =
                await guild.CreateCategoryChannelAsync(
                    $"{unityClient.GameName} @{guildUser.DisplayName}"
                );
            await categoryChannel.AddPermissionOverwriteAsync(guild.EveryoneRole,
                OverwritePermissions.DenyAll(categoryChannel));

            var chatChannel = await guild.CreateTextChannelAsync(
                $"chat",
                tc =>
                {
                    tc.CategoryId = categoryChannel.Id;
                }
            );

            string commandsList = string.Empty;
            foreach (var unityCommand in unityClient.Commands)
            {
                commandsList += $"/{Command.SEND_GAME_COMMAND} {unityCommand.Name}\n";
            }
            var commandsMessage = await chatChannel.SendMessageAsync(commandsList);
            await commandsMessage.PinAsync();

            var voiceChannel = await guild.CreateVoiceChannelAsync(
                "voice",
                vc => vc.CategoryId = categoryChannel.Id
            );

            await categoryChannel.AddPermissionOverwriteAsync(
                command.User,
                new OverwritePermissions(
                    viewChannel: PermValue.Allow,
                    readMessageHistory: PermValue.Allow,
                    sendMessages: PermValue.Allow,
                    useApplicationCommands: PermValue.Allow
                    )
            );

            //Save
            Tuple<ulong, ulong> instanceID = new Tuple<ulong, ulong>(guild.Id, chatChannel.Id);
            BotClient client = new BotClient(guild.Id, categoryChannel.Id, chatChannel.Id, guildUser.Id, this, unityClient);
            _instances.Add(instanceID, client);

            await command.Channel.SendMessageAsync($"{guildUser.DisplayName} is hosting a game session of \"{unityClient.GameName}\". Join him by using the \"/join @{guildUser.DisplayName}\" command!");

            unityClient.Send(new HostConnect());
        }
        private async Task ClientJoin(SocketSlashCommand command)
        {
            var guild = _client.GetGuild(command.GuildId.Value);
            var guildUser = guild.GetUser(command.User.Id);

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

            var spectators = instance.Spectators;
            if (!spectators.ContainsKey(command.User.Id))
            {
                string avatarImagePath = $"{AVATAR_BASE_FOLDER}/{guildUser.DisplayAvatarId}.png";
                if (!File.Exists(avatarImagePath))
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.DownloadFile(guildUser.GetDisplayAvatarUrl(ImageFormat.Png, 64), avatarImagePath);
                    }
                }
                
                System.Drawing.Image img = Image.FromFile(avatarImagePath);
                byte[] imgData = Array.Empty<byte>();
                using (var ms = new MemoryStream())
                {
                    img.Save(ms,System.Drawing.Imaging.ImageFormat.Png);
                    imgData = ms.ToArray();
                }

                spectators.Add(command.User.Id, new DiscordUser(guildUser.DisplayName, imgData));
            }

            await guild.GetCategoryChannel(instance.CategoryID)
                .AddPermissionOverwriteAsync(
                    command.User,
                    new OverwritePermissions(
                        viewChannel: PermValue.Allow,
                        readMessageHistory: PermValue.Allow,
                        sendMessages: PermValue.Allow,
                        useApplicationCommands: PermValue.Allow
                    )
                );

            await command.RespondAsync($"You've successfully joined {guildHost.DisplayName}");

        }

        private async Task SendGameCommand(SocketSlashCommand command)
        {
            var client = GetClient(command);

            if (client == null)
            {
                await command.RespondAsync("Uh Oh! Game not found!");
                return;
            }

            if (client.HostId == command.User.Id)
            {
                await command.RespondAsync("No cheating host!");
                return;
            }

            var unity = client.Unity;

            var rawCommand = command.Data.Options.First().Value as String;
            var splitCommand = rawCommand.Split(unity.CommandDelimiter);

            if (splitCommand.Length == 0)
            {
                await command.RespondAsync("No game command given!");
                return;
            }

            var unityCommand = unity.Commands.FirstOrDefault(c => c.Name.ToUpperInvariant() == splitCommand[0].ToUpperInvariant(), null);

            if (unityCommand == null)
            {
                await command.RespondAsync("No command found for game. Is it correctly written?");
                return;
            }

            var user = client.Spectators[command.User.Id];
            unity.Send(new GameCommand
            {
                Command = unityCommand,
                DiscordUser = user
            });
            await command.RespondAsync($"Executing command: {unityCommand.Name}");
        }

        private BotClient GetClient(SocketSlashCommand command)
        {
            var instanceKey = new Tuple<ulong, ulong>(command.GuildId.Value, command.ChannelId.Value);

            if (!_instances.ContainsKey(instanceKey))
                return null;

            return _instances[instanceKey];
        }
    }
}
