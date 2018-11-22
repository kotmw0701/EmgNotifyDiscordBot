using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EmgNotifyDiscordBot {
    public class Configration {
        public static Configration Instance { get; } = new Configration();
        private Configration() {
            using (var reader = new StreamReader("data.json", Encoding.UTF8))
                Datas = JsonConvert.DeserializeObject<PrivateDatas>(reader.ReadToEnd());
        }

        public PrivateDatas Datas { get; }
    }
}
