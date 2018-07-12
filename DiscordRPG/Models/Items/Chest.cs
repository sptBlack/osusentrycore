using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRPG.Models.Items
{
    public class Chest : BaseItem
    {
        public int c_100 { get; set; } = -2;
        public int c_200 { get; set; } = -2;
        public int c_500 { get; set; } = -2;
        public int c_1000 { get; set; } = -2;
        public int c_potion { get; set; } = -2;
        public int c_boost { get; set; } = -2;
        public int c_osu_supporter { get; set; } = -2;

        public int CoinAmmount { get; set; } = -2;
        
        public bool CanGetItem
        {
            get => c_100 != 0 || c_200 != 0 || c_500 != 0 || c_1000 != 0 || c_potion != 0 || c_boost != 0 || c_osu_supporter != 0;
        }

        public BaseItem Reward
        {
            get
            {
                if (!CanGetItem)
                    return null;

                int c_100 = this.c_100;
                int c_200 = c_100 + this.c_200;
                int c_500 = c_200 + this.c_500;
                int c_1000 = c_500 + this.c_1000;
                int c_potion = c_1000 + this.c_potion;
                int c_boost = c_potion + this.c_boost;
                int c_osu_supporter = c_boost + this.c_osu_supporter;

                int Random = DiscordRPG.RandomNumber.Next(0, c_osu_supporter);

                if (Random >= c_osu_supporter && this.c_osu_supporter != 0)
                {
                    return new Chest("osu! Supporter", Type: ItemType.osuChest);
                }
                else if (Random >= c_boost && this.c_boost != 0)
                {
                    return new XPBoost();
                }
                else if (Random >= c_potion && this.c_potion != 0)
                {
                    return new XPPotion();
                }
                else if (Random >= c_1000 && this.c_1000 != 0)
                {
                    return new Chest("Coin Chest", 1000, ItemType.CoinChest);
                }
                else if (Random >= c_500 && this.c_500 != 0)
                {
                    return new Chest("Coin Chest", 500, ItemType.CoinChest);
                }
                else if (Random >= c_200 && this.c_200 != 0)
                {
                    return new Chest("Coin Chest", 200, ItemType.CoinChest);
                }
                else
                {
                    return new Chest("Coin Chest", 100, ItemType.CoinChest);
                }
            }
        }

        public Chest(string Name, int CoinAmmount = -2, ItemType Type = ItemType.BaseChest) : base(Name, Type)
        {
            this.CoinAmmount = CoinAmmount;
        }
    }
}
