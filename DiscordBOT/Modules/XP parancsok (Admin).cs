using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordRPG.Models.Items;
using DiscordRPG.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DiscordRPG;
using SuperSocket.ClientEngine;
using System.IO;
using System.Diagnostics;

namespace DiscordBOT.Modules
{
    public class XP_parancsok__Admin_ : ModuleBase
    {
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

        [Command("kerdes")]
        public async Task kerdes(string Question, string Answer, int Coins = 0, string RewardName = null)
        {
            if (!UserHasPermission((SocketGuildUser)Context.User, "Admin")
                 && !UserHasPermission((SocketGuildUser)Context.User, "Staff")
                 && Context.User.Id != 193356184806227969)
            {
                await Context.Channel.SendMessageAsync(":x: Nincs megfelelő jogosultságod hozzá, hogy ezt a parancsot használd! " + Context.User.Mention);
                return;
            }

            if (Program.CurrentQuestion != null)
            {
                await Context.Channel.SendMessageAsync("Most van függőben egy kérdés retard");
                return;
            }

            BaseItem RewardItem = null;
            if (RewardName != null)
            {
                ListItem Item = DiscordRPG.DiscordRPG.Items.GetItemByName(RewardName);
                if (Item == null)
                {
                    await Context.Channel.SendMessageAsync("Nincs ilyen item retard");
                    return;
                }
                RewardItem = Item.Item;
            }

            Program.CurrentQuestion = new Models.Question(Question, Answer, Coins, RewardItem?.Name);

            ITextChannel Channel = await Context.Guild.GetTextChannelAsync(Program.QuestionChannel);
            await Program.CurrentQuestion.SendAsync(Channel);
        }

        [Command("newitem")]
        public async Task AddItemAsync(params string[] Input)
        {
            string Type = null;
            if (Input.Length > 0)
                Type = string.Join(" ", Input);

            string[] EnumNames = Enum.GetNames(typeof(ItemType));
            if (string.IsNullOrWhiteSpace(Type))
            {
                string Msg = "Ezek közül tudsz választani: `" + string.Join(", ", EnumNames) + "`";
                await Context.Channel.SendMessageAsync(Msg);
                return;
            }
            if (Context.Channel.Id != Program.ItemChannel) return;

            if (Program.CurrentConfiguredItem != null)
            {
                await Context.Channel.SendMessageAsync("Jelenleg már van egy item konfigurálás alatt! Ha szeretnéd törölni, írd be a \"mégse\" szót!");
                return;
            }

            for (int i = 0; i < EnumNames.Length; i++)
            {
                if (Type.ToLower() == EnumNames[i].ToLower())
                {
                    switch ((ItemType)i)
                    {
                        case ItemType.BaseChest:
                            Program.CurrentConfiguredItem = new Chest(null);
                            break;
                        case ItemType.CoinChest:
                            Program.CurrentConfiguredItem = new Chest(null);
                            break;
                        case ItemType.osuChest:
                            Program.CurrentConfiguredItem = new Chest(null);
                            break;
                        case ItemType.XPBoost:
                            Program.CurrentConfiguredItem = new XPBoost(null);
                            break;
                        case ItemType.XPPotion:
                            Program.CurrentConfiguredItem = new XPPotion(null);
                            break;
                    }
                    break;
                }
            }

            if (Program.CurrentConfiguredItem == null)
            {
                await Context.Channel.SendMessageAsync($"Nincs { Type } típusú alap item!");
            }
            else
            {
                await Context.Channel.SendMessageAsync($"Egy { Type } típusú item konfigurálása:");

                PropertyInfo[] Properties = Program.CurrentConfiguredItem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                bool SentOne = false;
                foreach (PropertyInfo Property in Properties)
                {
                    if (Property.Name != "Type")
                    {
                        object Value = Property.GetValue(Program.CurrentConfiguredItem);
                        if (Value is string StringValue)
                        {
                            if (StringValue == null)
                            {
                                await Context.Channel.SendMessageAsync($"A(z) \"{ Property.Name }\" nevű változó üres! Kérlek adj meg egy szöveges értéket neki:");
                                SentOne = true;
                                break;
                            }
                        }
                        else if (Value is int IntValue)
                        {
                            if (IntValue == -2)
                            {
                                await Context.Channel.SendMessageAsync($"A(z) \"{ Property.Name }\" nevű változó üres! Kérlek adj meg egy számértéket neki:");
                                SentOne = true;
                                break;
                            }
                        }
                    }
                }
                if (!SentOne)
                {
                    await Context.Channel.SendMessageAsync("Kérlek add meg a BuyPrice értékét:");
                }
            }
        }

