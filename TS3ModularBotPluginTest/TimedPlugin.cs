using System;
using TeamSpeak3ModularBotPlugin;
using TS3QueryLib.Core.Server;
using System.Timers;
using TS3QueryLib.Core.CommandHandling;

namespace TS3ModularBotPluginTest
{
    internal class TimedPlugin : IPlugin
    {
        private readonly Timer _timer = new Timer(1000);

        public void OnLoad()
        {
            _timer.Elapsed += TimerOnElapsed;
            _timer.Enabled = true;
        }

        public void Dispose()
        {
            _timer.Elapsed -= TimerOnElapsed;
            _timer.Enabled = false;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Ts3Instance.SendTextMessage(MessageTarget.Server, 0, DateTime.Now.ToLongTimeString());
        }

        public string Author => "Nicer";

        public Version Version => new Version(1, 0, 0, 0);

        public QueryRunner Ts3Instance { get; set; }
    }
}
