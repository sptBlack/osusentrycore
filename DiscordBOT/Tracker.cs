using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Timers;
using System.Text.RegularExpressions;
using System.IO;
using OppaiSharp;
using System.Text;

namespace DiscordBOT.Modules
{
    public static class Endpoints
    {
        public static string Base { get; set; } = "https://osu.ppy.sh/api/";
        public static string GetRecentScore { get; set; } = Base + "get_user_recent";
        public static string GetBestScore { get; set; } = Base + "get_user_best";
        public static string GetBeatmaps { get; set; } = Base + "get_beatmaps";
        public static string GetPlayer { get; set; } = Base + "get_user";
    }
    public class Tracker
    {
        private string Filename { get { return "tracking.json"; } }

        public static string osuApi { get; private set; }
        private int CallLimit { get; set; }

        public event Action<BestScore, UserTrack> NewScore;
        public List<UserTrack> TrackedUsers { get; private set; }

        private Timer Checker { get; set; }
        private object Locking = new object();

        public Tracker(string osuApi, int CallLimit)
        {
            Tracker.osuApi = osuApi;
            this.CallLimit = CallLimit;
            TrackedUsers = new List<UserTrack>();
            Checker = new Timer(1200 * Math.Max(4, TrackedUsers.Count));
            Checker.Elapsed += (sender, Args) =>
            {
                lock (Locking)
                {
                    var HasToSave = false;
                    for (int i = 0; i < TrackedUsers.Count; i++)
                    {
                        var User = TrackedUsers[i];

                        List<BestScore> BestScore = new List<BestScore>();

                        HttpWebRequest Request = WebRequest.Create(Endpoints.GetBestScore + $"?k={ osuApi }&u={ User.UserId }&limit={ User.Limit }&type=id") as HttpWebRequest;
                        using (HttpWebResponse Response = (HttpWebResponse)Request.GetResponse())
                        {
                            if (Response.StatusCode == HttpStatusCode.OK)
                            {
                                using (var Reader = new StreamReader(Response.GetResponseStream(), Encoding.UTF8))
                                {
                                    string Json = Reader.ReadToEnd();
                                    BestScore.AddRange(JsonConvert.DeserializeObject<List<BestScore>>(Json));
                                }
                            }
                        }

                        if (BestScore.Count == 0 || User.BestScores == null)
                        {
                            HasToSave = true;
                        }
                        else
                        {
                            List<BestScore> Scores = new List<BestScore>();
                            List<DateTime> UserScores = User.BestScores.Select(t => t.Date).ToList();

                            for (int j = 0; j < BestScore.Count; j++)
                            {
                                if (!UserScores.Contains(BestScore[j].Date))
                                {
                                    Scores.Add(BestScore[j]);
                                }
                            }

                            Scores = Scores.OrderByDescending(t => t.Date).ToList();

                            for (int j = 1; j < Scores.Count; j++)
                            {
                                NewScore?.Invoke(Scores[j], User);
                                HasToSave = true;
                            }
                        }

                        User.BestScores = BestScore;
                        TrackedUsers[i] = User;
                    }

                    if (HasToSave) Save();
                }
            };

            Load();
        }

        public void StartTrack()
        {
            Checker.Start();
        }
        public void StopTrack()
        {
            Checker.Stop();
        }

        public UserTrack RegisterUser(string UsernameOrId, int Limit)
        {
            if (string.IsNullOrWhiteSpace(UsernameOrId))
            {
                return null;
            }

            if (int.TryParse(UsernameOrId, out int UserId))
            {
                UsernameOrId = GetUsername(UserId);
            }
            else
            {
                UserId = GetUserId(UsernameOrId);
                UsernameOrId = GetUsername(UserId);
            }

            return RegisterUser(UsernameOrId, UserId, Limit);
        }

        private UserTrack RegisterUser(string Username, int UserId, int Limit)
        {
            var User = new UserTrack(Username, UserId, Limit);

            TrackedUsers.Add(User);

            Checker.Interval = 1200 * Math.Max(4, TrackedUsers.Count);

            Save();
            return User;
        }

        public void Load()
        {
            if (File.Exists(Filename))
            {
                TrackedUsers.AddRange(JsonConvert.DeserializeObject<UserTrack[]>(File.ReadAllText(Filename)));
            }
        }
        public void Save()
        {
            if (File.Exists(Filename))
            {
                File.Delete(Filename);
            }

            File.WriteAllText(Filename, JsonConvert.SerializeObject(TrackedUsers, Formatting.Indented));
        }

