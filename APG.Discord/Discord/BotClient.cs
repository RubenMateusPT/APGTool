using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APG.Common.Discord;
using APG.Discord.Unity;
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
        public UnityClient Unity { get; private set; }

        public BotClient(ulong guildID, ulong categoryId, ulong chatId,ulong hostId ,UnityClient unity)
        {
            GuildID = guildID;
            CategoryID = categoryId;
            ChatID = chatId;
            
            HostId = hostId;
            Spectators = new Dictionary<ulong, DiscordUser>();


            Unity = unity;
        }
    }
}
