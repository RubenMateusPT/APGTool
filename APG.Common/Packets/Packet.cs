using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using APG.Common.Packets.Types;
using Newtonsoft.Json;

namespace APG.Common.Packets
{
    public class Packet
    {
        [JsonIgnore]
        public Guid ID { get => Guid.Parse(idString); set => idString = value.ToString(); }
        [JsonProperty]
        private string idString;

        public int Size { get; set; }
        public int PartNumber { get; set; }

        [JsonIgnore]
        public Type DataType { get => typeof(Packet).Assembly.GetType(dataTypeName); set => dataTypeName = value.FullName; }
        [JsonProperty]
        private string dataTypeName;
        [JsonIgnore] 
        public object Data { set => dataJson = JsonConvert.SerializeObject(value); }
        [JsonProperty]
        private string dataJson;

        public Packet(){}

        public Packet(object data)
        {
            ID = Guid.NewGuid();
            Size = 1;
            PartNumber = 1;
            DataType = data.GetType();
            Data = data;
        }

        public Packet(Guid id, object data)
        {
            ID = id;
            Size = 1;
            PartNumber = 1;
            DataType = data.GetType();
            Data = data;
        }

        public Packet(Guid id, int size, int partNumber, SplitPacket splitPacket)
        {
            ID = id;
            Size = size;
            PartNumber = partNumber;
            DataType = typeof(SplitPacket);
            Data = splitPacket;
        }

        public bool IsSplitPacket() => Size > 1;
        public T GetData<T>() => JsonConvert.DeserializeObject<T>(dataJson);

        public byte[] GetDataBytes()
        {
            return Encoding.ASCII.GetBytes(dataJson);
        }

        public byte[] Pack()
        {
            var json = JsonConvert.SerializeObject(this);
            return Encoding.ASCII.GetBytes(json);
        }

        public static Packet Unpack(byte[] buffer, int bytes)
        {
            var json = Encoding.ASCII.GetString(buffer, 0, bytes);
            var packet = JsonConvert.DeserializeObject<Packet>(json);

            return packet;
        }
    }
}
