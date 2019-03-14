using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmgNotifyDiscordBot {
	internal class DiscordManager {
		public DiscordSocketClient Client { get; private set; }
		private CommandService commands;
		private IServiceProvider services;

		private List<RestUserMessage> restMessages;

		public async Task Enable() {
			Client = new DiscordSocketClient();
			commands = new CommandService();
			Client.Log += Log;

			services = new ServiceCollection().BuildServiceProvider();
			Client.MessageReceived += MessageReceived;
			//await commands.AddModulesAsync(Assembly.GetEntryAssembly());

			await Client.LoginAsync(TokenType.Bot, Configration.Instance.Datas.DiscordToken);
			await Client.StartAsync();
		}

		private async Task MessageReceived(SocketMessage socketMessage) {
			if (!(socketMessage is SocketUserMessage message)) return;

			int argPos = 0;

			if (!(message.HasCharPrefix('!', ref argPos)
				|| message.HasMentionPrefix(Client.CurrentUser, ref argPos))) return;
			var context = new CommandContext(Client, message);

			var result = await commands.ExecuteAsync(context, argPos, services);
			if (!result.IsSuccess)
				await context.Channel.SendMessageAsync(result.ErrorReason);
		}

		private Task Log(LogMessage msg) {
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		public List<SocketTextChannel> GetChannels() {
			var channels = new List<SocketTextChannel>();
			foreach (var guild in Client.Guilds) {
				foreach (var channel in guild.TextChannels) {
					if (!string.IsNullOrWhiteSpace(channel.Topic)
							&& channel.Topic.StartsWith("%notice"))
						channels.Add(channel);
				}
			}
			return channels;
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
				Color = Color.Gold,
				Author = new EmbedAuthorBuilder().WithName("エレボット1号")
						.WithUrl(@"https://eletusk.club/@elebot1st")
						.WithIconUrl(@"https://eletusk.club/system/accounts/avatars/000/001/642/original/ec186395ae173203.png")
			};
			if (!string.IsNullOrEmpty(notice)) builder.AddField("予告緊急", notice);
			List<string> checker = new List<string>(servers);
			checker.RemoveAll(check => string.IsNullOrEmpty(check));
			if (checker.Count > 0) for (int i = 0; i < 10; i++) builder.AddField($"{i + 1}鯖", string.IsNullOrEmpty(servers[i]) ? "―" : servers[i], true);
			if (!string.IsNullOrEmpty(league)) builder.AddField(nowLeague ? "⚠アークスリーグ開催中⚠" : "アークスリーグ予定", league);
			return builder.Build();
		}

		public async Task SendMessageAsync((string notice, string[] servers, string league, bool nowLeague, bool isFollow) embedData, bool isFollow = false) {
			if (isFollow)
				foreach (var restMessage in restMessages) await restMessage.DeleteAsync();
			var newRestMessages = (await Task.WhenAll(GetChannels().Select(channel => channel.SendMessageAsync("", false, CreateEmbed(embedData.notice, embedData.servers, embedData.league, embedData.nowLeague, embedData.isFollow))))).ToList();
			restMessages = newRestMessages;
		}

		public async void SendAnnounceAsync(string message) {
			var builder = new EmbedBuilder() {
				Title = "テスト",
				Description = "テストembed"
			};
			foreach (var channel in GetChannels()) await channel.SendMessageAsync(message, false, builder.Build());
		}
	}
}
