using APG.Discord.Server;

namespace APG.Discord
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            new ServerManager().Start();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
    }
}
