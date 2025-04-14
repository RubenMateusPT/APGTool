using APG.Common.Commands;
using APG.Common.Discord;

namespace APG.Common.Packets.Types
{
    public class GameCommand
    {
        public Command Command { get; set; }
        public DiscordUser DiscordUser { get; set; }
    }
}
