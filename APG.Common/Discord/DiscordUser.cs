using System.Linq;
using Newtonsoft.Json;

namespace APG.Common.Discord
{
    public class DiscordUser
    {
        public string Username { get;  set; }

        [JsonIgnore]
        public byte[] ImageBytes { get => imageBytes.Select(x => (byte)x).ToArray(); set => imageBytes = value.Select(x => (int)x).ToArray(); }
        [JsonProperty] private int[] imageBytes;

        public DiscordUser()
        {

        }

        public DiscordUser(string username, byte[] imageBytes)
        {
            Username = username;
            ImageBytes = imageBytes;
        }
    }
}
