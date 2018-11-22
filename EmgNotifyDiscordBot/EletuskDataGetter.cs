using Discord;
using Discord.WebSocket;
using Mastonet;
using Mastonet.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmgNotifyDiscordBot {
    public class EletuskDataGetter {

        DiscordSocketClient dicordClient;

        public EletuskDataGetter(DiscordSocketClient client) {
            this.dicordClient = client;
        }

        public async Task Stream() {
            var appRegistration = new AppRegistration() {
                Instance = "eletusk.club",
                ClientId = Configration.Instance.Datas.EletuskClientID,
                ClientSecret = Configration.Instance.Datas.EletuskClientSecret
            };

            var authClient = new AuthenticationClient(appRegistration);
            var auth = await authClient.ConnectWithPassword(Configration.Instance.Datas.Email, Configration.Instance.Datas.Pass);

            var client = new MastodonClient(appRegistration, auth);

            var streaming = client.GetUserStreaming();

            streaming.OnUpdate += (sender, e) => {
                //if (e.Status.Account.AccountName != "elebot1st") return;
                //string content = Regex.Replace(e.Status.Content, @"<(p|/p|br /)>", "");
                //if (!content.Contains("PSO2緊急クエスト予告")) return;

                string[] servers = new string[10];
                for (int i = 0; i < 10; i++) servers[i] = "―";
                
                dicordClient.GetGuild(427091125170601985).GetTextChannel(427101602093072384).SendMessageAsync("**これはテスト投稿です**", false, CreateEmbed(null, servers));
            };

            await streaming.Start();
        }

        public Embed CreateEmbed(string notice, string[] servers) {
            var builder = new EmbedBuilder {
                Title = $"{DateTime.Now.AddHours(1).ToString("HH")} 時の緊急クエストっきゅ",
                Color = Color.Gold,
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder().WithName("予告緊急").WithValue(notice ?? "予告緊急はありません。").WithIsInline(false),
                    new EmbedFieldBuilder().WithName("1鯖").WithValue(servers[0]).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("2鯖").WithValue(servers[1]).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("3鯖").WithValue(servers[2]).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("4鯖").WithValue(servers[3]).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("5鯖").WithValue(servers[4]).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("6鯖").WithValue(servers[5]).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("7鯖").WithValue(servers[6]).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("8鯖").WithValue(servers[7]).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("9鯖").WithValue(servers[8]).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("10鯖").WithValue(servers[9]).WithIsInline(true)
                }
            };
            return builder.Build();
        }
    }
}
