using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TimeSpan = System.TimeSpan;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    public TextMeshProUGUI serverIpText;
    public Image botStatus;

    public TcpListener server;
    public TcpClient client;

    private bool canOpenDoor = false;
    private bool doorhasbeenopened = false;

    private bool canISendScreenshot = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            serverIpText.text = $"Server IP: 127.0.0.1:2525";

            Instance.serverIpText = this.serverIpText;
            Instance.botStatus = this.botStatus;
            Instance.canOpenDoor = false;
            Instance.doorhasbeenopened = false;

            Destroy(this);
        }
        else
        {
            Instance = this;
            canISendScreenshot = true;
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
        client.ReceiveBufferSize = int.MaxValue;
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
                        var pong = Encoding.UTF8.GetBytes($"pong");
                        await stream.WriteAsync(pong, 0, pong.Length);
                        break;

                    case "givelife":
                        FindFirstObjectByType<Player>().AddLife();
                        break;

                    case "slowtime":
                        FindFirstObjectByType<GameManager>().StartTimeSlow();
                        break;

                    case "opendoor":
                        if (canOpenDoor && !doorhasbeenopened)
                        {
                            doorhasbeenopened = true;
                            var denyDoor = Encoding.UTF8.GetBytes($"openDoor");
                            await stream.WriteAsync(denyDoor, 0, denyDoor.Length);
                            FindFirstObjectByType<GameManager>().OpenDoor();
                        }
                        else
                        {
                            var denyDoor = Encoding.UTF8.GetBytes($"denyDoor");
                            await stream.WriteAsync(denyDoor, 0, denyDoor.Length);
                        }
          
                        break;
                }
            }

            await Task.Delay(TimeSpan.FromMilliseconds(50));
        }

        botStatus.color = Color.red;
        Debug.Log("Client has left");
    }

    public async void SendDoorRequest()
    {
        canOpenDoor = true;

        if (client == null || !client.Connected)
        {
            FindFirstObjectByType<GameManager>().OpenDoor();
        }
        else
        {
            var gameCommand = "doorRequest";
            var buffer = Encoding.UTF8.GetBytes(gameCommand);
            await client.GetStream().WriteAsync(buffer, 0, buffer.Length);

            StartCoroutine(ScreenShot());
        }
    }

    public async void SendScreenshoot(bool won)
    {
        if (!canISendScreenshot)
            return;

        canISendScreenshot = false;

        var gameCommand = $"endgame:{(won ? 'y':'n')}";
        var buffer = Encoding.UTF8.GetBytes(gameCommand);
        await client.GetStream().WriteAsync(buffer, 0, buffer.Length);
        StopAllCoroutines();
        StartCoroutine(ScreenShot());
    }

    private IEnumerator ScreenShot()
    {
        yield return new WaitForEndOfFrame();
        var png = ScreenCapture.CaptureScreenshotAsTexture().EncodeToPNG();
        client.GetStream().Write(png, 0, png.Length);
        canISendScreenshot = true;
    }
}