        public int GetUserId(string Username)
        {
            var Request = (HttpWebRequest)WebRequest.Create("https://osu.ppy.sh/users/" + Username);
            Request.AllowAutoRedirect = false;
            var Response = (HttpWebResponse)Request.GetResponse();
            var RedirectUrl = Response.Headers["Location"];
            Response.Close();

            var Regex = new Regex("\\/users\\/(\\d*)");
            if (Regex.IsMatch(RedirectUrl))
            {
                return int.Parse(Regex.Match(RedirectUrl).Groups[1].Value);
            }

            return -1;
        }
        public string GetUsername(int UserId)
        {
            WebClient Client = new WebClient();

            var Text = Client.DownloadString("https://osu.ppy.sh/u/" + UserId);

            var Regex = new Regex("<title>(.*?)'s profile<\\/title>");
            if (Regex.IsMatch(Text))
            {
                return Regex.Match(Text).Groups[1].Value;
            }

            return null;
        }

        public RecentScore GetRecentScore(string Username)
        {
            List<RecentScore> Recent;

            WebClient Client = new WebClient();

            if (int.TryParse(Username, out int UserId))
            {
                Recent = JsonConvert.DeserializeObject<List<RecentScore>>(
                        Client.DownloadString(Endpoints.GetBestScore + $"?k={ osuApi }&u={ UserId }&type=id"));
            }
            else
            {
                Recent = JsonConvert.DeserializeObject<List<RecentScore>>(
                        Client.DownloadString(Endpoints.GetBestScore + $"?k={ osuApi }&u={ Username }&type=string"));
            }

            if (Recent == null) return null;
            int Count;

            var Rec = Recent.First();

            for (Count = 0; Count < Recent.Count; Count++)
            {
                if (Recent[Count].BeatmapId != Rec.BeatmapId)
                    break;
            }

            Rec.PlayCount = (Count == (Recent.Count - 1) ? (Count + "+") : (Count + 1).ToString());
            return Recent.First();
        }
    }
    // Beatmap osztály, amit az API visszaad
    public class Beatmap
    {
        [JsonProperty("approved")]
        public int Approved { get; set; }

        [JsonProperty("approved_date")]
        public DateTime? ApprovedDate { get; set; }

        [JsonProperty("last_update")]
        public DateTime? LastUpdate { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("beatmap_id")]
        public int BeatmapID { get; set; }

        [JsonIgnore]
        private int _BeatmapSetID { get; set; }

        [JsonProperty("beatmapset_id")]
        public int BeatmapSetID
        {
            get
            {
                return _BeatmapSetID;
            }
            set
            {
                _BeatmapSetID = value;
                ThumbnailURL = "https://b.ppy.sh/thumb/" + value + ".jpg";
            }
        }

        [JsonProperty("bpm")]
        public float BPM { get; set; }

        [JsonProperty("creator")]
        public string Creator { get; set; }

        [JsonProperty("difficultyrating")]
        public float Difficulty { get; set; }

        [JsonProperty("diff_size")]
        public float CS { get; set; }

        [JsonProperty("diff_overall")]
        public float OD { get; set; }

        [JsonProperty("diff_approach")]
        public float AR { get; set; }

        [JsonProperty("diff_drain")]
        public float HP { get; set; }

        [JsonProperty("hit_length")]
        public int HitLength { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("genre_id")]
        public int GenreID { get; set; }

        [JsonProperty("language_id")]
        public int LanguageID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("total_length")]
        public long TotalLength { get; set; }

        [JsonProperty("version")]
        public string DifficultyName { get; set; }

        [JsonProperty("file_md5")]
        public string MD5 { get; set; }

        [JsonProperty("mode")]
        public long Mode { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("favourite_count")]
        public int FavouriteCount { get; set; }

        [JsonProperty("playcount")]
        public int PlayCount { get; set; }

        [JsonProperty("passcount")]
        public int PassCount { get; set; }

        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

