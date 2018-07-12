using DiscordRPG.Models.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRPG.Modules
{
    public class ItemList
    {
        /// <summary>
        /// The file where to safe
        /// </summary>
        private string DatabaseFile { get; set; }

        /// <summary>
        /// The items
        /// </summary>
        public List<ListItem> Items { get; private set; }

        /// <summary>
        /// The constructor
        /// </summary>
        public ItemList(string DatabaseFile = "Items.json")
        {
            this.DatabaseFile = DatabaseFile;
            Items = new List<ListItem>();
            
            if (File.Exists(DatabaseFile))
            {
                Items = JsonConvert.DeserializeObject<List<ListItem>>(File.ReadAllText(DatabaseFile));
            }
            else
            {
                Save();
            }
        }

        /// <summary>
        /// Add an item to the Item List
        /// </summary>
        /// <param name="Item">The item itself</param>
        /// <param name="Name">The name of the item</param>
        /// <param name="Price">The price of the item: -1 = can't buy or sell</param>
        public void AddItem(BaseItem Item, int BuyPrice, int SellPrice)
        {
            Items.Add(new ListItem(Item, BuyPrice, SellPrice));

            Save();
        }

        /// <summary>
        /// Saves the list
        /// </summary>
        private void Save()
        {
            if (File.Exists(DatabaseFile))
                File.Delete(DatabaseFile);

            File.WriteAllText(DatabaseFile, JsonConvert.SerializeObject(Items, Formatting.Indented, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            }));
        }
    }
    public class ListItem
    {
        /// <summary>
        /// The buy price of the item
        /// </summary>
        public int BuyPrice { get; set; }
        /// <summary>
        /// The sell price of the item
        /// </summary>
        public int SellPrice { get; set; }
        /// <summary>
        /// The item itself
        /// </summary>
        public BaseItem Item { get; set; }

        /// <summary>
        /// Default constructor for Newtonsoft (better be safe)
        /// </summary>
        public ListItem()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Item">The item</param>
        /// <param name="Name">The name of the item</param>
        /// <param name="BuyPrice">The buy price</param>
        /// <param name="SellPrice">The sell price</param>
        public ListItem(BaseItem Item, int BuyPrice, int SellPrice)
        {
            this.BuyPrice = BuyPrice;
            this.SellPrice = SellPrice;
            this.Item = Item;
        }
    }
    public static class ItemListExtension
    {
        /// <summary>
        /// Gets the item by name
        /// </summary>
        /// <param name="Items">The item list</param>
        /// <param name="Name">The name of the item (not case sensitive)</param>
        /// <returns>The item if there's a matching name, otherwise null</returns>
        public static ListItem GetItemByName(this ItemList Items, string Name)
        {
            string Lower = Name.ToLower();
            for (int i = 0; i < Items.Items.Count; i++)
            {
                if (Items.Items[i].Item.Name.ToLower() == Lower) return Items.Items[i];
            }
            return null;
        }
    }
}
