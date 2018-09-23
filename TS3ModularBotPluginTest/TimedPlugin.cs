using System;
using TeamSpeak3ModularBotPlugin;
using TS3QueryLib.Core.Server;
using System.Timers;
using TeamSpeak3ModularBotPlugin.Helper;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TS3ModularBotPluginTest
{
    internal class TimedPlugin : IPlugin
    {
        private readonly Timer _timer = new Timer(1000);
        private int _i;
        public void OnLoad()
        {
            _timer.Elapsed += TimerOnElapsed;
            _timer.Enabled = true;
        }

        public void Dispose()
        {
            _timer.Elapsed -= TimerOnElapsed;
            _timer.Enabled = false;
            _timer.Dispose();
        }

        [ClientCommand("hi", ClientCommand.MessageMode.Private | ClientCommand.MessageMode.Channel)]
        public void SendMessage(MessageReceivedEventArgs eventArgs, Message e)
        {
            Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId,
                $"hello there {eventArgs.InvokerNickname}");
        }


        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Console.WriteLine($"Test {_i}");
            _i++;
            //Ts3Instance.SendTextMessage(MessageTarget.Client, 12, DateTime.Now.ToLongTimeString());
        }

        public string Author => "Nicer";

        public Version Version => new Version(1, 0, 0, 0);

        public QueryRunner Ts3Instance { get; set; }
    }
}
