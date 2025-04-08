using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APG.Common.Discord;
using APG.Common.Packets.Types;
using APG.Discord.Unity;
using Discord;
using Discord.WebSocket;

namespace APG.Server.Discord
{
    internal class BotClient
    {
        public ulong GuildID { get; private set; }
        public ulong CategoryID { get; private set; }
        public ulong ChatID { get; private set; }
        public ulong HostId { get; private set; }
        public Dictionary<ulong, DiscordUser> Spectators { get; private set; }

        public DiscordBot Discord { get; private set; }
        public UnityClient Unity { get; private set; }

        public BotClient(ulong guildID, ulong categoryId, ulong chatId,ulong hostId ,DiscordBot discord,UnityClient unity)
        {
            GuildID = guildID;
            CategoryID = categoryId;
            ChatID = chatId;
            
            HostId = hostId;
            Spectators = new Dictionary<ulong, DiscordUser>();

            Discord = discord;
            Unity = unity;
            Unity.RegisterBot(this);
        }

        public async Task SendScreenshoot(Screenshoot screenshoot)
        {
            var textChannel = Discord.Client.GetGuild(GuildID).GetTextChannel(ChatID);

            using (var ms = new MemoryStream(screenshoot.ScreenshootData))
            {
                using (var png = new Image(ms))
                {
                    await textChannel.SendFileAsync(png.Stream,$"{Guid.NewGuid().ToString()}.png", screenshoot.Message);
                }
            }
        }
    }
}
