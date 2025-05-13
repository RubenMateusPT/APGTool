using System.Net;
using APG.Common.Discord;
using APG.Common.Packets.Types;
using APG.Discord.Server;
using APG.Discord.Unity;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using Command = APG.Server.Discord.Commands.Command;
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
            //This bit of code removes any old Discord Bot Commands from showing up on Discord
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
                case Command.DELETE_SERVER: //Removes all Discord Channels, Removes the Unity Instance from the server and create a new Main Channel

                    foreach (var channel in guild.Channels)
                    {
                        await channel.DeleteAsync();
                    }

                    foreach (var instance in _instances.Where(i => i.Key.Item1 == guild.Id))
                    {
                        await instance.Value.Delete();
                    }

                    await guild.CreateTextChannelAsync("main");

                    foreach (var user in guild.Users)
                    {
                        if (user.Id != 156159600159096832 &&
                            user.Id != 1351077479351062593 &&
                            !user.IsBot)
                        {
                            await user.KickAsync(
                                "Abertay Digital Graduate Show Demostrations is over. Thank you for participating!");
                        }
                    }

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

            //Write Discord Message with all the available game commands
            string commandsList = "```\nAvailable Commands\n" +
                                  $"Param Delimiter: {unityClient.CommandDelimiter}\n\n";
            foreach (var unityCommand in unityClient.Commands)
            {
                string commandText = $"- /{Command.SEND_GAME_COMMAND} {unityCommand.Name}\n";

                if (unityCommand.Parameters.Length > 0)
                {
                    int i = 1;
                    foreach (var parameter in unityCommand.Parameters)
                    {
                        commandText += $" - Param {i}:\n" +
                                       $"  - Name: {parameter.Name}\n";
                        commandText += $"   - Type: {parameter.Type.ToString()}\n" +
                                       $"   - Is Required:{parameter.IsRequired}\n";
                        i++;
                    }
                }

                commandText += "\n";
                commandsList += commandText;
            }

            commandsList += "```";
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

            //Create and Save this as a new Instance Of Discord <-> Unity Connection
            Tuple<ulong, ulong> instanceID = new Tuple<ulong, ulong>(guild.Id, chatChannel.Id);
            BotClient client = new BotClient(guild.Id, categoryChannel.Id, chatChannel.Id, guildUser.Id, this, unityClient);
            unityClient.OnStatusChange += UnityClient_OnStatusChange;
            _instances.Add(instanceID, client);

            await command.Channel.SendMessageAsync($"{guildUser.DisplayName} is hosting a game session of \"{unityClient.GameName}\". Join him by using the \"/join @{guildUser.DisplayName}\" command!");

            unityClient.Send(new HostConnect(true));
        }

        /// <summary>
        /// What to do when an Instance Status Changes
        /// </summary>
        /// <param name="unityClient"></param>
        /// <param name="status"></param>
        private async void UnityClient_OnStatusChange(UnityClient unityClient, UnityClient.Status status)
        {
            if (status == UnityClient.Status.Disposed)
            {
                var botClient = unityClient.DiscordBot;
                _instances.Remove(new Tuple<ulong, ulong>(botClient.GuildID, botClient.ChatID));

                var guild = _client.GetGuild(botClient.GuildID);
                try
                {
                    var categoryChannel = guild.GetCategoryChannel(botClient.CategoryID);
                    foreach (var spectator in botClient.Spectators.Keys)
                    {
                        await categoryChannel.RemovePermissionOverwriteAsync(guild.GetUser(spectator));
                    }

                    await categoryChannel.RemovePermissionOverwriteAsync(guild.GetUser(botClient.HostId));
                }
                catch { }

                unityClient.OnStatusChange -= UnityClient_OnStatusChange;
            }
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
                    i.Value.HostId == host.Id &&
                    i.Value.Unity.CurrentStatus == UnityClient.Status.Connected).Value;

            if (instance == null)
            {
                await command.RespondAsync("User is not hosting any game!");
                return;
            }

            //Add this User to the list of spectators of the game, save its discord image
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

            //Give the correct permission to the user
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

        /// <summary>
        /// Send Command from Discord To Unity
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
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

            if (!client.Spectators.ContainsKey(command.User.Id))
            {
                await command.RespondAsync("You have to join the host first! Before you can start interacting with it!");
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

            for (int i = 0; i < splitCommand.Length; i++)
            {
                var sc = splitCommand[i];
                sc = sc.TrimStart();
                sc = sc.TrimEnd();
                splitCommand[i] = sc;
            }

            var unityCommand = unity.Commands.FirstOrDefault(c => c.Name.ToUpperInvariant() == splitCommand[0].ToUpperInvariant(), null);

            if (unityCommand == null)
            {
                await command.RespondAsync("No command found for game. Is name correctly written?");
                return;
            }

            if (splitCommand.Length - 1 < unityCommand.Parameters.Where(p => p.IsRequired).Count())
            {
                await command.RespondAsync("Not enough parameters!");
                return;
            }

            if (unityCommand.HasCooldown)
            {
                if (!unityCommand.HasFinishedCooldown(out var timeRemaining))
                {
                    await command.RespondAsync($"Sorry command is in cooldown! Time Remaining: {((int)timeRemaining) + 1}");
                    return;
                }
            }

            for (int i = 0; i < unityCommand.Parameters.Length; i++)
            {
                var sc = string.Empty;
                var pm = unityCommand.Parameters[i];

                if (pm.IsRequired) //This parameter is required!
                {
                    if (i + 1 >= splitCommand.Length) // Parameter has not given
                    {
                        await command.RespondAsync($"Missing Required Parameter: {pm.Name}");
                        return;
                    }
                    else if (string.IsNullOrEmpty(splitCommand[i + 1])) // Parameter is empty
                    {
                        await command.RespondAsync($"Missing Required Parameter: {pm.Name}");
                        return;
                    }
                    else 
                        sc = splitCommand[i + 1];
                }
                else //This parameter is not required
                {
                    if (i + 1 >= splitCommand.Length) //Parameter not given
                        sc = pm.DefaultValue;
                    else if (string.IsNullOrEmpty(splitCommand[i + 1])) //Empty parameter
                        sc = pm.DefaultValue;
                    else
                        sc = splitCommand[i + 1];
                }


                if (!pm.IsSameType(sc))
                {
                    await command.RespondAsync($"Invalid Parameter! Parameter {pm.Name} needs to be of type {pm.Type.ToString()}");
                    return;
                }

                unityCommand.Parameters[i].Value = sc;
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

            var instance = _instances[instanceKey];

            return instance;
        }
    }
}