        [Command("mylevel")]
        public async Task lvl(params string[] Args)
        {
            DiscordRPG.Models.User User = Program.DiscordRPG.UserDatabase.FindUser(Context.Message.Author.Id);
            var OrderedUsers = Program.DiscordRPG.UserDatabase.Users.OrderByDescending(t => t.XP).ToList();

            var embed = new EmbedBuilder();
            embed.WithAuthor(x =>
            {
                x.Name = Context.User.Username;
                x.IconUrl = Context.User.GetAvatarUrl();
            });
            embed.WithColor(67, 160, 71);
            embed.WithTitle("__XP-vel való kapcsolatos infórmációid:__");
            embed.AddField("Szint:", User.Level);
            embed.AddField("XP:", User.XP);
            embed.AddField("Következő szint:", Program.DiscordRPG.GetXPByLevel(User.Level) + "XP");
            embed.AddField("Helyezés:", $"#{Program.DiscordRPG.UserDatabase.Users.OrderByDescending(t => t.XP).Select(t => t.UserId).ToList().IndexOf(Context.User.Id) + 1}");

            await Context.Channel.SendMessageAsync("", embed: embed.Build());
        }
        [Command("checklvl")]
        public async Task lurklvl(SocketGuildUser user, params string[] Args)
        {

            DiscordRPG.Models.User User = Program.DiscordRPG.UserDatabase.FindUser(user.Id);

            var embed = new EmbedBuilder();
            embed.WithAuthor(x =>
            {
                x.Name = user.Username;
                x.IconUrl = user.GetAvatarUrl();
            });
            embed.WithColor(67, 160, 71);
            embed.WithTitle($"__{user.Username} XP infórmációi:__");
            embed.AddField("Szint:", User.Level);
            embed.AddField("XP:", User.XP);
            embed.AddField("Következő szint:", Program.DiscordRPG.GetXPByLevel(User.Level) + "XP");
            embed.AddField("Helyezés:", $"#{Program.DiscordRPG.UserDatabase.Users.OrderByDescending(t => t.XP).Select(t => t.UserId).ToList().IndexOf(user.Id) + 1}");

            await Context.Channel.SendMessageAsync("", embed: embed.Build());

        }
        [Command("top10")]
        public async Task top10(params string[] Args)
        {

            List<string> top10 = new List<string>();
            List<int> top10lvl = new List<int>();

            var _Ordered = Program.DiscordRPG.UserDatabase.Users.OrderByDescending(t => t.XP);
            List<ulong> _Ids = _Ordered.Select(t => t.UserId).ToList();


            foreach (var User in _Ordered)
            {
                ulong Id = User.UserId;

                IGuildUser _User = await Context.Guild.GetUserAsync(Id);
                top10.Add(_User.Nickname ?? _User.Username);

                top10lvl.Add(User.Level);
            }
            string emoji = "";
            string output = "";
            for (int i = 0; i < 10; i++)
            {
                if (i >= top10.Count()) break;

                switch (i)
                {
                    case 0:
                        emoji = ":first_place:";
                        break;
                    case 1:
                        emoji = ":second_place:";
                        break;
                    case 2:
                        emoji = ":third_place:";
                        break;
                    default:
                        emoji = "";
                        break;

                }
                output += "**#**" + (i + 1) + " - " + top10.ElementAt(i) + "  -  " + "Lv."+top10lvl.ElementAt(i) + "   " + emoji + "\n";
            }

            var embed = new EmbedBuilder();
            embed.WithColor(67, 160, 71);
            embed.WithTitle($"__Top10 Lista__");
            embed.WithDescription(output);

            await Context.Channel.SendMessageAsync("", embed: embed.Build());

        }
        [Command("xpreset")]
        [Summary("Summary exomnak: kitörli a faszba a json fájlt így resetelődik az xp system :joy:")]
        public async Task xpresetAsync (params string [] Args)
        {

            if (!UserHasPermission((SocketGuildUser)Context.User, "Admin"))
            {
                await Context.Channel.SendMessageAsync(":x: Nincs megfelelő jogosultságod hozzá, hogy ezt a parancsot használd! :x:" + Context.User.Mention);
                return;
            }

            string users = @"D:\BOT Projektek\DiscordBOT\DiscordBOT\bin\Debug\Users.json";

            if (File.Exists(users))
            {
                File.Delete(users);               
            }

            await Context.Channel.SendMessageAsync("Sikeresen resetelted az XP systemet!");

        }
    }
}

