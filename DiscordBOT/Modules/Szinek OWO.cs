using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UserColor : ModuleBase
{
    [Command("szín"), Summary("Beállítja neked a kért színt. _(használd a **.color help** parancsot a színek listázásához)_")]
    public async Task SetColorAsync(params string[] SelectedColor)
    {
        if (SelectedColor.Length == 0)
        {
            await Context.Channel.SendMessageAsync(Context.User.Mention + " kérlek adj meg egy színt, vagy használd a **.szín help** parancsot a színek listázásához!");
        }
        else if (SelectedColor.Length == 1)

        {
            var ColorSource = File.ReadAllLines("Színek.txt");
            var Colors = ColorSource.Select(Source => new Tuple<string[], ulong>(
                Source.Substring(0, Source.IndexOf(':')).Split(','),
                Convert.ToUInt64(Source.Split(':').Last()))).ToList();

            if (SelectedColor[0].ToLower().Equals("help"))
            {
                string allColors = "";

                foreach (var colorList in Colors)
                    allColors += Context.Guild.GetRole(colorList.Item2).Mention + " Alias: **" + string.Join(", ", colorList.Item1) + "**\n";

                await Context.Channel.SendMessageAsync(Context.User.Mention + " __A választható színek:__\n\n" + allColors +
                    "\n\nHa el szeretnéd távolítani az egyedi színt, kérlek használd a **.színtörlés** parancsot!");
            }
            else
            {
                foreach (var colorSet in Colors)
                {
                    if (colorSet.Item1.Contains(SelectedColor[0].ToLower()))
                    {
                        foreach (var colorCheck in Colors)
                        {
                            if ((Context.User as IGuildUser).RoleIds.Contains(colorCheck.Item2))
                            {
                                await (await Context.Guild.GetUserAsync(Context.User.Id)).RemoveRoleAsync(Context.Guild.GetRole(colorCheck.Item2));
                            }
                        }

                        await Context.Guild.GetUserAsync(Context.User.Id).Result.AddRoleAsync(Context.Guild.GetRole(colorSet.Item2));
                        await Context.Channel.SendMessageAsync(Context.User.Mention + " mostantól **" + colorSet.Item1.First() + "** színű vagy!");
                        return;
                    }
                }

                await Context.Channel.SendMessageAsync(Context.User.Mention + " nem találtam az általad kért színt! Kérlek használd a **.szín help** parancsot a színek listázásához!");
            }
        }
        else
        {
            await Context.Channel.SendMessageAsync(Context.User.Mention + " kérlek, csak egy színt adj meg!");
        }
    }

    [Command("színbővítés"), Summary("Hozzá adja az említett role ID-jét az egyedi szín listához.")]
    public async Task AddColorAsync(params string[] Color)
    {
        if (!UserHasPermission((SocketGuildUser)Context.User, "Admin"))
        {
            await Context.Channel.SendMessageAsync(":x: Nincs megfelelő jogosultságod hozzá, hogy ezt a parancsot használd! " + Context.User.Mention);
            return;
        }

        if (Color.Length == 2)
        {
            bool Argument = UInt64.TryParse(Color[1], out ulong ID);

            if (Argument)
            {
                if (Context.Guild.Roles.Select(t => t.Id).ToList().Contains(ID))
                {
                    File.AppendAllText("Színek.txt", Environment.NewLine + Color[0] + ":" + ID, Encoding.Unicode);
                    await Context.Channel.SendMessageAsync(Context.User.Mention + " hozzáadtam a(z) " + Context.Guild.GetRole(ID).Mention + " színt a fájlhoz, a következő nevekkel: **" + Color[0] + "**");
                }
                else
                {
                    await Context.Channel.SendMessageAsync(Context.User.Mention + " nem találtam az említett role ID-t a szerver role-jai között!");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync(Context.User.Mention + " nem tudtam konvertálni az említett role ID-t! Kérlek, ellenőrizd!");
            }
        }
        else
        {
            await Context.Channel.SendMessageAsync(Context.User.Mention + " kérlek, szolgáltass helyes paramétereket a művelet végrehajtásához!");
        }
    }

    [Command("színtörlés"), Summary("Eltávolítja a beállított egyedi színt, hogy a fő role-od színét használhasd újra.")]
    public async Task RemoveColorAsync(params string[] Addition)
    {
        var ColorSource = File.ReadAllLines("Színek.txt");
        var Colors = ColorSource.Select(Source => new Tuple<string[], ulong>(
            Source.Substring(0, Source.IndexOf(":")).ToString().Split(','),
            Convert.ToUInt64(Source.Split(':').Last()))).ToList();

        foreach (var colorCheck in Colors)
        {
            if ((Context.User as IGuildUser).RoleIds.Contains(colorCheck.Item2))
            {
                await Context.Guild.GetUserAsync(Context.User.Id).Result.RemoveRoleAsync(Context.Guild.GetRole(colorCheck.Item2));
                await Context.Channel.SendMessageAsync(Context.User.Mention + " eltávolítottam az egyedi színt, mostantól a fő role-od színét használod újra!");
                return;
            }
        }

        await Context.Channel.SendMessageAsync(Context.User.Mention + " nem találtam nálad egyedi színt, amit el tudnék távolítani!");
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

}
