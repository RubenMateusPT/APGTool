using APG.Discord.Server;
using APG.Server.Discord;
using Microsoft.Extensions.Configuration;

namespace APG.Discord
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine($"Pre configuring server...");
            var builder = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();
            var botToken = builder.GetSection("BotToken").Value;
            Console.WriteLine($"Got all settings correctly");

            Console.WriteLine("Starting Server...");
            var server = new ServerManager();
            server.Start();
            Console.WriteLine("Server started");

            Console.WriteLine("Starting Discord Bot...");
            var bot = new DiscordBot(botToken, server);
            bot.Start();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
    }
}
