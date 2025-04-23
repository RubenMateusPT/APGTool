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
        Dictionary<Guid,Unity.UnityClient> clients = new Dictionary<Guid, UnityClient>();



        public ServerManager()
        {
            listener = new TcpListener(new IPEndPoint(IPAddress.Any, 8000));
        }

        public UnityClient GetClient(Guid code) => clients.ContainsKey(code) ? clients[code] : null;

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
                await ListenForConnections();
                ProcessReceive();
                ProcessSend();
                CheckClientStatus();
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }

        private async Task ListenForConnections()
        {
            while (listener.Pending())
            {
                var tcpClient = await listener.AcceptTcpClientAsync();
                var newClient = new Unity.UnityClient(tcpClient);

                clients.Add(newClient.Guid,newClient);

                Console.WriteLine($"New client connected from {tcpClient.Client.RemoteEndPoint}");
            }
        }

        private void ProcessReceive()
        {
            foreach (var client in clients.Values)
            {
                client.ProcessReceive();
            }
        }

        private void ProcessSend()
        {
            foreach (var client in clients.Values)
            {
                client.ProcessSend();
            }
        }

        /// <summary>
        /// Checks if all clients connected are still online, if not remove them
        /// </summary>
        private void CheckClientStatus()
        {
            foreach (var client in clients)
            {
                var status = client.Value.CheckStatus(TimeSpan.FromMilliseconds(10).TotalMilliseconds);
                if (status == UnityClient.Status.Disposed)
                    clients.Remove(client.Key);
            }
        }
    }
}
