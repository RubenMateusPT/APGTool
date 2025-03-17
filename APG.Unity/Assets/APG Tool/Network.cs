using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using APG.Common.Packets;
using APG.Common.Packets.Types;
using UnityEngine;

public class Network : MonoBehaviour
{
    TcpClient _tcp;
    NetworkStream _stream;
    private byte[] _streamBuffer = new byte[PacketManager.MAX_BUFFER_SIZE];
    PacketManager _packetManager = new PacketManager();
    Queue<Packet> _sendPacketsQueue = new Queue<Packet>();


    private async void Start()
    {
        Debug.Log("Connecting to Discord Bot @ 127.0.0.1:8000");
        _tcp = new TcpClient();
        await _tcp.ConnectAsync("127.0.0.1", 8000);

        if (!_tcp.Connected)
        {
            Debug.LogError("Failed to connect to server");
            return;
        }

        Debug.Log("Connection Successfully");
        _stream = _tcp.GetStream();

        Debug.Log("Requesting connection id...");
        Send(new CodeRequest
        {
            GameName = "Super Game"
        });
    }

    private async void Update()
    {
        if (_stream == null)
            return;

        if (_stream.DataAvailable)
            ProcessReceive();

        if(_sendPacketsQueue.Count > 0)
            ProcessSend();

    }

    private async void ProcessReceive()
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
            case nameof(CodeSend):
                var codeSend = packet.GetData<CodeSend>();
                Debug.Log($"Got Code {codeSend.ID}");
                break;
        }
    }

    private async void ProcessSend()
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
}
