using Discord.Commands;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Reflection;
using Discord.WebSocket;
using Discord;

namespace FluffyMusic.Core
{
    public partial class CommandHandler
    {
        public readonly CommandService CommandService;
        public readonly FluffyClient Client;

        private readonly ILogger<CommandHandler> _logger;
        private readonly IServiceProvider _services;

        public CommandHandler(CommandService commandService,
            FluffyClient client,
            ILogger<CommandHandler> logger,
            IServiceProvider services)
        {
            CommandService = commandService;
            Client = client;
            _logger = logger;
            _services = services;
            CommandService.CommandExecuted += CommandExecutedHandler;
            Client.Client.MessageReceived += OnClientMessageReceived;
        }

        private async Task CommandExecutedHandler(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (result.Error == CommandError.BadArgCount)
            {
                await context.Channel.SendInfoAsync(string.Format("Usage: **`{0}{1}`**.",
                 Client.Options.Prefix, CommandToString(command.Value)));
            }
        }

        private string CommandToString(CommandInfo command)
        {
            string result = command.Name + " ";

            foreach (var param in command.Parameters)
            {
                result += param.IsOptional
                ? string.Format("[{0}] ", param.Name)
                : string.Format("<{0}> ", param.Name);
            }

            return result.TrimEnd();
        }

        public async Task LoadModulesAsync(Assembly assembly)
        {
            await CommandService.AddModulesAsync(assembly, _services);
        }

        private async Task OnClientMessageReceived(SocketMessage message)
        {
            if (!(message is SocketUserMessage userMessage)) return;

            int argPos = 0;
            if (!userMessage.HasStringPrefix(Client.Options.Prefix, ref argPos) || userMessage.Author.IsBot) return;

            SocketCommandContext context = new SocketCommandContext(Client.Client, userMessage);
            IResult result = await CommandService.ExecuteAsync(context, argPos, _services);
        }
    }
}
