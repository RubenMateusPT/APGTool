using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using APG.Common;
using UnityEngine;
using Random = UnityEngine.Random;
using Newtonsoft.Json;

public class Test : MonoBehaviour
{
    TcpListener listener = new TcpListener(IPAddress.Any, 9000);

    UdpClient udpClient = new UdpClient();


    void Start()
    {
        var ep = listener.LocalEndpoint.ToString().Split(':');
        var serverInfo = new Info()
        {
            IP = ep[0],
            port = int.Parse(ep[1])
        };

        var json = JsonConvert.SerializeObject(serverInfo);

        Debug.Log(json);
        var encoded = Encoding.ASCII.GetBytes(json);

        var packet = new Packet()
        {
            Data = encoded
        };

        var jsonPacket = JsonConvert.SerializeObject(packet);

        Byte[] sb = Encoding.ASCII.GetBytes(jsonPacket);
        udpClient.Send(sb, sb.Length, "127.0.0.1", 8000);

        var ea = new IPEndPoint(IPAddress.Any, 0);
        var res = udpClient.Receive(ref ea);
        Debug.Log(Encoding.ASCII.GetString(res));
    }
}
