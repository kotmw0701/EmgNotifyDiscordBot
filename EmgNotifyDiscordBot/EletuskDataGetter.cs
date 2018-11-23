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
                if (e.Status.Account.AccountName != "elebot1st") return;
                string content = Regex.Replace(e.Status.Content.Replace("</p><p>", "|"), @"<(p|/p)>", "").Replace("<br />", "|").Replace("#PSO2", "");
                if (!content.Contains("PSO2緊急クエスト予告")) return;
                var (notice, servers, league, nowLeague) = ParseData(content);
                dicordClient.GetGuild(427091125170601985).GetTextChannel(427101602093072384).SendMessageAsync("", false, CreateEmbed(notice, servers, league, nowLeague));
            };

            await streaming.Start();
        }

        public Embed CreateEmbed(string notice, string[] servers, string league, bool nowLeague) {
            var builder = new EmbedBuilder {
                Title = $"{DateTime.Now.AddHours(1).ToString("HH")} 時の緊急クエストっきゅ",
                Color = Color.Gold
            };
            builder.AddField("予告緊急", string.IsNullOrEmpty(notice) ? "予告緊急はありません" : notice);
            List<string> checker = new List<string>(servers);
            checker.RemoveAll(check => string.IsNullOrEmpty(check));
            if(checker.Count > 0) for (int i = 0; i < 10; i++) builder.AddInlineField($"{i+1}鯖", string.IsNullOrEmpty(servers[i]) ? "―" : servers[i]);
            builder.AddField(nowLeague ? "⚠アークスリーグ開催中⚠" : "アークスリーグ予定", league);
            return builder.Build();
        }


        public (string notice, string[] servers, string league, bool nowLeague) ParseData(string text) {
            string[] brArray = text.Split("|");
            string[] servers = new string[10];
            StringBuilder league = new StringBuilder(), notice = new StringBuilder();
            bool nowLeague = false;
            for (int i = 1; i < brArray.Length; i++) {
                string line = brArray[i];
                if (Regex.IsMatch(line, "アークスリーグ")) {
                    nowLeague = line.Contains("開催中");
                    league.AppendLine(brArray[i + 1]).AppendLine(brArray[i + 2]);
                    break;
                }
                if (Regex.IsMatch(line, @"^\d{2}")) {//ランダム緊急
                    string[] split = line.Split(":");
                    int num = Int32.Parse(split[0])-1;
                    servers[num] = Regex.IsMatch(split[1], "[発生中.*]") ? "" : split[1];
                } else if (Regex.IsMatch(line, "^【.*】")) //予告緊急
                    notice.AppendLine(Regex.Replace(line, "^【.*】", ""));
            }
            return (notice.ToString(), servers, league.ToString(), nowLeague);
        }
    }
}
