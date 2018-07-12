using Discord;
using Discord.WebSocket;
using DiscordRPG.Models;
using DiscordRPG.Models.Items;
using DiscordRPG.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRPG
{
    public class DiscordRPG
    {
        internal static List<int> XPLevelTable;

        public int GainedXPMin { get; set; }
        public int GainedXPMax { get; set; }

        public static ItemList Items { get; set; }

        public event Action<User, int, ITextChannel> LevelUp;

        internal static Random RandomNumber { get; set; }

        public Database.Database UserDatabase;

        /// <summary>
        /// The main constructor of the RPG system
        /// </summary>
        /// <param name="Client">The where it should be applied</param>
        public DiscordRPG(DiscordSocketClient Client, int GainedXPMin = 10, int GainedXPMax = 20,
            string DatabaseFile = "Users.json", string ItemsFile = "Items.json", bool BotProtection = true)
        {
            RandomNumber = new Random(new Random().Next());

            XPLevelTable = new List<int>()
            {
                100
            };
            Items = new ItemList(ItemsFile);

            UserDatabase = new Database.Database(DatabaseFile);

            int MaxLevel = 1;

            for (int i = 0; i < UserDatabase.Users.Count; i++)
            {
                UserDatabase.Users[i].NewLevel += UserNewLevel;

                if (UserDatabase.Users[i].Level > MaxLevel)
                    MaxLevel = UserDatabase.Users[i].Level;
            }

            for (int i = 1; i < MaxLevel; i++)
            {
                GenerateNextLevel();
            }

            this.GainedXPMin = GainedXPMin;
            this.GainedXPMax = GainedXPMax;
            
            Client.MessageReceived += (Message) =>
            {
                if (Message.Author.IsBot && BotProtection)
                    return Task.CompletedTask;

                if (!UserDatabase.HasUser(Message.Author.Id))
                {
                    User NewUser = new User(Message.Author.Id);
                    NewUser.NewLevel += UserNewLevel;
                    UserDatabase.AddUser(NewUser);
                }

                User User = UserDatabase.FindUser(Message.Author.Id);

                int XP = RandomNumber.Next(GainedXPMin, GainedXPMax);

                // Check whether the user have xp boost
                List<XPBoost> XPBoosts = User.Items.FindAll(t => t.Type == ItemType.XPBoost).Select(t => t as XPBoost)
                                            .Where(t => t.Activated).ToList();
                if (XPBoosts.Count > 0)
                {
                    int Index = User.Items.FindIndex(t => t == XPBoosts[0]);
                    XPBoost Boost = User.Items[Index] as XPBoost;

                    XP = XP * 2;
                    Boost.Times--;

                    if (Boost.Times == 0)
                    {
                        User.Items.RemoveAt(Index);
                    }
                    else
                    {
                        User.Items[Index] = Boost;
                    }
                }

                User.AddXP(XP, Message.Channel as ITextChannel);
                UserDatabase.UpdateUser(User);
                return Task.CompletedTask;
            };
        }

        private void UserNewLevel(User User, int NextLevelXP, ITextChannel Channel)
        {
            LevelUp?.Invoke(User, NextLevelXP, Channel);
        }

        public int LevelOf(int Xp)
        {
            return XPLevelTable.IndexOf(Xp) + 1;
        }
        public int GetXPByLevel(int Level)
        {
            return XPLevelTable[Level - 1];
        }

        internal static void GenerateNextLevel()
        {
            int Last = XPLevelTable[XPLevelTable.Count - 1];
            XPLevelTable.Add((int)(Last * 2.5));
        }
    }
}
