using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Mastonet;
using Mastonet.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmgNotifyDiscordBot {
    internal class EletuskDataGetter {

        private DiscordManager discordManager;

		private MastodonClient client;
		private TimelineStreaming streaming;
		private DateTime latestTime;

        private (string notice, string[] servers, string league, bool nowLeague, bool isFollow) embedData;

		private bool _nowStreaming = false;

        public EletuskDataGetter(DiscordManager manager) {
            this.discordManager = manager;
        }

        /// <summary>
        /// EltuskのStreamingとメッセージの受け取りを開始する
        /// </summary>
        /// <returns></returns>
        public async Task StreamGenerate() {
            var appRegistration = new AppRegistration() {
                Instance = "eletusk.club",
                ClientId = Configration.Instance.Datas.EletuskClientID,
                ClientSecret = Configration.Instance.Datas.EletuskClientSecret
            };

            var authClient = new AuthenticationClient(appRegistration);
            var auth = await authClient.ConnectWithPassword(Configration.Instance.Datas.Email, Configration.Instance.Datas.Pass);

            client = new MastodonClient(appRegistration, auth);

			latestTime = client.GetHomeTimeline().Result.First().CreatedAt;

			streaming = client.GetUserStreaming();

            streaming.OnUpdate += async (sender, e) => await OnMessageRecieveAsync(sender, e);
        }

		public async Task Start() {
			if (_nowStreaming)
				return;
			_nowStreaming = true;
			Console.WriteLine("Start");
			await streaming.Start();
		}

		public void Stop() {
			if (!_nowStreaming)
				return;
			_nowStreaming = false;
			Console.WriteLine("Stop");
			streaming.Stop();
		}

		public async Task GetLatestAsync() {
			Status status = client.GetHomeTimeline().Result.First();
			await SendMessage(status.Content, status.CreatedAt);
		}

        private async Task OnMessageRecieveAsync(object sender, StreamUpdateEventArgs args) {
			Console.WriteLine("Received");
            if (args.Status.Account.AccountName != "elebot1st") return;
			await SendMessage(args.Status.Content, args.Status.CreatedAt);
		}

		public async Task SendMessage(string text, DateTime createdAt) {
			if (latestTime.Equals(createdAt)) return;
			string content = Regex.Replace(text.Replace("</p><p>", "|"), @"<(p|/p)>", "").Replace("<br />", "|");
			if (content.IndexOf("|") < 0) return;
			string head = content.Substring(0, content.IndexOf("|"));
			content = content.Substring(content.IndexOf("|") + 1, content.Length - content.IndexOf("|") - 1);
			bool isFollow = false;
			if (Regex.IsMatch(head, $"{DateTime.Now.ToString("HH")}:\\d{{2}}続報")) {
				embedData.isFollow = true;
				embedData.servers = FollowData(Regex.Replace(content, @"<a.*/a>", ""), embedData.servers);
				isFollow = true;
			} else if (!head.Contains("PSO2緊急クエスト予告")) return;
			else embedData = ParseData(content);
			await discordManager.SendMessageAsync(embedData, isFollow);
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
