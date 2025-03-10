using System;
using System.Collections;
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

        var data = _packetManager.PackPacket(new Packet(new CodeRequest()));
        Debug.Log(data.Count);
        foreach (var b in data)
        {
            await _stream.WriteAsync(b);
        }
    }

    private async void Update()
    {
        if (_stream == null)
            return;

        if (_stream.DataAvailable)
        {
            while (_stream.DataAvailable)
            {
                int received = await _stream.ReadAsync(_streamBuffer);
                Debug.Log(Encoding.ASCII.GetString(_streamBuffer, 0, received));
            }
        }
    }
}
