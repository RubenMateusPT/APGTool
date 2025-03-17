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
    internal class UnityClient
    {
        private PacketManager _packetManager = new PacketManager();

        private TcpClient _tcp;
        private NetworkStream _stream;
        private byte[] _streamBuffer = new byte[PacketManager.MAX_BUFFER_SIZE];
        private DateTime _lastPing;

        Queue<Packet> _sendPacketsQueue = new Queue<Packet>();

        //Client Info
        public string GameName { get; private set; }

        public Guid Guid { get; private set; }

        public UnityClient(TcpClient tcpClient)
        {
            _tcp = tcpClient;
            _stream = tcpClient.GetStream();

            _lastPing = DateTime.UtcNow;
            Guid = Guid.NewGuid();
        }


        public async void ProcessReceive()
        {
            while (_stream.DataAvailable)
            {
                int bytes = await _stream.ReadAsync(_streamBuffer);
                var packet = _packetManager.UnpackPacket(_streamBuffer,bytes);
                if(packet != null)
                    ProcessPacket(packet);
            }

            _lastPing = DateTime.Now;
        }

        private void ProcessPacket(Packet packet)
        {
            switch (packet.DataType.Name)
            {
                case nameof(CodeRequest):
                    var codeRequest = packet.GetData<CodeRequest>();
                    
                    GameName = codeRequest.GameName;

                    Send(new CodeSend{ID = this.Guid});
                    break;
            }
        }

        public async void ProcessSend()
        {
            while (_sendPacketsQueue.Count > 0)
            {
                var packetToSend = _sendPacketsQueue.Dequeue();
                var packets = _packetManager.PackPacket(packetToSend);
                foreach (var packet in packets)
                {
                    await _stream.WriteAsync(packet);
                }
            }
        }

        public void Send<T>(T data)
        {
            _sendPacketsQueue.Enqueue(new Packet(data));
        }

        public void CheckStatus()
        {

        }

    }
}
