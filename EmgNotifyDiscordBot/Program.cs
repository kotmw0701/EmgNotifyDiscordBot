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

            EletuskDataGetter getter = new EletuskDataGetter(manager.Client);
            await getter.Stream();

			Console.ReadKey();
			await manager.Client.LogoutAsync();
			Environment.Exit(0);


			//while (true) {
			//    string text = Console.ReadLine();
			//    if(text.StartsWith("%stop")) {
			//        await client.LogoutAsync();
			//        Environment.Exit(0);
			//        return;
			//    }
			//    await client.GetGuild(427091125170601985).GetTextChannel(427101602093072384).SendMessageAsync(text);
			//}
		}
	}
}
