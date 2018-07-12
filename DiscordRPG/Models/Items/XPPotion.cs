using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRPG.Models.Items
{
    public class XPPotion : BaseItem
    {
        /// <summary>
        /// Base XP Potion item
        /// </summary>
        /// <param name="Name">The name of the item</param>
        /// <param name="Type">The type of the item</param>
        public XPPotion(string Name = "XP Potion", ItemType Type = ItemType.XPPotion) : base(Name, Type)
        {

        }

        public int Use(int UserXP)
        {
           return (int)(UserXP * 0.1);
        }
    }
}
