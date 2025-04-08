using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace APG.Common.Packets.Types
{
    public class Screenshoot
    {
        public string Message { get; set; }

        [JsonIgnore]
        public byte[] ScreenshootData { get => screenshootData.Select(b => (byte)b).ToArray(); set => screenshootData = value.Select(b => (int)b).ToArray(); }
        [JsonProperty]
        private int[] screenshootData;

        public Screenshoot()
        {
            
        }

        public Screenshoot(string message,byte[] screenshootBytes)
        {
            Message = message;
            ScreenshootData = screenshootBytes;
        }
    }
}
