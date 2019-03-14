using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace EmgNotifyDiscordBot {
    class Program {

        static void Main(string[] args) {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync() {

            DiscordManager manager = new DiscordManager();
            await manager.Enable();
			
			EletuskDataGetter getter = new EletuskDataGetter(manager);
            await getter.StreamGenerate();

			StreamingScheduler scheduler = new StreamingScheduler();
			scheduler.OnEvent += async () => await getter.Start();
			scheduler.OffEvent += () => getter.Stop();
			scheduler.Start();

			while (true) {
			    string text = Console.ReadLine();
			    if(text.StartsWith("%stop")) {
					await manager.Client.LogoutAsync();
					Environment.Exit(0);
			        return;
			    } else if(text.StartsWith("%guilds")) {
					foreach (var guild in manager.Client.Guilds) {
						Console.WriteLine($"Guilds: {guild.Name}");
						foreach (var channel in guild.TextChannels) {
							if (!string.IsNullOrWhiteSpace(channel.Topic) 
									&& channel.Topic.StartsWith("%notice"))
								Console.WriteLine(channel.Topic);
						}
					}
				} else if (text.StartsWith("%test")) {
					manager.SendAnnounceAsync(text.Split(' ')[1]);
				}
			}
		}

		/*
		 * 必要権限メモ
		 * ・メッセージを送信(※重要)
		 * ・メッセージの管理
		 * ・埋め込みリンク(※重要)
		 * ・メッセージ履歴を見る
		 * ・全員宛
		 * ・外部絵文字の使用
		 * ・リアクションの追加
		 * 
		 */
	}
}
