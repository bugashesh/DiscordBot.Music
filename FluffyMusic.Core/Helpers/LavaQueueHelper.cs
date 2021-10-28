using Discord;
using System.Linq;
using System.Text;
using Victoria;
using FluffyMusic.Core.Pagination;

namespace FluffyMusic.Core.Helpers
{
    public static class LavaQueueHelper
    {
        public static Embed BuildQueueEmbed(LavaPlayer player, PageInfo info)
        {
            EmbedBuilder builder = new EmbedBuilder()
                    .WithTitle("Music queue")
                    .WithFooter(string.Format("Page {0} of {1} ({2} tracks of {3})",
                        info.CurrentPage, info.TotalPages, info.RecordsOnPage, info.TotalRecords))
                    .WithDescription(QueueToStringList(player, info));

            return builder.Build();
        }

        public static string QueueToStringList(LavaPlayer player, PageInfo info)
        {
            StringBuilder builder = new StringBuilder(
                string.Format("**Now playing: {0}({1})**\n", player.Track.Title, player.Track.Duration.ToString(@"mm\:ss")));

            if (player.Queue.Count == 0)
            {
                builder.Append("*Queue is empty.*");
            }
            else
            {
                for (int i = info.Start(); i < info.End(player.Queue.Count); i++)
                {
                    LavaTrack track = player.Queue.ElementAt(i);
                    builder.Append(
                        string.Format(" {0}. {1} ({2})\n", i + 1, track.Title, track.Duration.ToString(@"mm\:ss")));
                }
            }

            return builder.ToString();
        }
    }
}
