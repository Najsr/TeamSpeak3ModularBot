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

        [ClientCommand("rename", ClientCommand.MessageMode.Private)]
        public void Rename(MessageReceivedEventArgs eventArgs, string[] e)
        {
            if (e.Length == 0)
                return;
            var response = Ts3Instance.UpdateCurrentQueryClient(new ClientModification { Nickname = e[0]});
            if(!response.IsErroneous)
                Console.WriteLine("Changed name to {0}", e[0]);
        }
    }
}
