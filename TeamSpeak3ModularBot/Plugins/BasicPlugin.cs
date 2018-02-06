using System;
using System.Linq;
using TeamSpeak3ModularBotPlugin.Helper;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server;
using TS3QueryLib.Core.Server.Notification.EventArgs;
using TeamSpeak3ModularBot.PluginCore;

namespace TeamSpeak3ModularBot.Plugins
{
    internal class BasicPlugin : PrivatePlugin
    {
	    private PluginManager _pluginManager;

        public new void OnLoad()
        {
        }

	    public override void OnPluginManager(PluginManager pluginManager)
	    {
		    _pluginManager = pluginManager;
	    }

        public new void Dispose()
        {
            
        }

        public new string Author => "Nicer";

        public new Version Version => new Version(1, 0, 0, 0);

        public new QueryRunner Ts3Instance { get; set; }

        [ClientCommand("info", ClientCommand.MessageMode.Private | ClientCommand.MessageMode.Channel)]
        public void Info(MessageReceivedEventArgs eventArgs, Message e)
        {
	        var toSend = "I have currently " + _pluginManager.Plugins.Count + " plugins loaded. (" + string.Join(", ", _pluginManager.Plugins.Select(x => x.GetType().Name) + ")");
			Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, toSend);
        }
    }
}
