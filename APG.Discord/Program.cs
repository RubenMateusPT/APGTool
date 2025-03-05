namespace APG.Discord
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var relayServer = new RelayServer();

            relayServer.Run();


            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
    }
}
