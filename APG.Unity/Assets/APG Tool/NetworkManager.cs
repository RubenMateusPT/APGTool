using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using APG.Common.Packets;
using APG.Common.Packets.Types;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ping = APG.Common.Packets.Types.Ping;

public class NetworkManager : MonoBehaviour
{
    private TcpClient _tcp;
    private NetworkStream _stream;
    private byte[] _streamBuffer = new byte[PacketManager.MAX_BUFFER_SIZE];
    private PacketManager _packetManager = new PacketManager();
    private Queue<Packet> _sendPacketsQueue = new Queue<Packet>();

    public Action<string> OnStatusChange;

    public bool IsApgEnabled { get; private set; } = false;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private async void Update()
    {
        if (_stream == null)
            return;

        if (_stream.DataAvailable)
            await ProcessReceive();

        if(_sendPacketsQueue.Count > 0)
            await ProcessSend();
    }

    private async Task ProcessReceive()
    {
        while (_stream.DataAvailable)
        {
            int received = await _stream.ReadAsync(_streamBuffer);
            Packet packet = _packetManager.UnpackPacket(_streamBuffer, received);

            if (packet != null)
                ProcessPacket(packet);
        }
    }

    private void ProcessPacket(Packet packet)
    {
        switch (packet.DataType.Name)
        {
            case nameof(Ping):
                var ping = packet.GetData<Ping>();
                Send(new Pong{ID = ping.ID});
                break;

            case nameof(CodeSend):
                var codeSend = packet.GetData<CodeSend>();
                OnStatusChange.Invoke($"Got host code!\n Waiting for host to connect...");
                FindFirstObjectByType<MainMenuUI>().codeField.text = codeSend.ID.ToString();
                break;

            case nameof(HostConnect):
                OnStatusChange.Invoke("Host connected!\n You can start the game now!");
                IsApgEnabled = true;
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
        _tcp = new TcpClient();
        await _tcp.ConnectAsync("127.0.0.1", 8000);

        if (!_tcp.Connected)
        {
            OnStatusChange.Invoke("Failed to connect to server");
            return;
        }

        OnStatusChange.Invoke("Connection Successfully");
        _stream = _tcp.GetStream();

        OnStatusChange.Invoke("Requesting connection id...");
        Send(new CodeRequest
        {
            GameName = "Super Game"
        });
    }

    public void Send<T>(T data)
    {
        _sendPacketsQueue.Enqueue(new Packet(data));
    }
}
