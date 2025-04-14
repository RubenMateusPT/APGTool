using System;
using System.Linq;
using Newtonsoft.Json;

namespace APG.Common.Packets.Types
{
    public class SplitPacket
    {
        [JsonIgnore]
        public Type DataType { get => typeof(Packet).Assembly.GetType(dataTypeName); set => dataTypeName = value.FullName; }
        [JsonProperty]
        private string dataTypeName;

        [JsonIgnore]
        public byte[] Chunk { get => chunk.Select(x => (byte)x).ToArray(); set => chunk = value.Select(x => (int)x).ToArray(); }

        [JsonProperty] private int[] chunk;

        public SplitPacket()
        {
            
        }

        public SplitPacket(Type dataType, byte[] chunk)
        {
            DataType = dataType;
            Chunk = chunk;
        }
    }
}
