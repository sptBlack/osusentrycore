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
    public class Misc : ModuleBase
    {
        [Command("debug")]
        public async Task Echo(string message)
        {
            if (!UserHasPermission((SocketGuildUser)Context.User, "Admin")
                 && !UserHasPermission((SocketGuildUser)Context.User, "Staff"))
            {
                await Context.Channel.SendMessageAsync(":x: Nincs megfelelő jogosultságod hozzá, hogy ezt a parancsot használd! " + Context.User.Mention);
                return;
            }

            var embed = new EmbedBuilder();
            embed.WithTitle($"{ Context.User.Username } által kikiáltott autism");
            embed.WithDescription(message);
            embed.WithColor(new Color(0, 0, 255));

            await Context.Channel.SendMessageAsync("", false, embed.Build());
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

        [Command("kirug"), Alias("kick")]
        [Summary("Kirug egy kiválasztott felhasználót.")]
        public async Task Kick(SocketGuildUser user, params string[] Args)
        {
            var logChannel = (await Context.Guild.GetChannelAsync(426002375904329729)) as ITextChannel;
            var embed = new EmbedBuilder();
            embed.WithColor(new Color(120, 120, 255));
            embed.WithAuthor(Context.User.Mention);
            embed.WithAuthor(x =>
            {
                x.Name = Context.User.Username + "       " + "végrehajtott egy parancsot!";
                x.IconUrl = Context.User.GetAvatarUrl();
            });
            embed.WithTitle("");
            embed.AddField("**__Használt Parancs: __**", ".kirug", inline: true);
            embed.AddField($"**__Akin használták: __**", user.Mention, inline: true);
            embed.WithThumbnailUrl("http://clipartbarn.com/wp-content/uploads/2017/01/Warning-clipart-9.png");
            embed.WithFooter(x =>
            {
                x.Text = "Verzió: v0.0.7";
            });
            if (!UserHasPermission((SocketGuildUser)Context.User, "Admin")
                 && !UserHasPermission((SocketGuildUser)Context.User, "Staff"))
            {
                embed.AddField($"**__Jogosultsága __**", ":warning: Megtagadva");
                await Context.Channel.SendMessageAsync(":x: Nincs megfelelő jogosultságod hozzá, hogy ezt a parancsot használd! " + Context.User.Mention);
                await logChannel.SendMessageAsync("", false, embed.Build());
                return;
            }

            embed.AddField($"**__Jogosultsága __**", ":white_check_mark: Engedélyezve");
            await logChannel.SendMessageAsync("", false, embed.Build());

            if (Args.Length == 0)
            {
                await Context.Channel.SendMessageAsync(":x: Indokot kötelező megadnod! :x: " + Context.User.Mention);
                return;
            }

            await Context.Channel.SendMessageAsync($"{user.Username} ki lett rúgva a szerverről!");
            IDMChannel y = await user.GetOrCreateDMChannelAsync();
            {
                await y.SendMessageAsync($"Ki lettél rúgva ezzel az indokkal: {string.Join("", Args)} ");
            }
            await Context.Channel.SendMessageAsync($"{user.Username} ki lett rúgva a szerverről!", false, embed.Build());
            await user.KickAsync();


        }

        [Command("kitilt"), Alias("ban")]
        [Summary("Kitilt @")]
        [RequireBotPermission(GuildPermission.BanMembers)] ///BOT jogosultság ///
        public async Task BanAsync(SocketGuildUser user = null, params string[] Args)
        {
            var logChannel = (await Context.Guild.GetChannelAsync(426002375904329729)) as ITextChannel;
            var embed = new EmbedBuilder();
            embed.WithColor(new Color(120, 120, 255));
            embed.WithAuthor(Context.User.Mention);
            embed.WithAuthor(x =>
            {
                x.Name = Context.User.Username + "       " + "végrehajtott egy parancsot!";
                x.IconUrl = Context.User.GetAvatarUrl();
            });
            embed.WithTitle("");
            embed.AddField("**__Használt Parancs: __**", ".kitilt", inline: true);
            embed.AddField($"**__Akin használták: __**", user.Mention, inline: true);
            embed.WithThumbnailUrl("http://clipartbarn.com/wp-content/uploads/2017/01/Warning-clipart-9.png");

            if (!UserHasPermission((SocketGuildUser)Context.User, "Admin")
                 && !UserHasPermission((SocketGuildUser)Context.User, "Staff"))
            {
                embed.AddField($"**__Jogosultsága __**", ":warning: Megtagadva");
                await Context.Channel.SendMessageAsync(":x: Nincs megfelelő jogosultságod hozzá, hogy ezt a parancsot használd! " + Context.User.Mention);
                await logChannel.SendMessageAsync("", false, embed.Build());
                return;
            }

            embed.AddField($"**__Jogosultsága __**", ":white_check_mark: Engedélyezve");
            await logChannel.SendMessageAsync("", false, embed.Build());

            if (user == null)
            {
                await Context.Channel.SendMessageAsync("Adj meg 1 felhasználót előszőr.");
                return;
            }

            if (Args.Length == 0)

            {
                await Context.Channel.SendMessageAsync(":x: Indokot kötelező megadnod! :x: " + Context.User.Mention);
                return;
            }

            {
                var gld = Context.Guild as SocketGuild;
                await Context.Channel.SendMessageAsync($"{user.Username} ki lett tiltva a szerverről!");
                IDMChannel y = await user.GetOrCreateDMChannelAsync();
                {
                    await y.SendMessageAsync($"{user.Username} Ki lettél bannolva ezzel az indokkal: {string.Join("", Args)} ");
                }
                await gld.AddBanAsync(user); ///Kibannolja a kiválasztott felhasználót///
                await Context.Channel.SendMessageAsync("", false, embed.Build()); ///Elküldi az embedet///
            }
        }

        [Command("töröl"), Alias("prune")]
        [Summary("Kitöröl egy x mennyiségű szöveget")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ClearMessage([Remainder] int x = 0)
        {
            if (x <= 100)
            {
                var messagesToDelete = await Context.Channel.GetMessagesAsync(x + 1).Flatten().ToList();
                await (Context.Channel as ITextChannel).DeleteMessagesAsync(messagesToDelete);
                if (x == 1)
                {
                    IMessage Message = await Context.Channel.SendMessageAsync($":white_check_mark: {Context.User.Username} kitörölt 1 szöveget :white_check_mark:");
                    await Task.Delay(3000);
                    await Message.DeleteAsync();
                }
                else
                {
                    IMessage Message = await Context.Channel.SendMessageAsync($":white_check_mark: {Context.User.Username} kitörölt {x} szöveget :white_check_mark:");
                    await Task.Delay(3000);
                    await Message.DeleteAsync();
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync(":x: Nem tudsz 100 szövegnél többet kitörölni :x:");
            }
        }

        [Command("say")]
        public async Task SayBlackAsync(params string[] Args)
        {
            if (!UserHasPermission((SocketGuildUser)Context.User, "Admin")
         && !UserHasPermission((SocketGuildUser)Context.User, "Staff"))
            {
                await Context.Channel.SendMessageAsync(":x: Nincs megfelelő jogosultságod hozzá, hogy ezt a parancsot használd! :x:" + Context.User.Mention);
                return;
            }

            if (Args.Length > 0)
            {
                var Channel = Context.Channel;
                string Text = null;

                if (Args.Length > 1)
                {
                    var Channels = Context.Guild.GetTextChannelsAsync().Result.ToList().FindAll(t => t.Name.ToLower() == Args[0].ToLower());
                    if (Channels.Count == 1)
                    {
                        Channel = Channels[0];
                        Text = string.Join(" ", Args.Skip(1));
                    }

                }
                if (Text == null)
                {
                    Text = string.Join(" ", Args);
                }
                var embed = new EmbedBuilder();
                embed.WithTitle(":warning: Admin Announcement :warning:");
                embed.WithDescription(Text);
                embed.WithColor(new Color(0, 255, 255));

                await Channel.SendMessageAsync("", embed: embed.Build());
            }
        }

        /*[Command("tfelold")]
        [Summary("Unbannol egy adott felhasználót.")]
        public async Task UnbannolakXDAsync(SocketGuildUser user)
        {
            var logChannel = (await Context.Guild.GetChannelAsync(426002375904329729)) as ITextChannel;
            var serverTextChannels = await Context.Guild.GetTextChannelsAsync();
            var muteride = new OverwritePermissions(sendMessages: PermValue.Deny);
            var embed = new EmbedBuilder();
            embed.WithColor(new Color(120, 120, 255));
            embed.WithAuthor(Context.User.Mention);
            embed.WithAuthor(x => {
                x.Name = Context.User.Username + "       " + "végrehajtott egy parancsot!";
                x.IconUrl = Context.User.GetAvatarUrl();
            });
            embed.WithTitle("");
            embed.AddField("**__Használt Parancs: __**", ".tfelold", inline: true);
            embed.AddField($"**__Akin használták: __**", user.Mention, inline: true);
            embed.WithThumbnailUrl("http://clipartbarn.com/wp-content/uploads/2017/01/Warning-clipart-9.png");
            embed.WithFooter(x => {
                x.Text = "Verzió: v0.0.7";
            });
            if (!UserHasPermission((SocketGuildUser)Context.User, "Admin")
                 && !UserHasPermission((SocketGuildUser)Context.User, "Staff"))
            {
                embed.AddField($"**__Jogosultsága __**", ":warning: Megtagadva");
                await Context.Channel.SendMessageAsync(":x: Nincs megfelelő jogosultságod hozzá, hogy ezt a parancsot használd! " + Context.User.Mention);
                await logChannel.SendMessageAsync("", false, embed.Build());
                return;
            }
            embed.AddField($"**__Jogosultsága __**", ":white_check_mark: Engedélyezve");
            await logChannel.SendMessageAsync("", false, embed.Build());
            var Bans = await Context.Guild.GetBansAsync();
            foreach (var Ban in Bans)
            {
                //Etc,idk mit ad a Ban ezt kérte Exmo h írjam ide bármennyire retardáltnak is néz ki
            }
            await Context.Guild.RemoveBanAsync(user);
            await Context.Channel.SendMessageAsync(":white_check_mark: Felhasználó unbannolva :white_check_mark:");
            IDMChannel y = await user.GetOrCreateDMChannelAsync();
            {
                await y.SendMessageAsync($"Unbannolva lettél erről a szerverről: {Context.Guild.Name}");
            }
                  
            await Context.Channel.SendMessageAsync($"{user.Username} unbannolva lett a szerverről!");
        }  */
        [Command("leállítás")]
        [Summary("??? XD")]
        public async Task ShutdownAsync()
        {
            var logChannel = (await Context.Guild.GetChannelAsync(426002375904329729)) as ITextChannel;
            var embed = new EmbedBuilder();
            embed.WithColor(new Color(120, 120, 255));
            embed.WithAuthor(Context.User.Mention);
            embed.WithAuthor(x =>
            {
                x.Name = Context.User.Username + "       " + "végrehajtott egy parancsot!";
                x.IconUrl = Context.User.GetAvatarUrl();
            });
            embed.WithTitle("");
            embed.AddField("**__Használt Parancs: __**", ".leállítás", inline: true);
            embed.AddField($"**__Akin használták: __**", "Lexion A Programozó", inline: true);
            embed.WithThumbnailUrl("http://clipartbarn.com/wp-content/uploads/2017/01/Warning-clipart-9.png");
            if (!UserHasPermission((SocketGuildUser)Context.User, "Admin")
                 && !UserHasPermission((SocketGuildUser)Context.User, "Staff"))
            {
                embed.AddField($"**__Jogosultsága __**", ":warning: Megtagadva");
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }

            embed.AddField($"**__Jogosultsága __**", ":white_check_mark: Engedélyezve");
            await logChannel.SendMessageAsync("", false, embed.Build());
            await Context.Channel.SendMessageAsync("Viiszooooooooontlááááááátáááááááááááásraaaaaaaa :wave: <:cringe:250644825383239681>");
            Environment.Exit(exitCode: 0);
        }

        [Command("boatbotstop")]
        [Summary("w/e")]
        public async Task boatbotstopAsync()
        {
            var logChannel = (await Context.Guild.GetChannelAsync(426002375904329729)) as ITextChannel;
            var serverTextChannels = await Context.Guild.GetTextChannelsAsync();
            var muteride = new OverwritePermissions(sendMessages: PermValue.Deny);

            var embed = new EmbedBuilder();
            embed.WithColor(new Color(120, 120, 255));
            embed.WithAuthor(Context.User.Mention);
            embed.WithAuthor(x =>
            {
                x.Name = Context.User.Username + "       " + "végrehajtott egy parancsot!";
                x.IconUrl = Context.User.GetAvatarUrl();
            });
            embed.WithTitle("");
            embed.AddField("**__Használt Parancs: __**", ".boatbotstop", inline: true);
            embed.AddField($"**__Akin használták: __**", "BoatBot", inline: true);
            embed.WithThumbnailUrl("http://clipartbarn.com/wp-content/uploads/2017/01/Warning-clipart-9.png");

            if (!UserHasPermission((SocketGuildUser)Context.User, "Admin")
                 && !UserHasPermission((SocketGuildUser)Context.User, "Staff"))
            {
                embed.AddField($"**__Jogosultsága __**", ":warning: Megtagadva");
                await Context.Channel.SendMessageAsync(":x: Nincs megfelelő jogosultságod hozzá, hogy ezt a parancsot használd! " + Context.User.Mention);
                await logChannel.SendMessageAsync("", false, embed.Build());
                return;
            }

            embed.AddField($"**__Jogosultsága __**", ":white_check_mark: Engedélyezve");
            await logChannel.SendMessageAsync("", false, embed.Build());

            var boatBot = await Context.Client.GetUserAsync(185013154198061056);
            var Override = new OverwritePermissions(sendMessages: PermValue.Deny);

            await (Context.Channel as ITextChannel).AddPermissionOverwriteAsync(boatBot, Override);
            await Context.Channel.SendMessageAsync("BoatBot letiltva erről a channelről!");
        }
        [Command("boatbotstart")]
        [Summary("w/e")]
        public async Task boatbotstartAsync()
        {
            var logChannel = (await Context.Guild.GetChannelAsync(426002375904329729)) as ITextChannel;
            var serverTextChannels = await Context.Guild.GetTextChannelsAsync();
            var muteride = new OverwritePermissions(sendMessages: PermValue.Deny);

            var embed = new EmbedBuilder();
            embed.WithColor(new Color(120, 120, 255));
            embed.WithAuthor(Context.User.Mention);
            embed.WithAuthor(x =>
            {
                x.Name = Context.User.Username + "       " + "végrehajtott egy parancsot!";
                x.IconUrl = Context.User.GetAvatarUrl();
            });
            embed.WithTitle("");
            embed.AddField("**__Használt Parancs: __**", ".boatbotstart", inline: true);
            embed.AddField($"**__Akin használták: __**", "BoatBot", inline: true);
            embed.WithThumbnailUrl("http://clipartbarn.com/wp-content/uploads/2017/01/Warning-clipart-9.png");

            if (!UserHasPermission((SocketGuildUser)Context.User, "Admin")
                 && !UserHasPermission((SocketGuildUser)Context.User, "Staff"))
            {
                embed.AddField($"**__Jogosultsága __**", ":warning: Megtagadva");
                await Context.Channel.SendMessageAsync(":x: Nincs megfelelő jogosultságod hozzá, hogy ezt a parancsot használd! " + Context.User.Mention);
                await logChannel.SendMessageAsync("", false, embed.Build());
                return;
            }

            embed.AddField($"**__Jogosultsága __**", ":white_check_mark: Engedélyezve");
            await logChannel.SendMessageAsync("", false, embed.Build());

            var boatBot = await Context.Client.GetUserAsync(185013154198061056);
            await (Context.Channel as ITextChannel).RemovePermissionOverwriteAsync(boatBot);

            await Context.Channel.SendMessageAsync("BoatBot engedélyezve ezen a channelen!");
        }
        [Command("kuss"), Alias("mute")]
        public async Task kussbetaAsync(SocketGuildUser user)
        {

            var logChannel = (await Context.Guild.GetChannelAsync(426002375904329729)) as ITextChannel;
            var serverTextChannels = await Context.Guild.GetTextChannelsAsync();
            var muteride = new OverwritePermissions(sendMessages: PermValue.Deny);

            var embed = new EmbedBuilder();
            embed.WithColor(new Color(120, 120, 255));
            embed.WithAuthor(Context.User.Mention);
            embed.WithAuthor(x =>
            {
                x.Name = Context.User.Username + "       " + "végrehajtott egy parancsot!";
                x.IconUrl = Context.User.GetAvatarUrl();
            });
            embed.WithTitle("");
            embed.AddField("**__Használt Parancs: __**", ".kuss", inline: true);
            embed.AddField($"**__Akin használták: __**", user.Mention, inline: true);
            embed.WithThumbnailUrl("http://clipartbarn.com/wp-content/uploads/2017/01/Warning-clipart-9.png");

            if (!UserHasPermission((SocketGuildUser)Context.User, "Admin")
                 && !UserHasPermission((SocketGuildUser)Context.User, "Staff"))
            {
                embed.AddField($"**__Jogosultsága __**", ":warning: Megtagadva");
                await Context.Channel.SendMessageAsync(":x: Nincs megfelelő jogosultságod hozzá, hogy ezt a parancsot használd! " + Context.User.Mention);
                await logChannel.SendMessageAsync("", false, embed.Build());
                return;
            }

            embed.AddField($"**__Jogosultsága __**", ":white_check_mark: Engedélyezve");
            IMessage Message = await Context.Channel.SendMessageAsync("Sikeresen használtad a parancsot, kérlek várj egy picit míg végrehajtom! <a:process:426000377314541568>");
            await logChannel.SendMessageAsync("", false, embed.Build());

            foreach (ITextChannel currentChannel in serverTextChannels)
            {
                await (currentChannel).AddPermissionOverwriteAsync(user, muteride);
            }

            await Message.DeleteAsync();
            await Context.Channel.SendMessageAsync($"{user.Mention} muteolva lett!");
        }
        [Command("unkuss"), Alias("unmute")]
        public async Task unkussbetaAsync(SocketGuildUser user)
        {

            var logChannel = (await Context.Guild.GetChannelAsync(426002375904329729)) as ITextChannel;
            var serverTextChannels = await Context.Guild.GetTextChannelsAsync();
            var muteride = new OverwritePermissions(sendMessages: PermValue.Deny);
            var embed = new EmbedBuilder();
            embed.WithColor(new Color(120, 120, 255));
            embed.WithAuthor(Context.User.Mention);
            embed.WithAuthor(x =>
            {
                x.Name = Context.User.Username + "       " + "végrehajtott egy parancsot!";
                x.IconUrl = Context.User.GetAvatarUrl();
            });
            embed.WithTitle("");
            embed.AddField("**__Használt Parancs: __**", ".unkuss", inline: true);
            embed.AddField($"**__Akin használták: __**", user.Mention, inline: true);
            embed.WithThumbnailUrl("http://clipartbarn.com/wp-content/uploads/2017/01/Warning-clipart-9.png");

            if (!UserHasPermission((SocketGuildUser)Context.User, "Admin")
                 && !UserHasPermission((SocketGuildUser)Context.User, "Staff"))
            {
                embed.AddField($"**__Jogosultsága__** ", ":warning: Megtagadva");
                await Context.Channel.SendMessageAsync(":x: Nincs megfelelő jogosultságod hozzá, hogy ezt a parancsot használd! " + Context.User.Mention);
                await logChannel.SendMessageAsync("", false, embed.Build());
                return;
            }

            embed.AddField($"**__Jogosultsága __**", ":white_check_mark: Engedélyezve");
            await logChannel.SendMessageAsync("", false, embed.Build());

            IMessage Message = await Context.Channel.SendMessageAsync("Sikeresen használtad a parancsot, kérlek várj egy picit míg végrehajtom! <a:process:426000377314541568>");

            foreach (ITextChannel CurrentChannel in serverTextChannels)
            {
                await (CurrentChannel).RemovePermissionOverwriteAsync(user);
            }

            await Message.DeleteAsync();
            await Context.Channel.SendMessageAsync($"{user.Mention} unmuteolva lett!");
        }
        [Command("zár"), Alias("lock")]
        [Summary("Lezár egy adott channelt")]
        public async Task zárAsync(params string[] Arguments)
        {
            var everyone = Context.Guild.GetRole(418443717934841856);
            var Override = new OverwritePermissions(sendMessages: PermValue.Deny);
            var serverTextChannels = await Context.Guild.GetTextChannelsAsync();
            ITextChannel targetedChannel;

            if (Arguments.Length > 0)
            {
                var channelToFind = serverTextChannels.ToList().FindAll(t => t.Name.ToLower() == Arguments[0].ToLower());

                if (channelToFind.Count > 0)
                    targetedChannel = channelToFind.FirstOrDefault();
                else
                    targetedChannel = Context.Channel as ITextChannel;
            }
            else
                targetedChannel = Context.Channel as ITextChannel;

            await targetedChannel.AddPermissionOverwriteAsync(everyone, Override);

            //ez a line tesztelésre kell csak OWO (debug)        
            await Context.Channel.SendMessageAsync($"{targetedChannel} channel lezárva {Context.User.Username} által!");

        }
        [Command("cfelold"), Alias("unlock")]
        [Summary("Felold egy adott channelt")]
        public async Task unzárAsync(params string[] Arguments)
        {
            var everyone = Context.Guild.GetRole(418443717934841856);
            var Override = new OverwritePermissions(sendMessages: PermValue.Allow);
            var serverTextChannels = await Context.Guild.GetTextChannelsAsync();
            ITextChannel targetedChannel;

            if (Arguments.Length > 0)
            {
                var channelToFind = serverTextChannels.ToList().FindAll(t => t.Name.ToLower() == Arguments[0].ToLower());

                if (channelToFind.Count > 0)
                    targetedChannel = channelToFind.FirstOrDefault();
                else
                    targetedChannel = Context.Channel as ITextChannel;
            }
            else
                targetedChannel = Context.Channel as ITextChannel;

            await targetedChannel.AddPermissionOverwriteAsync(everyone, Override);

            //ez a line tesztelésre kell csak OWO (debug)        
            await Context.Channel.SendMessageAsync($"{targetedChannel} channel feloldva {Context.User.Username} által!");

        }
    }
}
