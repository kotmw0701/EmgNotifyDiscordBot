using Discord;
using Discord.Rest;
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

        private bool betaTest = true;

        private DiscordSocketClient dicordClient;
        private RestUserMessage restMessage;

        private (string notice, string[] servers, string league, bool nowLeague, bool isFollow) embedData;

        public EletuskDataGetter(DiscordSocketClient client) {
            this.dicordClient = client;
        }

        /// <summary>
        /// EltuskのStreamingとメッセージの受け取りを開始する
        /// </summary>
        /// <returns></returns>
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

            streaming.OnUpdate += async (sender, e) => await OnMessageRecieve(sender, e);

            await streaming.Start();
        }

        private async Task OnMessageRecieve(object sender, StreamUpdateEventArgs args) {
            if (args.Status.Account.AccountName != "elebot1st") return;
			Console.WriteLine("Received");
			string content = Regex.Replace(args.Status.Content.Replace("</p><p>", "|"), @"<(p|/p)>", "").Replace("<br />", "|");
			if (content.IndexOf("|") < 0) return;
            string head = content.Substring(0, content.IndexOf("|"));
            content = content.Substring(content.IndexOf("|") + 1, content.Length - content.IndexOf("|") - 1);
            if (Regex.IsMatch(head, $"{DateTime.Now.ToString("HH")}:\\d{{2}}続報")) {
                embedData.isFollow = true;
                embedData.servers = FollowData(Regex.Replace(content, @"<a.*/a>", ""), embedData.servers);
                await restMessage.DeleteAsync();
            } else if (!head.Contains("PSO2緊急クエスト予告")) return;
            else embedData = ParseData(content);
            restMessage = await dicordClient.GetGuild(427091125170601985).GetTextChannel(427101602093072384)
                                .SendMessageAsync(betaTest ? "**現在テスト中です**" : "", false, CreateEmbed(embedData.notice, embedData.servers, embedData.league, embedData.nowLeague, embedData.isFollow));
			Console.WriteLine("Sended");
		}

        public Embed CreateEmbed(string notice, string[] servers, string league, bool nowLeague, bool isFollow) {
            DateTime time = DateTime.Now;
            Console.WriteLine($"Time    : {time.ToString("yyMMddHH")}");
            Console.WriteLine($"Notice  : {notice.Replace(Environment.NewLine, " ")}");
            Console.WriteLine($"Servers : [{string.Join(" , ", servers)}]");
            Console.WriteLine($"League  : {league.Replace(Environment.NewLine, " ")}");
            var builder = new EmbedBuilder {
                Title = $"{time.AddHours(1).ToString("HH")} 時の緊急クエストです",
                Description = isFollow ? "❗❗続報がありました❗❗" : "",
                Color = betaTest ? Color.Red : Color.Gold,
                Author = new EmbedAuthorBuilder().WithName("エレボット1号")
                        .WithUrl(@"https://eletusk.club/@elebot1st")
                        .WithIconUrl(@"https://eletusk.club/system/accounts/avatars/000/001/642/original/ec186395ae173203.png")
            };
			if (!string.IsNullOrEmpty(notice)) builder.AddField("予告緊急", notice);
			List<string> checker = new List<string>(servers);
            checker.RemoveAll(check => string.IsNullOrEmpty(check));
            if (checker.Count > 0) for (int i = 0; i < 10; i++) builder.AddField($"{i + 1}鯖", string.IsNullOrEmpty(servers[i]) ? "―" : servers[i], true);
            if (!string.IsNullOrEmpty(league)) builder.AddField(nowLeague ? "⚠アークスリーグ開催中⚠" : "アークスリーグ予定", league);
			Console.WriteLine("Builded");
            return builder.Build();
        }


        public (string notice, string[] servers, string league, bool nowLeague, bool isFollow) ParseData(string text) {
            string[] brArray = text.Split("|");
            string[] servers = new string[10];
            StringBuilder league = new StringBuilder(), notice = new StringBuilder();
            bool nowLeague = false;
            for (int i = 0; i < brArray.Length; i++) {
                string line = brArray[i];
                if (Regex.IsMatch(line, "アークスリーグ")) {
                    nowLeague = line.Contains("開催中");
                    league.AppendLine(brArray[i + 1]).AppendLine(brArray[i + 2]);
                    break;
                }
                if (Regex.IsMatch(line, @"^\d{2}")) {//ランダム緊急
                    int num = Int32.Parse(Regex.Matches(line, @"^\d{2}")[0].Value) - 1;
                    string emg = line.Substring(3);
                    if (Regex.IsMatch(emg, @"^\[発生中.*\]")
                        || Regex.IsMatch(emg, @"^\(\d{2}.*\)")) continue;
                    servers[num] = emg;
                } else if (Regex.IsMatch(line, "^【.*】")) //予告緊急
                    notice.AppendLine(Regex.Replace(line, "^【.*】", ""));
            }
            return (notice.ToString(), servers, league.ToString(), nowLeague, false);
        }

        public string[] FollowData(string text, string[] before) {
            string[] brArray = text.Split("|");
            string[] servers = before;
            foreach (string server in brArray) {
                if (Regex.IsMatch(server, @"^\d{2}")) {
                    int num = Int32.Parse(Regex.Matches(server, @"^\d{2}")[0].Value) - 1;
                    string emg = server.Substring(3);
                    if (Regex.IsMatch(emg, @"^\[発生中.*\]")
                        || Regex.IsMatch(emg, @"^\(\d{2}時.*\)")) continue;
                    servers[num] = emg;
                }
            }
            return servers;
        }
    }
}
