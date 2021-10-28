using Discord;
using Discord.WebSocket;
using FluffyMusic.Core.Helpers;
using FluffyMusic.Core.Pagination;
using System.Threading.Tasks;
using System;
using Victoria;
using Victoria.EventArgs;
using Victoria.Enums;
using Microsoft.Extensions.Logging;
using FluffyMusic.Core.Options;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace FluffyMusic.Core
{
    public class AudioService
    {
        public readonly LavaNode LavaNode;
        public readonly FluffyClient Client;
        public readonly Dictionary<ulong, PageInfo> Pages;
        public readonly AudioOptions Options;
        public readonly ILogger<AudioService> _logger;

        public AudioService(LavaNode lavaNode,
            FluffyClient client,
            IOptions<AudioOptions> options,
            ILogger<AudioService> logger)
        {
            Pages = new Dictionary<ulong, PageInfo>();
            LavaNode = lavaNode;
            Client = client;
            _logger = logger;
            Options = options.Value;

            LavaNode.OnTrackEnded += OnTrackEnded;
            LavaNode.OnTrackStarted += OnTrackStarted;

            Client.Client.InteractionCreated += OnQueueInteraction;
            Client.Client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            Client.Client.Ready += async () => await ConnectToLavalink();
        }

        private async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            IGuildUser guildUser = user as IGuildUser;

            if(user is null || !LavaNode.HasPlayer(guildUser.Guild))
            {
                return;
            }
            
            LavaPlayer player = LavaNode.GetPlayer(guildUser.Guild);
            var users = await player
                            .VoiceChannel
                            .GetUsersAsync()
                            .FlattenAsync()
                            .ConfigureAwait(false);

            if (player.PlayerState == PlayerState.Playing &&
                player.VoiceChannel?.Id == oldState.VoiceChannel?.Id &&
                player.VoiceChannel?.Id != newState.VoiceChannel?.Id &&
                users.Count() == 1) 
            {
                var message = await player.TextChannel.SendWarningAsync("All users left the voice channel. Playback has been paused.");
                await player.PauseAsync();
                Task messageTask = new(() =>
                {
                    Task.Delay(Client.Options.RemoveNotificationDelay * 1000).Wait();
                    message.DeleteAsync();
                });
                messageTask.Start();
            }
            else if(player.PlayerState == PlayerState.Paused &&
                    player.VoiceChannel?.Id != oldState.VoiceChannel?.Id &&
                    player.VoiceChannel?.Id == newState.VoiceChannel?.Id &&
                    users.Count() == 2
                )
            {
                var message = await player.TextChannel.SendInfoAsync("Playback has been resumed.");
                await player.ResumeAsync();
                Task messageTask = new(() =>
                {
                    Task.Delay(Client.Options.RemoveNotificationDelay * 1000).Wait();
                    message.DeleteAsync();
                });
                messageTask.Start();
            }
        }

        private async Task OnQueueInteraction(SocketInteraction interaction)
        {
            var button = interaction as SocketMessageComponent;
            if (button != null & button.Type == InteractionType.MessageComponent)
            {
                if (Pages.TryGetValue(button.Message.Id, out PageInfo info))
                {
                    if(button.User.Id != info.MessageOwnerId)
                    {
                        return;
                    }

                    if (button.Message.Channel is IGuildChannel channel)
                    {
                        LavaPlayer player = LavaNode.GetPlayer(channel.Guild);
                        if(player == null)
                        {
                            return;
                        }

                        info.UpdateInfo(player.Queue.Count);

                        switch (button.Data.CustomId)
                        {
                            case PaginationButtons.StartId:
                                info.ToStart();
                                break;

                            case PaginationButtons.EndId:
                                info.ToEnd();
                                break;

                            case PaginationButtons.NextId:
                                if (!info.PageUp())
                                {
                                    return;
                                }
                                break;

                            case PaginationButtons.PrevId:
                                if (!info.PageDown())
                                {
                                    return;
                                }
                                break;

                            case PaginationButtons.CloseId:
                                await button.Message.DeleteAsync();
                                Pages.Remove(button.Message.Id);
                                return;

                            default: break;
                        }

                        await button.Message.ModifyAsync(p => p.Embed = LavaQueueHelper.BuildQueueEmbed(player, info));
                    }
                }
            }
        }

        private async Task OnTrackStarted(TrackStartEventArgs e)
        {
            if (e.Player.Queue.Count == 0 && e.Player.Track == null)
            {
                return;
            }

            ITextChannel channel = e.Player.TextChannel;
            IEnumerable<IMessage> messages = await channel.GetMessagesAsync(1).FlattenAsync().ConfigureAwait(false);
            IUserMessage message = messages.First() as IUserMessage;
            Embed messageEmbed = await e.Player.Track.ToDiscordEmbed("Now playing:", Colors.Primary);
            if (message.Author.Id == Client.Client.CurrentUser.Id &&
                message.Embeds.Count > 0 && message.Embeds.First().Title == "Now playing:")
            {
                await message.ModifyAsync(m => m.Embed = messageEmbed);
            }
            else
            {
                await e.Player.TextChannel.SendMessageAsync(embed: messageEmbed);
            }
        }

        private async Task OnTrackEnded(TrackEndedEventArgs e)
        {
            if (e.Player.Queue.TryDequeue(out LavaTrack track))
            {
                if(Pages.TryGetValue(e.Player.TextChannel.Id, out PageInfo info))
                {
                    info.UpdateInfo(e.Player.Queue.Count);
                }
                await e.Player.PlayAsync(track);
            }
        }

        public async Task ConnectToLavalink()
        {
            if (!LavaNode.IsConnected)
            {
                await LavaNode.ConnectAsync();
                if (LavaNode.IsConnected)
                {
                    _logger.LogInformation("Successfully connected to lavalink server.");
                }
                else
                {
                    _logger.LogCritical("Unable to connect to lavalink server. Check your port and credentials.");
                    Environment.Exit(-1);
                }
            }
        }
    }
}
