using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TimeSpan = System.TimeSpan;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    public TextMeshProUGUI serverIpText;
    public Image botStatus;

    private TcpListener server;
    private TcpClient client;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            server = new TcpListener(IPAddress.Parse("127.0.0.1"), 2525);
            serverIpText.text = $"Server IP: 127.0.0.1:2525";
            server.Start();
            Debug.Log($"APG Server started at: {server.LocalEndpoint}");
            WaitForClients();
        }
    }

    private async Task WaitForClients()
    {
        client = await server.AcceptTcpClientAsync();
        var stream = client.GetStream();
        Debug.Log("New client has connected");
        botStatus.color = Color.green;

        while (client.Connected)
        {
            if (stream.CanRead)
            {
                var bytes = new byte[client.Available];
                await stream.ReadAsync(bytes, 0, bytes.Length);
                var command = Encoding.UTF8.GetString(bytes);

                switch (command)
                {
                    case "ping":
                        Debug.Log("Bot asked for ping, sending pong");
                        var pong = Encoding.UTF8.GetBytes($"Pong");
                        await stream.WriteAsync(pong, 0, pong.Length);
                        break;
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50));
        }

        botStatus.color = Color.red;
        Debug.Log("Client has left");
    }
}
