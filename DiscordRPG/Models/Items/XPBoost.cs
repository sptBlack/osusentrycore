using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRPG.Models.Items
{
    public class XPBoost : BaseItem
    {
        /// <summary>
        /// How many times it will be applied
        /// </summary>
        public int Times { get; set; } = -2;

        /// <summary>
        /// Returns whether the boost is activated
        /// </summary>
        public bool Activated { get; set; }

        /// <summary>
        /// Base XP Boost Item
        /// </summary>
        /// <param name="BoostPercent">100 = base; more than 100 = more xp, less than 100 = less xp</param>
        /// <param name="Times">How many messages will be applied to</param>
        /// <param name="Name">The name of the item</param>
        /// <param name="Type">The type of the item</param>
        public XPBoost(string Name = "XP Boost", ItemType Type = ItemType.XPBoost) : base(Name, Type)
        {
            Times = DiscordRPG.RandomNumber.Next(80, 100);
        }
    }
}
