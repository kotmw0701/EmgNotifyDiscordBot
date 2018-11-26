using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmgNotifyDiscordBot {
    class DiscordManager {
        public DiscordSocketClient Client { get; private set; }
        private CommandService commands;
        private IServiceProvider services;

        public async Task Enable() {
            Client = new DiscordSocketClient();
            commands = new CommandService();
            Client.Log += Log;

            services = new ServiceCollection().BuildServiceProvider();
            Client.MessageReceived += MessageReceived;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

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
    }
}
