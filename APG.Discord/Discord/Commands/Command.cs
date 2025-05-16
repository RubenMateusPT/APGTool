using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APG.Server.Discord.Commands
{
    /// <summary>
    /// Available Discord Bot Commands
    /// </summary>
    internal class Command
    {
        //Server Related
        public const string DELETE_SERVER = "nuke";
        public const string DELETE_CATEGORY = "delete-category";

        public const string CLEAR_GAME_CHAT = "clear-game-chat";
        public const string KICK_ADGS = "kick-adgs";

        //Game Related
        public const string HOST_JOIN = "create";
        public const string CLIENT_JOIN = "join";
        public const string SEND_GAME_COMMAND = "command";

    }
}