        [JsonProperty("thumbnail_url")]
        public string ThumbnailURL { get; set; }
    }

    public class Player
    {
        [JsonProperty("user_id")]
        public int UserID { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("count300")]
        public uint Hit300s { get; set; }

        [JsonProperty("count100")]
        public uint Hit100s { get; set; }

        [JsonProperty("count50")]
        public uint Hit50s { get; set; }

        [JsonProperty("playcount")]
        public int Playcount { get; set; }

        [JsonProperty("ranked_score")]
        public long RankedScore { get; set; }

        [JsonProperty("total_score")]
        public long TotalScore { get; set; }

        [JsonProperty("pp_rank")]
        public uint GlobalRank { get; set; }

        [JsonProperty("level")]
        public float Level { get; set; }

        [JsonProperty("pp_raw")]
        public float GlobalPP { get; set; }

        [JsonProperty("accuracy")]
        public float Accuracy { get; set; }

        [JsonProperty("count_rank_ss")]
        public int SSCount { get; set; }

        [JsonProperty("count_rank_ssh")]
        public int SSHCount { get; set; }

        [JsonProperty("count_rank_s")]
        public int SCount { get; set; }

        [JsonProperty("count_rank_sh")]
        public int SHCount { get; set; }

        [JsonProperty("count_rank_a")]
        public int ACount { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("pp_country_rank")]
        public uint CountryRank { get; set; }
    }

    // RecentScore osztály amit az API visszaad
    public class RecentScore
    {
        [JsonIgnore]
        private Beatmap _Beatmap;

        [JsonIgnore]
        public Beatmap Beatmap
        {
            get
            {
                if (_Beatmap == null)
            {
                    _Beatmap = JsonConvert.DeserializeObject<Beatmap[]>(
                        new WebClient().DownloadString(
                            Endpoints.GetBeatmaps + $"?k={ Tracker.osuApi }&b={ BeatmapId }"))[0];
                }
                return _Beatmap;
            }
        }

        [JsonProperty("beatmap_id")]
        public int BeatmapId { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }

        [JsonProperty("count300")]
        public int Count300 { get; set; }

        [JsonProperty("count100")]
        public int Count100 { get; set; }

        [JsonProperty("count50")]
        public int Count50 { get; set; }

        [JsonProperty("countmiss")]
        public int Misses { get; set; }

        [JsonProperty("maxcombo")]
        public int MaxCombo { get; set; }

        [JsonProperty("countkatu")]
        public int CountKatu { get; set; }

        [JsonProperty("countgeki")]
        public int CountGeki { get; set; }

        [JsonProperty("perfect")]
        public int Perfect { get; set; }

        [JsonProperty("enabled_mods")]
        public double Mods { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("rank")]
        public string Rank { get; set; }

        [JsonIgnore]
        public string PlayCount { get; set; }
    }

    // BestScore osztály amit az API visszaad
    public struct BestScore
    {
        [JsonIgnore]
        private Beatmap _Beatmap;

        [JsonIgnore]
        public Beatmap Beatmap
        {
            get
            {
                if (_Beatmap == null)
                {
                    _Beatmap = JsonConvert.DeserializeObject<Beatmap[]>(
                        new WebClient().DownloadString(
                            Endpoints.GetBeatmaps + $"?k={ Tracker.osuApi }&b={ BeatmapId }"))[0];
                }
                return _Beatmap;
            }
        }

        [JsonProperty("beatmap_id")]
        public string BeatmapId { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }

        [JsonProperty("count300")]
        public int Count300 { get; set; }

        [JsonProperty("count100")]
        public int Count100 { get; set; }

        [JsonProperty("count50")]
        public int Count50 { get; set; }

        [JsonProperty("countmiss")]
        public int Misses { get; set; }

        [JsonProperty("maxcombo")]
        public int MaxCombo { get; set; }

        [JsonProperty("countkatu")]
        public int CountKatu { get; set; }

        [JsonProperty("countgeki")]
        public int CountGeki { get; set; }

        [JsonProperty("perfect")]
        public int Perfect { get; set; }

        [JsonProperty("enabled_mods")]
        public double Mods { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("rank")]
        public string Rank { get; set; }

        [JsonProperty("pp")]
        public float PP { get; set; }
    }

    // Trackelt felhasználók
    public class UserTrack
    {
        public List<BestScore> BestScores { get; set; }
        public int Limit { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }

        public UserTrack(string Username, int UserId, int Limit = 50)
        {
            this.UserId = UserId;
            this.Username = Username;
            this.Limit = 50;
        }
    }

    // Modok: előre megadott, így tartja nyilván az API
    enum Mods
    {
        None = 0,
        NF = 1,
        EZ = 2,
        NV = 4,
        HD = 8,
        HR = 16,
        SD = 32,
        DT = 64,
        RX = 128,
        HT = 256,
        NC = 512,
        FL = 1024,
        AP = 2048,
        SO = 4096,
        AP2 = 8192,
        PF = 16384,
        K4 = 32768,
        K5 = 65536,
        K6 = 131072,
        K7 = 262144,
        K8 = 524288,
        Key = K4 | K5 | K6 | K7 | K8,
        FI = 1048576,
        Random = 2097152,
        LM = 4194304,
        FreeModAllowed = NF | EZ | HD | HR | SD | FL | FI | RX | AP2 | SO | Key,
        K9 = 16777216,
        K10 = 33554432,
        K1 = 67108864,
        K3 = 134217728,
        K2 = 268435456
    }

    // Accuracy counting, PP counting
    public static class Helpers
    {
        // A this miatt ez egy extension:
        // Egy object-hez ad egy plussz funkciót
        // public static kötelező extension-hoz
        public static double GetAccuracy(this RecentScore Score)
        {
            // Accuracy count formula
            return Math.Truncate((((Score.Count300 * 6)
                                    + (Score.Count100 * 2)
                                    + Score.Count50) /
                                    ((Score.Count300
                                    + Score.Count100
                                    + Score.Count50
                                    + Score.Misses) * 6f) * 100) * 100f) / 100f;
        }

        public static string GetMods(this RecentScore Score)
        {
            var EnabledMods = Score.Mods;

            if (EnabledMods == 0)
                return "";

            string Mods = "";

            // Lekéri a modok hosszát, először a neveket, majd annak a hosszát
            var ModsCount = Enum.GetNames(typeof(Mods)).Length;

            // Visszaszámláló ciklus, mivel a 2 hatványai a modok, és ha e.g. 72 az Enabled Mods (HDDT), akkor ha fordítva menne
            // Bedobná a NF EZ NoVideo HD HR SD DT-t :v
            for (var i = ModsCount; i != 0; i--)
            {
                // Yehát 2 hatványain vannak a modok
                var Value = (long)Math.Pow(2, i);


                // Ha nagyobb a vagy egyenlő az értéke a modnak
                if (EnabledMods >= Value)
                {
                    // Lekéri az enum-ből a nevét, és hozzáadja a modok neveihez
                    Mods = Mods.Insert(0, Enum.GetName(typeof(Mods), Value));
                    // Kivonja, ne legyen az utáni modok hozzáadva
                    EnabledMods -= Value;
                }
            }
            return Mods;
        }

        public static double GetPP(this RecentScore Score)
        {
            var Client = new WebClient();

            var Data = Client.DownloadData("https://osu.ppy.sh/osu/" + Score.BeatmapId);
            var Stream = new MemoryStream(Data, false);
            var Reader = new StreamReader(Stream);

            var Beatmap = OppaiSharp.Beatmap.Read(Reader);
            var mods = (OppaiSharp.Mods)Score.Mods;
            var diff = new DiffCalc().Calc(Beatmap, mods);

            var pp = new PPv2(new PPv2Parameters(Beatmap, diff, c300: Score.Count300, c100: Score.Count100, c50: Score.Count50, cMiss: Score.Misses, combo: Score.MaxCombo, mods: mods));

            return pp.Total;
        }

        public static double GetPotentialMaximumPP(this RecentScore Score)
        {
            var Client = new WebClient();

            var Data = Client.DownloadData("https://osu.ppy.sh/osu/" + Score.BeatmapId);
            var Stream = new MemoryStream(Data, false);
            var Reader = new StreamReader(Stream);

            var beatmap = OppaiSharp.Beatmap.Read(Reader);
            var mods = (OppaiSharp.Mods)Score.Mods;
            var diff = new DiffCalc().Calc(beatmap, mods);

            var pp = new PPv2(new PPv2Parameters(beatmap, diff, c100: 0, c50: 0, cMiss: 0, mods: mods));

            return pp.Total;
        }

        public static string GetRankEmoji(this RecentScore Score)
        {
            string scoreRank = Score.Rank;
            string emojiToReturn = "";

            switch (scoreRank)
            {
                case "D":
                    emojiToReturn = "<:rankD:423508936386281482>";
                    break;
                case "C":
                    emojiToReturn = "<:rankC:423508936474492929>";
                    break;
                case "B":
                    emojiToReturn = "<:rankB:423508936159789057>";
                    break;
                case "A":
                    emojiToReturn = "<:rankA:423508936348663818>";
                    break;
                case "S":
                    emojiToReturn = "<:rankS:423508936247738369>";
                    break;
                case "SH":
                    emojiToReturn = "<:rankHS:423508936386150400>";
                    break;
                case "X":
                    emojiToReturn = "<:rankSS:423508936403189760>";
                    break;
                case "XH":
                    emojiToReturn = "<:rankHSS:423508936591933440>";
                    break;
                default:
                    emojiToReturn = "**F**";
                    break;
            }

            return emojiToReturn;
        }
    }
}
