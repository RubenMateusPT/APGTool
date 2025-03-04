using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DiscordBot
{
    internal class RelayServer
    {
        private const int PORT = 8000;
        private UdpClient server = new UdpClient(PORT);
        public async Task Run()
        {
            while (true)
            {
                var rb = await server.ReceiveAsync();
                var mg = Encoding.ASCII.GetString(rb.Buffer);

                var packet = JsonConvert.DeserializeObject<Packet>(mg);
                var rawData = Encoding.ASCII.GetString(packet.Data);
                var data = JsonConvert.DeserializeObject<Info>(rawData);

                Console.WriteLine($"{data.IP}:{data.port}");

                var res = Encoding.ASCII.GetBytes("Hi");
                await server.SendAsync(res,res.Length,rb.RemoteEndPoint);
            }
        }


        public class Packet
        {
            public byte[] Data { get; set; }
        }

        public class Info
        {
            public int port { get; set; }
            public string IP { get; set; }
        }
    }
}
