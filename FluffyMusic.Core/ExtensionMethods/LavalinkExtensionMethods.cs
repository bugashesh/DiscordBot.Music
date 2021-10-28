using Discord;
using System.Threading.Tasks;

namespace Victoria
{
    public static class LavalinkExtensionMethods
    {
        public static async Task<Embed> ToDiscordEmbed(this LavaTrack track, string title, Color color)
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle(title)
                .WithColor(color)
                .WithThumbnailUrl(await track.FetchArtworkAsync())
                .WithDescription(string.Format("[{0}]({1})", track.Title, track.Url))
                .AddField("Author", track.Author, true)
                .AddField("Duration", track.Duration.ToString("mm\\:ss"), true);

            return builder.Build();
        }
    }
}
