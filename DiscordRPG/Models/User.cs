using Discord;
using DiscordRPG.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRPG.Models
{
    public class User
    {
        /*
         * Events
         */
        /// <summary>
        /// Triggers when new level is reached
        /// </summary>
        public event Action<User, int, ITextChannel> NewLevel;
        
        /// <summary>
        /// The id of the user
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// The inventory of the user
        /// </summary>
        public List<BaseItem> Items { get; set; }

        /// <summary>
        /// How many coins the user has
        /// </summary>
        public int Coin { get; set; }

        /// <summary>
        /// How much xp the user has
        /// </summary>
        public int XP { get; set; }

        /// <summary>
        /// The current level of the user
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// User constructor
        /// </summary>
        /// <param name="UserId">The id of the user</param>
        public User(ulong UserId)
        {
            this.UserId = UserId;
            Items = new List<BaseItem>();
            Coin = 10;
            XP = 0;
            Level = 1;
        }
        public User() { }

        /// <summary>
        /// Transfer item between users
        /// </summary>
        /// <param name="ItemIndex">The index of the Item</param>
        /// <param name="To">Who gains the item</param>
        public void TransferItem(int ItemIndex, ref User To)
        {
            BaseItem Item = Items[ItemIndex];
            Items.RemoveAt(ItemIndex);
            To.Items.Add(Item);
        }

        /// <summary>
        /// Adds item to the user
        /// </summary>
        /// <param name="Item">The item</param>
        public void AddItem(BaseItem Item)
        {
            Items.Add(Item);
        }

        /// <summary>
        /// Activates an item
        /// </summary>
        /// <param name="Name">The name of the item</param>
        /// <returns>True if success, otherwise false</returns>
        public bool ActivateItem(string Name, ITextChannel Channel)
        {
            string Lower = Name.ToLower();
            return ActivateItem(Items.FindIndex(t => t.Name.ToLower() == Lower), Channel);
        }

        /// <summary>
        /// Activates an item
        /// </summary>
        /// <param name="InventoryIndex">The index of the item</param>
        /// <returns>True if success, otherwise false</returns>
        public bool ActivateItem(int InventoryIndex, ITextChannel Channel)
        {
            if (InventoryIndex < 0 || Items.Count >= InventoryIndex)
                return false;

            BaseItem Item = Items[InventoryIndex];
            Items.RemoveAt(InventoryIndex);
            
            if (Item.Type == ItemType.osuChest)
            {
                Channel.SendMessageAsync($"<@120243459666870272> Gib osu!supporter to <@{ UserId }>");
            }
            else if (Item.Type == ItemType.CoinChest)
            {
                int Ammount = (Item as Chest).CoinAmmount;
                AddCoin(Ammount);
                Channel.SendMessageAsync($"<@{ UserId }> kaptál { Ammount } coin-t!");
            }
            else if (Item.Type == ItemType.BaseChest)
            {
                BaseItem NewItem = (Item as Chest).Reward;
                AddItem(NewItem);
            }
            else if (Item.Type == ItemType.XPPotion)
            {
                XPPotion Potion = Item as XPPotion;
                AddXP(Potion.Use(XP), Channel);
            }
            else if (Item.Type == ItemType.XPBoost)
            {
                XPBoost Boost = Item as XPBoost;
                if (Boost.Activated)
                {
                    Channel.SendMessageAsync($"Ez az item már aktiválva van, hátra van még { Boost.Times } boost!");
                }
                else
                {
                    (Items[InventoryIndex] as XPBoost).Activated = true;
                    Channel.SendMessageAsync("Aktiváltad az XP boostot!");
                }
            }
            return true;
        }
        
        /// <summary>
        /// Add XP to the user
        /// </summary>
        /// <param name="Ammount">The ammount of the gained XP</param>
        public void AddXP(int Ammount, ITextChannel Channel)
        {
            XP += Ammount;

            int CurrentLevelXP = DiscordRPG.XPLevelTable[Level - 1];

            if (XP > CurrentLevelXP)
            {
                Level++;
                if (DiscordRPG.XPLevelTable.Count <= (Level + 1))
                    DiscordRPG.GenerateNextLevel();

                NewLevel?.Invoke(this, DiscordRPG.XPLevelTable[Level - 1], Channel);
            }
        }

        /// <summary>
        /// Add coins to the actual user
        /// </summary>
        /// <param name="Ammount">The ammount of the coin</param>
        public void AddCoin(int Ammount)
        {
            Coin += Ammount;
        }
    }
}
