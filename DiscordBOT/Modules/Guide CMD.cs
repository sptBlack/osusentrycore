using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBOT.Modules
{
    public class Guide_CMD : ModuleBase
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
        
        [Command("tvt")]
        [Summary("Tablet vásárlási tanácsok")]
        public async Task tvtAsync()
        {
            await Context.Channel.SendMessageAsync("<:tablet:423189223559004170> Tablet vásárlási tanácsok fórum: https://osu.ppy.sh/community/forums/topics/339104");
        }

        [Command("bvt")]
        [Summary("Billentyűzet vásárlási tanácsok")]
        public async Task bvtAsyn()
        {
            await Context.Channel.SendMessageAsync(":keyboard: Billentyűzet vásárlási tanácsok fórum: https://osu.ppy.sh/forum/t/341461");
        }

        [Command("noob")]
        [Summary("Lexion faszsága (beginner fórum)")]
        public async Task noobAsync()
        {
            await Context.Channel.SendMessageAsync("Kezdőknek való kis lábaló: https://osu.ppy.sh/forum/t/659911");
        }
        [Command("pp")]
        [Summary("PP instrukció kimutatása")]
        public async Task ppAsync()
        {
            await Context.Channel.SendMessageAsync("PP instrukció streamekre: https://osu.ppy.sh/forum/t/697214");
        }
        [Command("multi")]
        public async Task multiAsync()
        {
            var multi = Context.Guild.GetRole(401809353033908225);      
            
            if ((Context.User as IGuildUser).RoleIds.ToList().Contains(401809353033908225))
            {
                await (await Context.Guild.GetUserAsync(Context.User.Id)).RemoveRoleAsync(Context.Guild.GetRole(401809353033908225));
                await Context.Channel.SendMessageAsync($"Sikeresen eltávolítottad a multis rankod, {Context.User.Username}!");

            }
            else
            {
                await Context.Guild.GetUserAsync(Context.User.Id).Result.AddRoleAsync(multi);
                await Context.Channel.SendMessageAsync($"Hozzáadtalak a multisokhoz, {Context.User.Username}!");
                
            }
            
        }
        [Command("segítség"), Alias("cmds")]
        public async Task helpcmd()
        {

            string Message = "```cs\n" +
                "[Felhasználói parancsok:] \n" +
                "\n" +
                "#Bot Prefixe: [ . ] \n" +
                "#Parancsokat használjátok #osu / #botspam channelben hitlerW lexion elkerülése végett, cheers\n" +
                "\n" +
                "[1] [ osu! ]\n" +
                "\n" +
                ".score - < Ki írja a recent scoreod >\n" +
                "\n" +
                "[2] [ Discord stuffs ]\n" +
                "\n" +
                ".szín (szín) - < Kapsz egy megadott színt ami látható lesz a felhasználói listában, további infóért .szín help >\n" +
                ".mylevel - < Megmutassa a jelenlegi szintedet, XP-det és a jelenlegi helyezésed >\n" +
                ".lurklvl (felhasználó név) - < Megmutassa a kiválasztott felhasználónak az XP-vel kapcsolatos információt >\n" +
                ".top10 - < Megmutassa a jelenlegi XP rendszerünkben a 10 legtöbb XP-vel rendelkező embereket >\n" +
                ".multi - < Add egy multi rangot amivel értesülhetsz mindig ping alapján, hogy ha lesz közös multizás >\n" +
                "\n" +
                "[3] [ Tutorialok / Helpek ]\n" +
                ".avatar/profilkép (név) - < Lekéred a megadott felhasználónak a profilképét >\n" +
                "\n" +
                ".pp - < PP számláló tutorial streamereknek, youtubereknek >\n" +
                ".noob - < Kezdőknek egy kis lábaló az osuhoz >\n" +
                ".bvt - < Billentyűzet vásárlási tanácsok >\n" +
                ".tvt - < Tablet vásárlási tanácsok >\n" +
                "\n";

            if (UserHasPermission((SocketGuildUser)Context.User, "Admin")
                 || UserHasPermission((SocketGuildUser)Context.User, "Staff"))
            {
                Message += "\n\nAdmin parancsok: \n" +
                "\n" +
                "[1] [ Moderációs parancsok ]\n" +
                "\n" +
                ".kirug (név) (indok) - < Kikickelsz egy adott felhasználót a szerverről >\n" +
                ".kitilt (név) (indok) - < Kitiltasz egy adott felhasználót a szerverről >\n" +
                ".töröl (szöveg mennyiség) - < Kitöröl egy adott mennyiségű szöveget >\n" +
                ".kuss (név) (indok) - < Lesilencelsz egy adott felhasználót a szerveren >\n" +
                ".unkuss (név) - < Feloldod a silencet az adott felhasználóról >\n" +
                ".zár (channel név) - < Lezár egy adott text channelt >\n" +
                ".cfelold (channel név) - < Felold egy előzöleg lezárt text channelt >\n" +
                "\n" +
                "[2] [ XD dolgok ]\n" +
                "\n" +
                ".xpreset - < Reseteli az XP adatbázist >\n" +
                ".leállítás - < Leállítja a botot >\n" +
                ".boatbotstop - < Letiltja a BoatBotot >\n" +
                ".newitem - < #NotImplementedYet >\n" +
                ".kerdes - < #NotImplementedYet >\n" +
                "\n" +
                "#A commandok nagy része logolva van tehát nem tudod használni titokban :smirk:\n" +
                "\n";
            }
            Message +=
                "#No abuserino ples :smiley_face:" +
                "```";

            await Context.Channel.SendMessageAsync(Message);
        }
    }
}
