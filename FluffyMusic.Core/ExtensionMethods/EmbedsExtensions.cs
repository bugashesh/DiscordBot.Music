using System.Threading.Tasks;

namespace Discord.WebSocket
{
    public static class EmbedsExtensions
    {
        public static Task<IUserMessage> SendErrorAsync(this IMessageChannel textChannel, string content)
        {
            Embed embed = new EmbedBuilder()
                .WithColor(Colors.Danger)
                .WithTitle("Error")
                .WithDescription(":x: " + content)
                .Build();
            return textChannel.SendMessageAsync(embed: embed);
        }

        public static Task<IUserMessage> SendInfoAsync(this IMessageChannel textChannel, string content)
        {
            Embed embed = new EmbedBuilder()
                .WithColor(Colors.Primary)
                .WithTitle("Information")
                .WithDescription(":information_source: " + content)
                .Build();
            return textChannel.SendMessageAsync(embed: embed);
        }

        public static Task<IUserMessage> SendWarningAsync(this IMessageChannel textChannel, string content)
        {
            Embed embed = new EmbedBuilder()
                .WithColor(Colors.Warning)
                .WithTitle("Warning")
                .WithDescription(":warning: " + content)
                .Build();
            return textChannel.SendMessageAsync(embed: embed);
        }

        public static Task<IUserMessage> SendSuccessAsync(this IMessageChannel textChannel, string content)
        {
            Embed embed = new EmbedBuilder()
                .WithColor(Colors.Success)
                .WithTitle("Success")
                .WithDescription(":white_check_mark: " + content)
                .Build();
            return textChannel.SendMessageAsync(embed: embed);
        }
    }
}
