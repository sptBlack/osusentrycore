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
    public class FUN_CMD : ModuleBase
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

        [Command("telefon")]
        [Summary("Telefonos segítséget kér valakitől")]
        public async Task TelefonAsync(SocketGuildUser user)
        {
            if (!UserHasPermission((SocketGuildUser)Context.User, "Admin"))
            {
                await Context.Channel.SendMessageAsync(":x: Nincs megfelelő jogosultságod hozzá, hogy ezt a parancsot használd! " + Context.User.Mention);
                return;
            }
            await user.GetOrCreateDMChannelAsync();
            {
                await Context.Channel.SendMessageAsync("Telefonos segítség elküldve my ni :b: :b: a");
            }
            IDMChannel x = await user.GetOrCreateDMChannelAsync();
            {
                await x.SendMessageAsync($"Telefonos segítséget kér tőled  {Context.User.Mention} <:ossloth:250972301133086720>");
                System.Threading.Thread.Sleep(50000);
            }

        }
        [Command("spit")]
        [Summary("Autista fasszopóknak parancs")]
        public async Task spitAsync(SocketGuildUser user)
        {
            await Context.Channel.SendMessageAsync($"Leköpött téged {Context.User.Mention} :smirk:");
            System.Threading.Thread.Sleep(20000);
        }

        [Command("osuhun")]
        public async Task osuhunAsync()
        {
            await Context.Channel.SendMessageAsync("osu!Hungary :heart_eyes: https://puu.sh/zI8m1/8cacd63675.png");
            System.Threading.Thread.Sleep(20000);
        }
        [Command("test")]
        public async Task testAsync()
        {
            
            var embed = new EmbedBuilder();
            embed.WithAuthor(x =>
            {
                x.Name = Context.User.Username + "      " + "szintet lépett!";
                x.IconUrl = Context.User.GetAvatarUrl();
            });
            embed.WithThumbnailUrl("https://puu.sh/A8iTI/7e40411cbe.png");
            embed.WithColor(67, 160, 71);
            embed.WithTitle("Szint Lépés!");
            embed.AddField("__Szint__:", "xyyy", inline: true);
            embed.AddField("__XP__:", "xyyy", inline: true);
            embed.AddField("__Coin__:", "xyyy");
            embed.WithFooter(x => {
                x.Text = "Exmokám a legszebb :uwu:";
            });

            await Context.Channel.SendMessageAsync("", embed: embed.Build()); 
        }
        [Command("profilkép"), Alias("avatar")]
        public async Task avatarAsync(IGuildUser user)
        {
            var embed = new EmbedBuilder();
            var avatar = user.GetAvatarUrl(size:2048);
            embed.WithAuthor(x =>
            {
                x.Name = user.Username + "      " + "profilképe:";
                x.IconUrl = user.GetAvatarUrl();
            });
            embed.WithColor(144,204,187);
            embed.WithImageUrl(avatar);
            await Context.Channel.SendMessageAsync("", embed: embed.Build());
        }

    }
}
