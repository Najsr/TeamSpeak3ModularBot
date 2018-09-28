using System;
using TeamSpeak3ModularBotPlugin;
using TS3QueryLib.Core.Server;
using TeamSpeak3ModularBotPlugin.Helper;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server.Entities;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TS3ModularBotPluginTest
{
    public class MyPlugin : IPlugin
    {
        public string Author => "Nicer";

        public Version Version => new Version(1, 0, 0, 0);

        public QueryRunner Ts3Instance { get; set; }

        public void OnLoad()
        {

        }

        public void Dispose()
        {

        }

        [ClientCommand("hello", ClientCommand.MessageMode.Private | ClientCommand.MessageMode.Channel)]
        public void SendMessage(MessageReceivedEventArgs eventArgs, string[] e)
        {
            Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId,
                $"Hello {eventArgs.InvokerNickname}");
        }
    }
}
