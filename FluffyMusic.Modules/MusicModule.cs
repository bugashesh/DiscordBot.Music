using System.Threading.Tasks;
using System;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Rest;
using Microsoft.Extensions.Logging;
using FluffyMusic.Core;
using FluffyMusic.Core.Pagination;
using FluffyMusic.Core.Helpers;
using FluffyMusic.Core.ExtensionMethods;

namespace FluffyMusic.Modules
{
    public sealed class MusicModule : ModuleBase<SocketCommandContext>
    {
        private readonly AudioService _service;
        private readonly ILogger<MusicModule> _logger;
        private static readonly Color successColor = new Color(0, 204, 68);

        public MusicModule(AudioService service, ILogger<MusicModule> logger)
        {
            _service = service;
            _logger = logger;
        }

        private async Task RunPaginationAsync(LavaPlayer player)
        {
            PageInfo info = new PageInfo(player.Queue.Count, _service.Options.TracksPerPage, Context.User.Id);

            ComponentBuilder builder = new ComponentBuilder();
            if (info.TotalPages >= _service.Options.MinPagesForSkips)
            {
                builder.WithButton(PaginationButtons.Start, PaginationButtons.StartId, ButtonStyle.Success);
            }
            builder.WithButton(PaginationButtons.Prev, PaginationButtons.PrevId, ButtonStyle.Primary);
            builder.WithButton(PaginationButtons.Close, PaginationButtons.CloseId, ButtonStyle.Danger);
            builder.WithButton(PaginationButtons.Next, PaginationButtons.NextId, ButtonStyle.Primary);
            if (info.TotalPages >= _service.Options.MinPagesForSkips)
            {
                    builder.WithButton(PaginationButtons.End, PaginationButtons.EndId, ButtonStyle.Success);
            }

            var message = await ReplyAsync(embed: LavaQueueHelper.BuildQueueEmbed(player, info), component: builder.Build());
            _service.Pages.Add(message.Id, info);
            await Task.Delay(_service.Options.PaginationResetDelay * 1000);
            if (_service.Pages.ContainsKey(message.Id))
            {
                _service.Pages.Remove(message.Id);
                await message.DeleteAsync();
            }
        }

        [Command("queue", RunMode = RunMode.Async)]
        [Alias("q")]
        [Summary("Displays current queue.")]
        public async Task QueueCommandAsync()
        {
            if (await IsPlayerInTheSameChannelAsync())
            {
                LavaPlayer player = _service.LavaNode.GetPlayer(Context.Guild);
                if (player.Track == null)
                {
                    await Context.Channel.SendErrorAsync("I'm not playing any music on this server.");
                }

                Task paginationTask = new(action: async () =>
                {
                    await RunPaginationAsync(player);
                });

                paginationTask.Start();
            }
        }

        [Command("join")]
        [Alias("connect", "j", "summon")]
        [Summary("Summons bot to your voice channel.")]
        public async Task JoinCommandAsync()
        {
            IVoiceState voiceState = Context.User as IVoiceState;

            if (voiceState?.VoiceChannel == null)
            {
                await Context.Channel.SendErrorAsync("You must be in a voice channel in order to summon the bot.");
                return;
            }

            if (_service.LavaNode.HasPlayer(Context.Guild))
            {
                if (_service.LavaNode.GetPlayer(Context.Guild).VoiceChannel.Id !=
                    voiceState.VoiceChannel.Id)
                {
                    await Context.Channel.SendErrorAsync("I'm already connected **to another** voice channel.");
                }
                else
                {
                    await Context.Channel.SendErrorAsync("I'm already connected **to your** voice channel.");
                }
                return;
            }

            try
            {
                await _service.LavaNode.JoinAsync(voiceState.VoiceChannel);
                await Context.Channel.SendInfoAsync($"I'm joined the channel **:loud_sound: {voiceState.VoiceChannel.Name}**.");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occured while connecting to the voice channel:");
                _logger.LogError(ex.Message);
                await Context.Channel.SendErrorAsync("An error occurred while executing this command.");
            }
            return;
        }

        [Command("skip")]
        [Summary("Skips current track.")]
        public async Task SkipCommandAsync()
        {
            if (await IsPlayerInTheSameChannelAsync())
            {
                LavaPlayer player = _service.LavaNode.GetPlayer(Context.Guild);
                await player.SeekAsync(player.Track.Duration);
                await Context.Channel.SendSuccessAsync("Track is skipped.");
            }
        }

        [Command("stop")]
        [Summary("Stops music playback completely and clears the queue.")]
        public async Task StopCommandAsync()
        {
            if (await IsPlayerInTheSameChannelAsync())
            {
                LavaPlayer player = _service.LavaNode.GetPlayer(Context.Guild);
                await player.StopAsync();
                player.Queue.Clear();
                await Context.Channel.SendSuccessAsync("Music playback has been completely stopped.");
            }
        }

        [Command("pause")]
        [Summary("Pauses current track.")]
        public async Task PauseCommandAsync()
        {
            if (await IsPlayerInTheSameChannelAsync())
            {
                LavaPlayer player = _service.LavaNode.GetPlayer(Context.Guild);
                if (player.PlayerState == PlayerState.Playing)
                {
                    await player.PauseAsync();
                    await Context.Channel.SendSuccessAsync("Track has been paused.");
                }
                else
                {
                    await Context.Channel.SendErrorAsync("I'm not playing any music on this server.");
                }
            }
        }

