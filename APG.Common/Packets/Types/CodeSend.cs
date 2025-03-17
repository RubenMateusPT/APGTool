using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace APG.Common.Packets.Types
{
    public class CodeSend
    {
        [JsonIgnore]
        public Guid ID { get => Guid.Parse(idString); set => idString = value.ToString(); }
        [JsonProperty]
        private string idString;
    }
}
