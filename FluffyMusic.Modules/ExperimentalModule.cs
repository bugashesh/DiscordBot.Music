using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace FluffyMusic.Modules
{
    [Group("expr")]
    public class ExperimentalModule : ModuleBase<SocketCommandContext>
    {
        [Command("buttons")]
        public async Task ButtonsCommandAsync()
        {
            ComponentBuilder builder = new ComponentBuilder()
                .WithButton("A", "test_A")
                .WithButton("B", "test_B");
        }
    }
}