        [Command("resume")]
        [Summary("Resumes current track.")]
        public async Task ResumeCommandAsync()
        {
            if (await IsPlayerInTheSameChannelAsync())
            {
                LavaPlayer player = _service.LavaNode.GetPlayer(Context.Guild);
                if (player.PlayerState == PlayerState.Paused)
                {
                    await player.ResumeAsync();
                    await Context.Channel.SendSuccessAsync("Track has been resumed.");
                }
                else
                {
                    await Context.Channel.SendErrorAsync("The music playback hasn't been stopped.");
                }
            }
        }

        [Command("nowplaying")]
        [Alias("np")]
        [Summary("Displays the information about current track.")]
        public async Task NowPlayingCommandAsync()
        {
            if (await IsPlayerInTheSameChannelAsync())
            {
                LavaTrack track = _service.LavaNode.GetPlayer(Context.Guild).Track;

                Embed info = new EmbedBuilder()
                    .WithTitle("Information about current track")
                    .WithThumbnailUrl(await track.FetchArtworkAsync())
                    .WithColor(successColor)
                    .WithDescription($"[{track.Title}]({track.Url})")
                    .AddField("Channel", track.Author, true)
                    .AddField("Duration", track.Duration.ToString(@"mm\:ss"))
                    .Build();

                await ReplyAsync(embed: info);
            }
        }

        [Command("play", RunMode = RunMode.Async)]
        [Alias("p")]
        [Summary("Searches and plays the track specified by the query or link.")]
        public async Task PlayCommandAsync([Remainder] string query)
        {
            bool isInSameChannel = await JoinAsync();
            if (isInSameChannel)
            {
                LavaPlayer player = _service.LavaNode.GetPlayer(Context.Guild);
                SearchResponse response = await _service.LavaNode.SearchYouTubeAsync(query);
                if (response.LoadStatus == LoadStatus.NoMatches)
                {
                    response = await _service.LavaNode.SearchAsync(query);
                }

                if (response.LoadStatus == LoadStatus.LoadFailed)
                {
                    await Context.Channel.SendErrorAsync("Something went wrong while loading the track.");
                    return;
                }

                if (response.LoadStatus == LoadStatus.NoMatches)
                {
                    await Context.Channel.SendErrorAsync("There are no results for your query.");
                    return;
                }

                switch (response.LoadStatus)
                {
                    case LoadStatus.TrackLoaded:
                    case LoadStatus.SearchResult:
                        LavaTrack track = response.Tracks[0];
                        if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
                        {
                            await player.PlayAsync(track);
                        }
                        else
                        {
                            player.Queue.Enqueue(track);
                            Embed embed = await BuildEmbedTrackInfoAsync(track);
                            await ReplyAsync(embed: embed);
                        }
                        break;
                    case LoadStatus.PlaylistLoaded:
                        if (player.PlayerState != PlayerState.Playing && player.PlayerState != PlayerState.Paused)
                        {
                            await player.PlayAsync(response.Tracks[0]);
                            for (int i = 1; i < response.Tracks.Count; i++)
                            {
                                player.Queue.Enqueue(response.Tracks[i]);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < response.Tracks.Count; i++)
                            {
                                player.Queue.Enqueue(response.Tracks[i]);
                            }
                        }
                        await Context.Channel.SendSuccessAsync($"**{Context.User}** added **{response.Playlist.Name}** playlist to queue ({response.Tracks.Count} tracks).");
                        break;
                }
            }
            else
            {
                await Context.Channel.SendErrorAsync("I'm already connected **to another** voice channel.");
            }
        }

        private async Task<Embed> BuildEmbedTrackInfoAsync(LavaTrack track)
        {
            return new EmbedBuilder()
                .WithThumbnailUrl(await track.FetchArtworkAsync())
                .WithColor(successColor)
                .WithAuthor(Context.User.ToString() + " added to queue:", Context.User.GetAvatarUrl())
                .WithDescription($"[{track.Title}]({track.Url})")
                .AddField("Channel", track.Author, true)
                .AddField("Duration", track.Duration.ToString(@"mm\:ss"))
                .Build();
        }

        private async Task<bool> JoinAsync()
        {
            IVoiceState voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel != null && !_service.LavaNode.HasPlayer(Context.Guild))
            {
                await _service.LavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            }

            return voiceState.VoiceChannel != null &&
                voiceState?.VoiceChannel.Id == _service.LavaNode.GetPlayer(Context.Guild).VoiceChannel.Id;
        }

        private async Task<bool> IsPlayerInTheSameChannelAsync()
        {
            if (!_service.LavaNode.HasPlayer(Context.Guild) || _service.LavaNode.GetPlayer(Context.Guild).Track == null)
            {
                await Context.Channel.SendErrorAsync("I'm not playing any music on this server.");
                return false;
            }

            IVoiceState voiceState = Context.User as IVoiceState;
            if (_service.LavaNode.GetPlayer(Context.Guild).VoiceChannel.Id != voiceState.VoiceChannel.Id)
            {
                await Context.Channel.SendErrorAsync("I'm already connected **to another** voice channel.");
                return false;
            }
            return true;
        }
    }
}
