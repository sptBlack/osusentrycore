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
    public class osu__Verify_System : ModuleBase
    {
        [Command("hitelesítés")]
        public async Task verify (params string [] Args)
        {
            var member = Context.Guild.GetRole(418443717934841856);
            //await Context.Guild.

        }
    }
}
