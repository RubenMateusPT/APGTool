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
        }


        public async void Receive()
        {
            while (_stream.DataAvailable)
            {
                int bytes = await _stream.ReadAsync(_streamBuffer);
                var packet = _packetManager.UnpackPacket(_streamBuffer,bytes);
                if(packet != null)
                    ProcessPacket(packet);
            }
        }

        private void ProcessPacket(Packet packet)
        {
            switch (packet.DataType.Name)
            {
                case nameof(CodeRequest):
                    var codeRequest = packet.GetData<CodeRequest>();
                    Send(new Packet(new CodeSend()));
                    break;
            }
        }

        public async void Send(Packet packet)
        {
            var data = _packetManager.PackPacket(packet);
            foreach (var bytes in data)
            {
                await _stream.WriteAsync(bytes);
            }
        }

    }
}
