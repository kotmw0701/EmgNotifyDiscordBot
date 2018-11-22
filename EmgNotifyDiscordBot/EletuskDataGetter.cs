using Discord.WebSocket;
using Mastonet;
using Mastonet.Entities;
using System;
using System.Collections.Generic;
using System.Text;
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
                dicordClient.GetGuild(427091125170601985).GetTextChannel(427101602093072384).SendMessageAsync(e.Status.Content);
            };

            await streaming.Start();
        }
    }
}
