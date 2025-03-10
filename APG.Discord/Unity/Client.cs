using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using APG.Common.Packets;
using APG.Common.Packets.Types;
using Newtonsoft.Json.Linq;

namespace APG.Discord.Unity
{
    internal class Client
    {
        private TcpClient _tcp;
        private NetworkStream _stream;
        private byte[] _streamBuffer = new byte[PacketManager.MAX_BUFFER_SIZE];
        private DateTime _lastPing;

        private PacketManager _packetManager = new PacketManager();

        public Guid Guid { get; private set; }

        public Client(TcpClient tcpClient)
        {
            _tcp = tcpClient;
            _stream = tcpClient.GetStream();

            _lastPing = DateTime.UtcNow;
            Guid = Guid.NewGuid();

            var data = _packetManager.PackPacket(new Packet(new CodeRequest()));
        }


        public async void Receive()
        {
            while (_stream.DataAvailable)
            {
                int bytes = await _stream.ReadAsync(_streamBuffer);
                _packetManager.UnpackPacket(_streamBuffer,bytes);
            }
        }

        public async void Send()
        {

        }
    }
}
