using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using APG.Common.Packets;
using APG.Common.Packets.Types;
using APG.Unity.Sample.UI;
using APG.Unity.ScriptableObjects;
using UnityEngine;
using UnityEngine.Events;
using Ping = APG.Common.Packets.Types.Ping;

namespace APG.Unity.Managers
{
    public class NetworkManager : MonoBehaviour
    {
        [SerializeField] private SettingsScriptableObject settings;

        private TcpClient _tcp;
        private NetworkStream _stream;

        private byte[] _streamBuffer = new byte[PacketManager.MAX_BUFFER_SIZE];
        private int totalReceived = 0;

        private PacketManager _packetManager = new PacketManager();
        private Queue<Packet> _sendPacketsQueue = new Queue<Packet>();

        [SerializeField] 
        private UnityEvent<string> OnStatusChange = new UnityEvent<string>();

        [SerializeField] 
        private UnityEvent<string> OnCodeReceive = new UnityEvent<string>();

        public bool IsApgEnabled { get; private set; } = false;
        private APGManager _currentAPGManager = null;

        private void Awake()
        {
            Application.runInBackground = true;
            DontDestroyOnLoad(this.gameObject);
            IsApgEnabled = false;
        }

        private async void Update()
        {
            if (_stream == null)
                return;

            if (_stream.DataAvailable)
                await ProcessReceive();

            if (_sendPacketsQueue.Count > 0)
                await ProcessSend();
        }

        private async Task ProcessReceive()
        {
            var buffer = new byte[PacketManager.MAX_BUFFER_SIZE];

            while (_stream.DataAvailable)
            {
                int received = await _stream.ReadAsync(buffer, 0, PacketManager.MAX_BUFFER_SIZE - totalReceived);

                if (received == PacketManager.MAX_BUFFER_SIZE ||
                    totalReceived == PacketManager.MAX_BUFFER_SIZE) // No issues on packet
                {
                    totalReceived = 0;

                    Packet packet = null;
                    try
                    {
                        packet = _packetManager.UnpackPacket(buffer, PacketManager.MAX_BUFFER_SIZE);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Oops Invalid Packet - {ex.Message}");
                        _stream.Flush();
                        Send(new Pong { ID = Guid.NewGuid() });
                    }

                    if (packet != null)
                        ProcessPacket(packet);
                }
                else if (received < PacketManager.MAX_BUFFER_SIZE) // Issues on packet
                {
                    //Debug.LogWarning("Stream Buffer isn't full yet!");
                    Array.Copy(buffer, 0, _streamBuffer, totalReceived, received);
                    totalReceived += received;

                    if (totalReceived == PacketManager.MAX_BUFFER_SIZE)
                    {
                        //Debug.Log("Packet is splitted");
                        totalReceived = 0;
                        Packet packet = null;
                        try
                        {
                            packet = _packetManager.UnpackPacket(_streamBuffer, PacketManager.MAX_BUFFER_SIZE);

                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Oops Invalid Packet - {ex.Message}");
                            _stream.Flush();
                            Send(new Pong { ID = Guid.NewGuid() });
                        }

                        if (packet != null)
                            ProcessPacket(packet);
                    }
                }
                else
                {
                    Debug.Log($"Something really bad happened, Flushing Network Stream");
                    totalReceived = 0;
                    _stream.Flush();
                    Send(new Pong { ID = Guid.NewGuid() });
                }
            }
        }

        private void ProcessPacket(Packet packet)
        {
            switch (packet.DataType.Name)
            {
                case nameof(Ping):
                    var ping = packet.GetData<Ping>();
                    Send(new Pong { ID = ping.ID });
                    break;

                case nameof(CodeSend):
                    var codeSend = packet.GetData<CodeSend>();
                    OnStatusChange.Invoke($"Got host code!\n Waiting for host to connect...");
                    OnCodeReceive.Invoke(codeSend.ID.ToString());
                    break;


                case nameof(HostConnect):
                    var hostConnect = packet.GetData<HostConnect>();
                    if (hostConnect.Success)
                    {
                        OnStatusChange.Invoke("Host connected!\n You can start the game now!");
                        IsApgEnabled = true;
                    }
                    else
                    {
                        OnStatusChange.Invoke("Timeout! Failed to connect in time!");
                        OnCodeReceive.Invoke(String.Empty);
                        IsApgEnabled = true;
                    }
                    break;

                case nameof(CloseConnection):
                    var closeConnection = packet.GetData<CloseConnection>();
                    IsApgEnabled = false;
                    OnStatusChange.Invoke(closeConnection.Reason);
                    break;

                case nameof(GameCommand):

                    if (_currentAPGManager == null)
                        return;

                    var gameCommand = packet.GetData<GameCommand>();
                    _currentAPGManager.ExecuteCommand(gameCommand);
                    break;
            }
        }

        private async Task ProcessSend()
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

        public async void Connect()
        {
            OnStatusChange.Invoke("Connecting to Discord Bot @ 127.0.0.1:8000");
            await Task.Delay(TimeSpan.FromSeconds(1));
            _tcp = new TcpClient();
            try
            {
                await _tcp.ConnectAsync(settings.IP, settings.Port);
            }
            catch (Exception ex)
            {
                OnStatusChange.Invoke("Unable to reach server. Is it online?\nTry Again...");
                OnCodeReceive.Invoke(String.Empty);
                return;
            }

            if (!_tcp.Connected)
            {
                OnStatusChange.Invoke("Failed to connect to server");
                OnCodeReceive.Invoke(String.Empty);
                return;
            }

            OnStatusChange.Invoke("Connection Successfully");
            _stream = _tcp.GetStream();
            _stream.Flush();

            OnStatusChange.Invoke("Requesting connection id...");
            Send(new CodeRequest
            {
                GameName = settings.GameName,
                CommandDelimiter = settings.CommandDelimiter,
                Commands = settings.Commands
            });
        }

        public void Send<T>(T data)
        {
            _sendPacketsQueue.Enqueue(new Packet(data));
        }

        public void RegisterSceneManager(APGManager apgManager)
        {
            _currentAPGManager = apgManager;
        }

        private void OnApplicationQuit()
        {
            if (!IsApgEnabled)
                return;

            //Debug.LogWarning("Closing Network Connection");
            try
            {
                _stream.Write(_packetManager
                    .PackPacket(new Packet(new CloseConnection() { Reason = "Host Closed game" })).First());
            }
            catch{}
        }
    }
}
