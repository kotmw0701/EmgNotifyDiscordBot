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

        private CommandService commands;
        private DiscordSocketClient client;
        private IServiceProvider services;

        static void Main(string[] args) {
            
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync() {
            client = new DiscordSocketClient();
            commands = new CommandService();
            client.Log += Log;

            services = new ServiceCollection().BuildServiceProvider();
            client.MessageReceived += MessageReceived;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            await client.LoginAsync(TokenType.Bot, Configration.Instance.Datas.DiscordToken);
            await client.StartAsync();

            EletuskDataGetter getter = new EletuskDataGetter(client);
            await getter.Stream();
            
            await Task.Delay(-1);
            

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

        private async Task MessageReceived(SocketMessage socketMessage) {
            var message = socketMessage as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            if (!(message.HasCharPrefix('!', ref argPos)
                || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;

            var context = new CommandContext(client, message);

            var result = await commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }

        private Task Log(LogMessage msg) {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
