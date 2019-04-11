using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace EmgNotifyDiscordBot {
	class StreamingScheduler {
		public delegate void StreamStart();
		public delegate void StreamStop();
		public delegate void CheckPeriod();

		public event StreamStart OnEvent;
		public event StreamStop OffEvent;
		public event CheckPeriod CheckEvent;

		public void Start() {
			Timer timer = new Timer(60000);

			timer.Elapsed += (sender, e) => {
				var time = DateTime.Now;
				if (time.Minute >= 0 && time.Minute < 10) {
					OnEvent();
				} else if (time.Minute == 10) {
					OffEvent();
				}

				if ((time.Minute % 10) == 0) {
					CheckEvent();
				}
			};

			timer.Start();
		}
	}
}
