using FluffyMusic.Core;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Serilog;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Victoria;
using FluffyMusic.Core.Options;

namespace FluffyMusic.ConsoleUI
{
    class Program
    {
        private static IConfiguration _configuration;

        public static async Task Main()
        {
            _configuration = CreateConfigurationBuilder().Build();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            IHost host = CreateHostBuilder().Build();

            FluffyClient client = host.Services.GetRequiredService<FluffyClient>();
            CommandHandler handler = host.Services.GetRequiredService<CommandHandler>();
            await client.RunAsync();
            await handler.LoadModulesAsync(Assembly.Load("FluffyMusic.Modules"));
            await Task.Delay(-1);
        }

        public static IConfigurationBuilder CreateConfigurationBuilder() =>
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile("appsettings.Development.json", false, true)
                .AddEnvironmentVariables();

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    LavaConfig lavaCfg = new LavaConfig();
                    _configuration.GetSection("Lavalink").Bind(lavaCfg);
                    services
                    .Configure<AudioOptions>(_configuration.GetSection("Audio"))
                    .Configure<BotOptions>(_configuration.GetSection("Bot"))
                    .AddSingleton(lavaCfg)
                    .AddSingleton<DiscordSocketClient>()
                    .AddSingleton<CommandService>()
                    .AddSingleton<LavaNode>()
                    .AddSingleton<FluffyClient>()
                    .AddSingleton<CommandHandler>()
                    .AddSingleton<AudioService>()
                    .BuildServiceProvider();
                })
                .UseSerilog();
    }
}
