using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBOT.Modules;
using Discord.Commands;
using DiscordBOT.Models;
using DiscordRPG;
using DiscordRPG.Models.Items;
using Discord.Net.Providers.WS4Net;

namespace DiscordBOT
{
    class Program
    {
        public const ulong ItemChannel = 422538634961616911;
        public const ulong QuestionChannel = 422538634961616911;

        public static Question CurrentQuestion;
        public static Tracker osuTracker;
        public static DiscordRPG.DiscordRPG DiscordRPG;
        public static BaseItem CurrentConfiguredItem;

        DiscordSocketClient _client;
        CommandHandler _handler;

        static void Main(string[] args)
        => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            if (Config.bot.token == "" || Config.bot.token == null) return;
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                WebSocketProvider = WS4NetProvider.Instance,
            });
            _handler = new CommandHandler();
            await _handler.InitializeAsync(_client);

            osuTracker = new Tracker("INSERT API KEY HERE", 60);
            osuTracker.NewScore += async (Score, User) =>
            {
                var Channel = _client.GetChannel(402933846661660675) as ITextChannel;
                var embed = new EmbedBuilder();
                embed.WithAuthor(User.Username);
                embed.WithAuthor(x => {
                    x.Name = User.Username;
                    x.Url = "https://osu.ppy.sh/u/" + User.UserId;
                    x.IconUrl = "";
                });
                embed.WithTitle(Score.Beatmap.Artist + " - " + Score.Beatmap.Title + " [" + Score.Beatmap.DifficultyName + "]");
                embed.WithUrl("https://osu.ppy.sh/b/" + Score.Beatmap.BeatmapID);
                string Details =
                    "MaxCombo: **" + Score.MaxCombo +
                    "**\n300: **" + Score.Count300 +
                    "**\n100: **" + Score.Count100 +
                    "**\n50: **" + Score.Count50 +
                    "**\nMisses: **" + Score.Misses +
                    "**\nPP: **" + Score.PP + "**";

                embed.AddField("Friss Score", Details);
                embed.WithColor(new Color(75, 0, 130));
                embed.WithThumbnailUrl(Score.Beatmap.ThumbnailURL);
                embed.WithFooter(x => {
                    x.Text = Score.Date.ToString("yyyy.MM.dd. HH:mm");
                });
                await Channel.SendMessageAsync("", false, embed.Build());
            };

            _client.Log += Log;
            _client.Ready += async () =>
            {
                osuTracker.StartTrack();
                //var sysConfig = _client.GetChannel(281579783224295434) as ITextChannel;
                //await sysConfig.SendMessageAsync(":warning: Elindult a fos bot most már használd a kibebaszott .score cmd-t <:angery:423936263154958336>");

                DiscordRPG = new DiscordRPG.DiscordRPG(_client);

                // A személy szintet lépett
                DiscordRPG.LevelUp += async (User, NextLevelXP, Channel) =>
                {
                    IUser _User = _client.GetUser(User.UserId);

                    var embed = new EmbedBuilder();
                    embed.WithAuthor(x =>
                    {
                        x.Name = _User.Username;
                        x.IconUrl = _User.GetAvatarUrl();
                    });
                    embed.WithColor(67, 160, 71);
                    embed.WithTitle(_User.Username + " elérte a " + DiscordRPG.LevelOf(NextLevelXP) + ". szintet!");
                    //embed.WithDescription(_User.Username + " szintet lépett!");
                    //embed.AddField("Szint:", DiscordRPG.LevelOf(NextLevelXP));
                    //embed.AddField("XP:", User.XP, inline:true);
                    //embed.AddField("Következő szint:", DiscordRPG.GetXPByLevel(User.Level) + "XP",inline:true);
                    embed.WithFooter(x => {
                        x.Text = "Használd a .mylevel parancsot további stuffért";
                    });

                    await Channel.SendMessageAsync("", embed: embed.Build());
                };
            };

            await _client.LoginAsync(TokenType.Bot, Config.bot.token);
            await _client.StartAsync();
            await _client.SetGameAsync("with hungarian nibba's scores");
            await Task.Delay(-1);
        }



        private async Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
            
        }

    }
}
