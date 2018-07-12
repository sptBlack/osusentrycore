using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordRPG.Models.Items;
using DiscordRPG.Modules;

namespace DiscordBOT.Models
{
    public class Question
    {
        public string _Question { get; private set; }
        public string Answer { get; private set; }

        public IUserMessage Message { get; private set; }

        public BaseItem Reward { get; private set; }
        public int Coins { get; private set; }

        public Question(string _Question, string Answer, int Coins = 0, string Reward = null)
        {
            this._Question = _Question;
            this.Answer = Answer;
            this.Coins = Coins;

            if (Reward != null)
            {
                foreach (ListItem Item in DiscordRPG.DiscordRPG.Items.Items)
                {
                    if (Item.Item.Name == Reward)
                    {
                        this.Reward = Item.Item;
                        break;
                    }
                }
            }
        }

        public async Task SendAsync(ITextChannel Channel, int TimeLimit = 60)
        {
            int ReduceTime = 15;
            while (TimeLimit > 0)
            {
                if (Message == null)
                {
                    Message = await Channel.SendMessageAsync("", embed: CreateEmbed(TimeLimit).Build());
                }
                else
                {
                    await Message.ModifyAsync(Msg =>
                    {
                        Msg.Embed = CreateEmbed(TimeLimit).Build();
                    });
                }
                await Task.Delay(ReduceTime * 1000);

                if (Answer == null)
                {
                    break;
                }
                TimeLimit -= ReduceTime;
            }

            Program.CurrentQuestion = null;
        }

        public bool Check(string Answer)
        {
            bool Answered = this.Answer.ToLower() == Answer.ToLower();
            Answer = null;
            return Answered;
        }

        private EmbedBuilder CreateEmbed(int LeftTime)
        {
            EmbedBuilder Builder = new EmbedBuilder()
            {
                Title = "Kérdés",
                Description = _Question + "\r\nJutalom:",
                Color = new Color(90, 60, 90),
                Footer = new EmbedFooterBuilder()
                {
                    Text = "⏳ Még ennyi idő maradt hátra: " + LeftTime + " másodperc! ⏳"
                }
            };

            if (Coins != 0)
                Builder.Description += " " + Coins + " coin";
            if (Reward != null)
                Builder.Description += (Coins == 0 ? " " : " + ") + Reward.Name + "";
            if (Coins == 0 && Reward == null)
                Builder.Description += " Semmi OMEGALUL";

            return Builder;
        }
    }
}
