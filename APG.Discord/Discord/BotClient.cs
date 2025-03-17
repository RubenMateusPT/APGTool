using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APG.Discord.Unity;

namespace APG.Server.Discord
{
    internal class BotClient
    {
        public ulong GuildID { get; private set; }
        public ulong CategoryID { get; private set; }
        public ulong ChatID { get; private set; }
        public ulong HostId { get; private set; }
        public UnityClient Unity { get; private set; }

        public BotClient(ulong guildID, ulong categoryId, ulong chatId,ulong hostId ,UnityClient unity)
        {
            GuildID = guildID;
            CategoryID = categoryId;
            ChatID = chatId;
            HostId = hostId;
            Unity = unity;
        }
    }
}
