using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using Victoria;
using FluffyMusic.Core.Pagination;
using FluffyMusic.Core.Options;
using Microsoft.Extensions.Options;

namespace FluffyMusic.Core
{
    public partial class FluffyClient
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger<FluffyClient> _logger;
        private readonly LavaNode _lavaNode;

        public readonly BotOptions Options;
        public DiscordSocketClient Client => _client;
        public readonly Dictionary<ulong, PageInfo> Pages;

        public FluffyClient(
            DiscordSocketClient client,
            LavaNode lavaNode,
            ILogger<FluffyClient> logger,
            IOptions<BotOptions> options)
        {
            _client = client;
            _logger = logger;
            _lavaNode = lavaNode;
            Options = options.Value;
            Pages = new Dictionary<ulong, PageInfo>();

            _client.Log += OnClientLog;
        }

        public async Task RunAsync()
        {
            await _client.LoginAsync(TokenType.Bot, Options.Token);
            await _client.StartAsync();
        }

        private Task OnClientLog(LogMessage log)
        {
            switch (log.Severity)
            {
                case LogSeverity.Error:
                    _logger.LogError(log.Message);
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(log.Message);
                    break;
                case LogSeverity.Critical:
                    _logger.LogCritical(log.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(log.Message);
                    break;
                default:
                case LogSeverity.Info:
                    _logger.LogInformation(log.Message);
                    break;
            }
            return Task.CompletedTask;
        }
    }
}
