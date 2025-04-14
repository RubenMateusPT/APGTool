using System;
using Newtonsoft.Json;

namespace APG.Common.Packets.Types
{
    public class Pong
    {
        [JsonIgnore]
        public Guid ID { get => Guid.Parse(idString); set => idString = value.ToString(); }
        [JsonProperty]
        private string idString;
    }
}
