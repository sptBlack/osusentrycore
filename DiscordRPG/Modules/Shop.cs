using DiscordRPG.Models;
using DiscordRPG.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRPG.Modules
{
    public enum BuyResult
    {
        NoSuchItem,
        NotEnoughCoin,
        Success
    }
    public enum SellResult
    {
        Activated,
        NoSuchItem,
        Success
    }
    public class Shop
    {
        public Shop()
        {

        }

        /// <summary>
        /// Buy an item from the shop
        /// </summary>
        /// <param name="User">The user who wants to buy the item</param>
        /// <param name="ItemName">The item name</param>
        /// <returns>The enum names speaks for themselves</returns>
        public BuyResult Buy(ref User User, string ItemName)
        {
            ListItem Item = DiscordRPG.Items.GetItemByName(ItemName);

            if (Item == null)
                return BuyResult.NoSuchItem;

            if (User.Coin < Item.BuyPrice)
                return BuyResult.NotEnoughCoin;

            User.AddCoin(-Item.BuyPrice);
            User.AddItem(Item.Item);

            return BuyResult.Success;
        }
        /// <summary>
        /// Sell and item to the shop
        /// </summary>
        /// <param name="User">The user who wants to sell</param>
        /// <param name="Index">The index of the item from the inventory</param>
        /// <returns>The enum names speaks for themselves</returns>
        public SellResult Sell(ref User User, int Index)
        {
            if (Index < 0 || User.Items.Count >= Index)
                return SellResult.NoSuchItem;

            BaseItem Item = User.Items[Index];

            if (Item is XPBoost)
                if ((Item as XPBoost).Activated)
                    return SellResult.Activated;

            User.Items.RemoveAt(Index);
            ListItem ListItem = DiscordRPG.Items.GetItemByName(Item.Name);

            User.AddCoin(ListItem.SellPrice);

            return SellResult.Success;
        }
    }
}
