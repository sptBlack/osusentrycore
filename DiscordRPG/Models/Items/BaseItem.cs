using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRPG.Models.Items
{
    public enum ItemType
    {
        BaseChest = 0,
        XPPotion = 1,
        XPBoost = 2,
        osuChest = 3,
        CoinChest = 4
    }

    public class BaseItem
    {
        /// <summary>
        /// The name of the item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the item
        /// </summary>
        public ItemType Type { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Name">The name of the item</param>
        /// <param name="Type">The type of the item</param>
        public BaseItem(string Name, ItemType Type)
        {
            this.Name = Name;
            this.Type = Type;
        }
    }
}
