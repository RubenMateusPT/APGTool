using System.Net.Sockets;
using System.Text;
using APG.Common;
using Newtonsoft.Json;

namespace APG.Discord
{
    internal class RelayServer
    {
        private const int PORT = 8000;
        private UdpClient server = new UdpClient(PORT);

        public async Task Run()
        {
            while (true)
            {
                Console.WriteLine("\n\nWaiting for messange\n\n");
                var rb = await server.ReceiveAsync();
                var mg = Encoding.ASCII.GetString(rb.Buffer);

                var packet = JsonConvert.DeserializeObject<Packet>(mg);
                var rawData = Encoding.ASCII.GetString(packet.Data);
                var data = JsonConvert.DeserializeObject<Info>(rawData);

                Console.WriteLine($"{data.IP}:{data.port}");

                var res = Encoding.ASCII.GetBytes("Hi");
                await server.SendAsync(res, res.Length, rb.RemoteEndPoint);
            }
        }
    }
}
