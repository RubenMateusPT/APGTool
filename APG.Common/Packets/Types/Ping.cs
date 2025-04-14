using System;
using Newtonsoft.Json;

namespace APG.Common.Packets.Types
{
    public class Ping
    {
        [JsonIgnore]
        public Guid ID { get => Guid.Parse(idString); set => idString = value.ToString(); }
        [JsonProperty]
        private string idString;

        public Ping()
        {
            ID = Guid.NewGuid();
        }
    }
}
