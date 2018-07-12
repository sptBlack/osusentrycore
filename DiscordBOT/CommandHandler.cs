using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using DiscordRPG.Models;
using DiscordBOT.Models;

namespace DiscordBOT
{
    class CommandHandler
    {
        DiscordSocketClient _client;
        CommandService _service;

        CommandServiceProvider Provider;

        private int PropertyIndex { get; set; } = 0;
        private int BuyPrice { get; set; } = -2;
        private int SellPrice { get; set; } = -2;


        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService(new CommandServiceConfig()
            {
                DefaultRunMode = RunMode.Async
            });
            Provider = new CommandServiceProvider();
            
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), Provider);
            _client.MessageReceived += _HandleCommandAsync;
        }

        private async Task _HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            if (msg == null) return;
            var context = new SocketCommandContext(_client, msg);
            if (context.User.IsBot) return;

            if (Program.CurrentQuestion != null && msg.Channel.Id == Program.QuestionChannel)
            {
                if (Program.CurrentQuestion.Check(msg.Content))
                {
                    User User = Program.DiscordRPG.UserDatabase.FindUser(s.Author.Id);
                    if (Program.CurrentQuestion.Reward != null)
                        User.AddItem(Program.CurrentQuestion.Reward);
                    
                    User.AddCoin(Program.CurrentQuestion.Coins);
                    Program.DiscordRPG.UserDatabase.UpdateUser(User);

                    await s.Channel.SendMessageAsync($"<@{ s.Author.Id }> megszerezte ennek a kérdésnek a jutalmát!");

                    Program.CurrentQuestion = null;
                }
                return;
            }
            if (Program.CurrentConfiguredItem != null && msg.Channel.Id == Program.ItemChannel)
            {
                PropertyInfo[] Properties = Program.CurrentConfiguredItem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                if (PropertyIndex == Properties.Length + 2)
                {
                    string Lower = msg.Content.ToLower();
                    if (Lower == "kész" || Lower == "done")
                    {
                        PropertyIndex = 0;
                        DiscordRPG.DiscordRPG.Items.AddItem(Program.CurrentConfiguredItem, BuyPrice, SellPrice);
                        Program.CurrentConfiguredItem = null;
                    }
                    else if (Lower.StartsWith("módosít") || Lower.StartsWith("edit"))
                    {
                        string[] Values = msg.Content.Split(' ');
                        if (Values.Length == 2)
                        {
                            Tuple<bool, int> Success = ParseInt(Values[1]);

                            if (Success.Item1)
                            {
                                PropertyIndex = Success.Item2;
                            }
                        }
                    }
                    else if (Lower == "mégse" || Lower == "cancel")
                    {
                        Program.CurrentConfiguredItem = null;
                        PropertyIndex = 0;
                        await msg.Channel.SendMessageAsync("Elvetve!");
                        return;
                    }
                    else
                    {
                        await msg.Channel.SendMessageAsync(@"```"
                            + "Ezek a parancsok:\r\n"
                            + "kész / done: véglegesíti az itemet, és hozzáadja a listához\r\n"
                            + "módosít / edit [szám]: indexszerűen szerkeszthető adattagok\r\n"
                            + "mégse / cancel: item elvetése\r\n"
                            + "```");
                    }
                }

                if (PropertyIndex < Properties.Length)
                {
                    PropertyInfo Info = Properties[PropertyIndex];
                    if (Info.PropertyType == typeof(string))
                    {
                        Info.SetValue(Program.CurrentConfiguredItem, msg.Content);
                        await msg.Channel.SendMessageAsync($"Beállítottad a(z) { Info.Name } változónak az értékét!");
                        PropertyIndex++;
                    }
                    else if (Info.PropertyType == typeof(int))
                    {
                        Tuple<bool, int> ParseResult = ParseInt(msg.Content);
                        if (ParseResult.Item1)
                        {
                            Info.SetValue(Program.CurrentConfiguredItem, ParseResult.Item2);
                            await msg.Channel.SendMessageAsync($"Beállítottad a(z) { Info.Name } változónak az értékét!");
                            PropertyIndex++;
                        }
                        else
                        {
                            await msg.Channel.SendMessageAsync($"A { Info.Name } nevű változó egész számot vár!");
                        }
                    }
                }
                else if (PropertyIndex == Properties.Length)
                {
                    Tuple<bool, int> ParseResult = ParseInt(msg.Content);
                    if (ParseResult.Item1)
                    {
                        BuyPrice = ParseResult.Item2;
                        await msg.Channel.SendMessageAsync($"Beállítottad a(z) BuyPrice változónak az értékét!");
                    }
                    else
                    {
                        await msg.Channel.SendMessageAsync($"A BuyPrice nevű változó egész számot vár!");
                    }
                }
                else if (PropertyIndex == Properties.Length + 1)
                {
                    Tuple<bool, int> ParseResult = ParseInt(msg.Content);
                    if (ParseResult.Item1)
                    {
                        SellPrice = ParseResult.Item2;
                        await msg.Channel.SendMessageAsync($"Beállítottad a(z) SellPrice változónak az értékét!");
                    }
                    else
                    {
                        await msg.Channel.SendMessageAsync($"A SellPrice nevű változó egész számot vár!");
                    }
                }


                bool Break = false;
                for (; PropertyIndex < Properties.Length; PropertyIndex++)
                {
                    PropertyInfo Info = Properties[PropertyIndex];
                    object Value = Info.GetValue(Program.CurrentConfiguredItem);
                    if (Value is string StringValue)
                    {
                        if (StringValue == null)
                        {
                            await msg.Channel.SendMessageAsync($"Kérlek add meg a(z) { Info.Name } értékét!");
                            Break = true;
                        }
                    }
                    else if (Value is int IntValue)
                    {
                        if (IntValue == -2)
                        {
                            await msg.Channel.SendMessageAsync($"Kérlek add meg a(z) { Info.Name } értékét!");
                            Break = true;
                        }
                    }

                    if (Break)
                        break;
                }

                if (!Break && Properties.Length - 1 == PropertyIndex)
                {
                    PropertyIndex++;
                }

                if (PropertyIndex >= Properties.Length)
                {
                    if (BuyPrice < -1)
                    {
                        await msg.Channel.SendMessageAsync("Kérlek add meg a BuyPrice értékét:");
                        PropertyIndex = Properties.Length;
                    }
                    else if (SellPrice < -1)
                    {
                        await msg.Channel.SendMessageAsync("Kérlek add meg a SellPrice értékét:");
                        PropertyIndex = Properties.Length + 1;
                    }
                    else
                    {
                        PropertyIndex = Properties.Length + 2;
                        string Text = "```";

                        for (int j = 0; j < Properties.Length; j++)
                        {
                            PropertyInfo Info = Properties[j];
                            Text += $"[{ j }] { Info.Name }: { Info.GetValue(Program.CurrentConfiguredItem) }\r\n";
                        }
                        Text += $"[{ Properties.Length }] Buy price: { BuyPrice }\r\n";
                        Text += $"[{ Properties.Length + 1 }] Sell price: { SellPrice }\r\n";

                        Text += "```";

                        await msg.Channel.SendMessageAsync("Minden érték beállítva! A végleges item tulajdonságai: " + Text);
                    }
                }

                

                return;
            }
            
            int argPos = 0;
            if(msg.HasStringPrefix(Config.bot.cmdPrefix, ref argPos))
            {
                var result = await _service.ExecuteAsync(context, argPos, Provider);
                if(!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    Console.WriteLine(result.ErrorReason);
                }
            }
        }


        private Tuple<bool, int> ParseInt(string Content)
        {
            bool Success = int.TryParse(Content, out int Result);
            return new Tuple<bool, int>(Success, Result);
        }
    }
}
