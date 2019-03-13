﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace EmgNotifyDiscordBot {
	class StreamingScheduler {
		public delegate void StreamStart();
		public delegate void StreamStop();

		public event StreamStart OnEvent;
		public event StreamStop OffEvent;

		public void Start() {
			Timer timer = new Timer(1000);

			timer.Elapsed += (sender, e) => {
				var time = DateTime.Now;
				if (time.Minute == 0) {
					OnEvent();
				} else if (time.Minute == 10) {
					OffEvent();
				}
			};

			timer.Start();
		}
	}
}
