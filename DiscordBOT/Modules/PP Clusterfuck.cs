using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBOT.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OppaiSharp;
using System.Net.Http;
using Newtonsoft.Json;
using System.Globalization;

namespace DiscordBOT.Modules
{
    public class TrackerCMD : ModuleBase
    {
        [Command("reg")]
        [Summary("Regisztrál 1 usert a trackinghez.")]
        public async Task TrackReg(params string[] Args)
        {
            /*if (!UserHasPermission((SocketGuildUser)Context.User, "Admin")
                 && !UserHasPermission((SocketGuildUser)Context.User, "Staff"))
            {
                await Context.Channel.SendMessageAsync(":x: Nincs megfelelő jogosultságod hozzá, hogy ezt a parancsot használd! " + Context.User.Mention);
                return;
            }*/
            var userToRegister = Args[0];
            var ScoreLimit = 50;

            var Limit = Args.Where(t => t.ToLower().StartsWith("limit="));
            if (Limit.Count() > 0)
            {
                bool Argument = int.TryParse(Limit.First().Split('=')[1], out ScoreLimit);
                if (!Argument)
                {
                    ScoreLimit = 50;
                }
            }
            var Success = Program.osuTracker.RegisterUser(Args[0], ScoreLimit);
            if (Success != null)
            {
                await Context.Channel.SendMessageAsync("Hozzáadva a listához! :white_check_mark:");
            }
            else
            {
                await Context.Channel.SendMessageAsync("Nem találtam a felhasználót! :x:");
            }
        }

        private bool UserHasPermission(SocketGuildUser user, string role)
        {
            string targetRoleName = role;
            var result = from r in user.Guild.Roles
                         where r.Name == targetRoleName
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return false;
            var targetRole = user.Guild.GetRole(roleID);
            return user.Roles.Contains(targetRole);
        }
        [Command("score")]
        [Summary("most semmi XD")]
        public async Task RecentAsync(params string[] Args)
        {
            /*if (!UserHasPermission((SocketGuildUser)Context.User, "Admin")
                 && !UserHasPermission((SocketGuildUser)Context.User, "Staff"))
            {
                await Context.Channel.SendMessageAsync(":x: Nincs megfelelő jogosultságod hozzá, hogy ezt a parancsot használd! " + Context.User.Mention);
                return;
            }*/

            WebClient Client = new WebClient();
            string Username = (Args.Length > 0 ? string.Join(" ", Args) : Context.User.Username);
            string ScoreJson = Client.DownloadString(Endpoints.GetRecentScore + "?k=" + Tracker.osuApi + "&u=" + Username + "&type=string&limit=50");
            var Scores = JsonConvert.DeserializeObject<List<RecentScore>>(ScoreJson);

            if (Scores.Count == 0)

            {
                await Context.Channel.SendMessageAsync("Ez a felhasználó nem játszott még ma, vagy a megadott felhasználó nem található, avagy goodfuckingbye API <:cringe:250644825383239681> ");
                return;
            }

            string UserJson = Client.DownloadString(Endpoints.GetPlayer + "?k=" + Tracker.osuApi + "&u=" + Username + "&type=string");
            var User = JsonConvert.DeserializeObject<List<Player>>(UserJson);
            var Player = User[0];

            RecentScore Score = Scores[0];
            
            //await Context.Channel.SendMessageAsync($"Try {Score.PlayCount} ");
            var embed = new EmbedBuilder();
            embed.WithAuthor(x => {
                x.Name = Player.Username + ": " + String.Format("{0:0}", Player.GlobalPP) + "pp   (#" + Player.GlobalRank + ")   [" + Player.Country + " #" + Player.CountryRank + "]";
                x.Url = "https://osu.ppy.sh/u/" + Score.UserId;
                x.IconUrl = "https://a.ppy.sh/" + Score.UserId;
            });
            embed.WithTitle(Score.Beatmap.Artist + " - " + Score.Beatmap.Title + " [" + Score.Beatmap.DifficultyName + "]");
            embed.WithUrl("https://osu.ppy.sh/b/" + Score.Beatmap.BeatmapID);

            embed.AddField("__Rank__", Score.GetRankEmoji(), inline: true);
            embed.AddField("__Score__", "**" + Score.Score.ToString("N0", CultureInfo.CreateSpecificCulture("nl-NL")) + "**", inline: true);
            embed.AddField("__Combo__", "**" + Score.MaxCombo + "x" +"**/" + Score.Beatmap.MaxCombo + "x", inline: true);
            embed.AddField("__Accuracy__", "**" + Score.GetAccuracy() + "%** _{ " + Score.Count300 + " / " + Score.Count100 + " / " + Score.Count50 + " / " + Score.Misses + " }_", inline: true);
            if (Score.Beatmap.Approved == 1 || Score.Beatmap.Approved == 2)
            {
                embed.AddField("__PP__", "**" + String.Format("{0:0.00}", Score.GetPP()) + "**/" + String.Format("{0:0.00}", Score.GetPotentialMaximumPP()), inline: true);
            }
            else
            {
                embed.AddField("__PP__", "~~**" + String.Format("{0:0.00}", Score.GetPP()) + "**/" + String.Format("{0:0.00}" + "~~", Score.GetPotentialMaximumPP()), inline: true);
            }
            if (!string.IsNullOrWhiteSpace(Score.GetMods()))
                embed.AddField("__Mods__", "+**" + Score.GetMods() + "**", inline: true);

            TimeSpan beatmapTotalLength = TimeSpan.FromSeconds(Score.Beatmap.TotalLength);
            TimeSpan beatmapDrainLength = TimeSpan.FromSeconds(Score.Beatmap.HitLength);
            embed.AddField("__Beatmap Details__",
                "**Length:** " + beatmapTotalLength.ToString(@"mm\:ss") + " (" + beatmapDrainLength.ToString(@"mm\:ss") + ") \n" +
                "**BPM:** " + Score.Beatmap.BPM + "     " +
                "**CS:** " + Score.Beatmap.CS + "     " +
                "**AR:** " + Score.Beatmap.AR + "     \n" + 
                "**OD:** " + Score.Beatmap.OD + "     " + 
                "**HP:** " + Score.Beatmap.HP + "     " + 
                "**Stars:** " + String.Format("{0:0.00}", Score.Beatmap.Difficulty));
            embed.WithColor(new Color(75, 0, 130));
            embed.WithThumbnailUrl(Score.Beatmap.ThumbnailURL);
            embed.WithFooter(x => {
                x.Text = "Played at: " + Score.Date.AddHours(-7).ToString("yyyy.MM.dd., HH:mm") + "    |    " + "Map made by: " + Score.Beatmap.Creator;
            });

            int SameCount = 0;
            int BeatmapId, EnabledMods;
            BeatmapId = Scores[0].BeatmapId;
            EnabledMods = (int)Scores[0].Mods;

            for (int i = 0; i < Scores.Count; i++)
            {
                if (Scores[i].BeatmapId == BeatmapId && EnabledMods == Scores[i].Mods) SameCount++;
                    else break;
            }

            await Context.Channel.SendMessageAsync($"Try #{SameCount}", false, embed.Build());
        }
    }

}
