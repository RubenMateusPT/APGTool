using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using APG.Common.Packets;
using APG.Common.Packets.Types;
using Newtonsoft.Json.Linq;
using Timer = System.Timers.Timer;

namespace APG.Discord.Unity
{
    internal class UnityClient
    {
        internal enum Status
        {
            WaitingForHost,
            Connected,
            Close,
            Disposed
        }

        private PacketManager _packetManager = new PacketManager();

        private TcpClient _tcp;
        private NetworkStream _stream;
        private byte[] _streamBuffer = new byte[PacketManager.MAX_BUFFER_SIZE];

        private Status _currentStatus;
        private double _waitCounter = 0;
        private double _maxWaitTime = TimeSpan.FromSeconds(10).TotalMilliseconds;
        private Guid _pingId = Guid.Empty;

        Queue<Packet> _sendPacketsQueue = new Queue<Packet>();

        //Client Info
        public string GameName { get; private set; }

        public Guid Guid { get; private set; }

        public UnityClient(TcpClient tcpClient)
        {
            _tcp = tcpClient;
            _stream = tcpClient.GetStream();

            Guid = Guid.NewGuid();

            _currentStatus = Status.WaitingForHost;
        }

        public void Activate()
        {
            Console.WriteLine($"{_tcp.Client.RemoteEndPoint} has a Owner now!");
            _waitCounter = 0;
            _currentStatus = Status.Connected;
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
        }

        private void ProcessPacket(Packet packet)
        {
            _waitCounter = 0;
            if(_pingId != Guid.Empty)
                _pingId = Guid.Empty;

            switch (packet.DataType.Name)
            {
                case nameof(Ping):
                    var ping = packet.GetData<Ping>();
                    Send(new Pong { ID = ping.ID });
                    break;

                case nameof(Pong):
                    var pong = packet.GetData<Pong>();
                    break;

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

        public Status CheckStatus(double dt)
        {
            switch (_currentStatus)
            {
                case Status.WaitingForHost:
                    _waitCounter += dt;
                    if (_waitCounter > _maxWaitTime)
                       _currentStatus = Status.Close;

                    break;

                case Status.Connected:
                    if (_tcp.Connected)
                    {
                        _waitCounter += dt;

                        if (_waitCounter > _maxWaitTime)
                        {
                            if (_pingId == Guid.Empty)
                            {
                                _waitCounter = 0;
                                var ping = new Ping();
                                _pingId = ping.ID;
                                Send(ping);
                            }
                            else
                            {
                                _currentStatus = Status.Close;
                            }
                        }
                    }
                    else
                    {
                        _currentStatus = Status.Close;
                    }
                    break;

                case Status.Close:
                    _currentStatus = Status.Disposed;
                    Console.WriteLine($"Client Disconnected from {_tcp.Client.RemoteEndPoint}");
                    _tcp.Close();
                    break;
            }

            return _currentStatus;
        }

    }
}
