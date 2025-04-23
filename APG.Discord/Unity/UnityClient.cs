using System.Net.Sockets;
using APG.Common.Commands;
using APG.Common.Packets;
using APG.Common.Packets.Types;
using APG.Server.Discord;

namespace APG.Discord.Unity
{
    internal class UnityClient
    {
        public event Action<UnityClient,Status> OnStatusChange = (unity,status) => {}; 

        internal enum Status
        {
            WaitingForHost,
            Connected,
            Closing,
            Closed,
            Disposed
        }

        private PacketManager _packetManager = new PacketManager();

        private TcpClient _tcp;
        private NetworkStream _stream;
        //private byte[] _streamBuffer = new byte[PacketManager.MAX_BUFFER_SIZE];

        private Status _currentStatus;
        public Status CurrentStatus
        {
            get
            {
                return _currentStatus;
            }
            set
            {
                OnStatusChange.Invoke(this,value);
                _currentStatus = value;
            }
        }

        private double _waitCounter = 0;
        private double _maxWaitTime = TimeSpan.FromSeconds(30).TotalMilliseconds;
        private Guid _pingId = Guid.Empty;

        private bool _isReceiving = false;
        public bool IsReceiving => _isReceiving;
        private bool _isSending = false;
        public bool IsSending => _isSending;

        Queue<Packet> _sendPacketsQueue = new Queue<Packet>();

        //Client Info
        public string GameName { get; private set; }
        public char CommandDelimiter { get; private set; }
        public Command[] Commands { get; private set; }

        public Guid Guid { get; private set; }

        //Discord Info
        private BotClient discordBot = null;
        public BotClient DiscordBot => discordBot;

        public UnityClient(TcpClient tcpClient)
        {
            _tcp = tcpClient;
            _stream = tcpClient.GetStream();
            _stream.Flush();

            Guid = Guid.NewGuid();

            CurrentStatus = Status.WaitingForHost;
        }

        /// <summary>
        /// Tells this Unity client is activated on the server (Got a host)
        /// </summary>
        public void Activate()
        {
            Console.WriteLine($"{_tcp.Client.RemoteEndPoint} has a Owner now!");
            _waitCounter = 0;
            CurrentStatus = Status.Connected;
        }

        public void RegisterBot(BotClient bot)
        {
            discordBot = bot;
        }

        public async void ProcessReceive()
        {
            if(CurrentStatus >= Status.Closing)
                return;

            if (_isReceiving)
                return;

            _isReceiving = true;

            var buffer = new Memory<byte>(new byte[PacketManager.MAX_BUFFER_SIZE]);

            while (_stream.DataAvailable)
            {
                int bytes = await _stream.ReadAtLeastAsync(buffer, PacketManager.MAX_BUFFER_SIZE);

                try
                {
                    var packet = _packetManager.UnpackPacket(buffer.ToArray(), bytes);
                    if (packet != null)
                        ProcessPacket(packet);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error unpacking packed: {ex.Message}");
                    Console.WriteLine($"Flushing stream of {_tcp.Client.RemoteEndPoint}");

                    await _stream.FlushAsync();
                } 
            }

            _isReceiving = false;
        }

        private async void ProcessPacket(Packet packet)
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
                    CommandDelimiter = codeRequest.CommandDelimiter;
                    Commands = codeRequest.Commands;

                    Send(new CodeSend{ID = this.Guid});
                    break;

                case nameof(Screenshoot):
                    var screenShoot = packet.GetData<Screenshoot>();
                    await discordBot.SendScreenshoot(screenShoot);
                    break;

                case nameof(Common.Packets.Types.CloseConnection):
                    await CloseConnection();
                    break;
            }
        }

        public async void ProcessSend()
        {
            if (CurrentStatus > Status.Closing)
                return;

            if (_isSending)
                return;

            _isSending = true;

            if (CurrentStatus == Status.Closing)
            {
                if (_tcp.Connected && _sendPacketsQueue.Count > 0)
                {
                    var closingPacket = _sendPacketsQueue.FirstOrDefault(p => p.DataType == typeof(HostConnect) || p.DataType == typeof(APG.Common.Packets.Types.CloseConnection));
                    if (closingPacket != null)
                    {
                        try
                        {
                            if (_tcp.Connected)
                                await _stream.WriteAsync(_packetManager.PackPacket(closingPacket).First());
                        }
                        catch{}
                    }
                }

                CurrentStatus = Status.Closed;
            }
            else
            {
                while (_sendPacketsQueue.Count > 0)
                {
                    var packetToSend = _sendPacketsQueue.Dequeue();
                    var packets = _packetManager.PackPacket(packetToSend);
                    foreach (var packet in packets)
                    {
                        try
                        {
                            if (_tcp.Connected)
                                await _stream.WriteAsync(packet);
                        }
                        catch
                        {
                            CurrentStatus = Status.Closed;
                        }
                    }
                }
            }

            _isSending = false;
        }

        public void Send<T>(T data)
        {
            _sendPacketsQueue.Enqueue(new Packet(data));
        }

        public Status CheckStatus(double dt)
        {
            switch (CurrentStatus)
            {
                case Status.WaitingForHost:
                    _waitCounter += dt;
                    if (_waitCounter > _maxWaitTime)
                    {
                        CurrentStatus = Status.Closing;
                        Send(new HostConnect(false));
                    }
                    break;

                case Status.Connected:
                    if (_tcp.Connected)
                    {
                        _waitCounter += dt;

                        if (_waitCounter > _maxWaitTime) //Makes sure host is still connected
                        {
                            if (_pingId == Guid.Empty)
                            {
                                Console.WriteLine("Pinging game");
                                _waitCounter = 0;
                                var ping = new Ping();
                                _pingId = ping.ID;
                                Send(ping);
                            }
                            else
                            {
                                CurrentStatus = Status.Closed;
                            }
                        }
                    }
                    else
                    {
                        CurrentStatus = Status.Closed;
                    }
                    break;

                case Status.Closed:
                    CurrentStatus = Status.Disposed;
                    Console.WriteLine($"Client Disconnected from {_tcp.Client.RemoteEndPoint}");
                    _stream.Close();
                    _tcp.Close();
                    break;
            }

            return CurrentStatus;
        }

        public async Task CloseConnection()
        {
            CurrentStatus = Status.Closing;
            Send(new CloseConnection(){Reason = "Terminated By Server"});
            while (CurrentStatus == Status.Closing)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

    }
}
