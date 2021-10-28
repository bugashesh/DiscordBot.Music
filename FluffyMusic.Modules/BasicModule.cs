using Discord;
using Discord.Commands;
using FluffyMusic.Core;
using System.Text;
using System.Threading.Tasks;

namespace FluffyMusic.Modules
{
    public class BasicModule : ModuleBase<SocketCommandContext>
    {
        private readonly FluffyClient _client;
        private readonly CommandHandler _commands;

        public BasicModule(FluffyClient client, CommandHandler commands)
        {
            _client = client;
            _commands = commands;
        }

        [Command("help")]
        [Alias("?", "h")]
        [Summary("Shows the list of all available commands.")]
        public async Task HelpCommandAsync()
        {
            EmbedBuilder embedBuilder = new EmbedBuilder()
               .WithTitle("Command list")
               .WithDescription(CommandsToStringList());

            await ReplyAsync(embed: embedBuilder.Build());
        }

        private string CommandsToStringList()
        {
            StringBuilder stringBuilder = new StringBuilder();
            int listCounter = 1;
            foreach (CommandInfo cmd in _commands.CommandService.Commands)
            {
                string line = string.Format("{0}. `{1}{2} {3}` - {4}\n",
                    listCounter,
                    _client.Options.Prefix,
                    cmd.Name,
                    CommandArgsToString(cmd),
                    cmd.Summary ?? "*no summary provided*");
                stringBuilder.Append(line);
                listCounter++;
            }
            return stringBuilder.ToString();
        }

        private string CommandArgsToString(CommandInfo command)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var param in command.Parameters)
            {
                string paramString = param.IsOptional
                    ? string.Format("[{0}] ", param.Name)
                    : string.Format("<{0}> ", param.Name);
                stringBuilder.Append(paramString);
            }
            return stringBuilder.ToString().TrimEnd();
        }
    }
}
