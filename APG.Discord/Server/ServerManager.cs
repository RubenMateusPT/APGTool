using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using APG.Discord.Unity;

namespace APG.Discord.Server
{
    internal class ServerManager
    {
        TcpListener listener;
        Dictionary<Guid,Unity.Client> clients = new Dictionary<Guid, Client>();

        public ServerManager()
        {
            listener = new TcpListener(new IPEndPoint(IPAddress.Any, 8000));
        }

        public void Start()
        {
            Console.WriteLine($"Starting Listening at {listener.LocalEndpoint}");
            listener.Start();

            Update();
        }

        private async void Update()
        {
            while (true)
            {
                ListenForConnections();
                ProcessReceive();
                ProcessSend();
                await Task.Delay(10);
            }
        }

        private async void ListenForConnections()
        {
            while (listener.Pending())
            {
                var tcpClient = await listener.AcceptTcpClientAsync();
                var newClient = new Unity.Client(tcpClient);
                clients.Add(newClient.Guid,newClient);

                Console.WriteLine($"New client connected from {tcpClient.Client.RemoteEndPoint}");
            }
        }

        private void ProcessReceive()
        {
            foreach (var client in clients.Values)
            {
                client.Receive();
            }
        }

        private void ProcessSend()
        {
            foreach (var client in clients.Values)
            {
                //client.Send();
            }
        }
    }
}
