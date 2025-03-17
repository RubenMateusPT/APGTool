using APG.Discord.Server;

namespace APG.Discord
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Server...");
            new ServerManager().Start();
            Console.WriteLine("Server started");

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
    }
}
