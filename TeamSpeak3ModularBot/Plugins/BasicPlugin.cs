using System;
using System.Linq;
using TeamSpeak3ModularBotPlugin.Helper;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server;
using TS3QueryLib.Core.Server.Notification.EventArgs;
using TeamSpeak3ModularBot.PluginCore;

namespace TeamSpeak3ModularBot.Plugins
{
    public class BasicPlugin : IPrivatePlugin
    {
	    private PluginManager _pluginManager;

        public void OnLoad()
        {
        }

	    public void OnPluginManager(PluginManager pluginManager)
	    {
		    _pluginManager = pluginManager;
	    }

        public void Dispose()
        {
            
        }

        public string Author => "Nicer";

        public Version Version => new Version(1, 0, 0, 0);
        public QueryRunner Ts3Instance { get; set; }

        [ClientCommand("info", ClientCommand.MessageMode.Private | ClientCommand.MessageMode.Channel)]
        public void Info(MessageReceivedEventArgs eventArgs, Message e)
        {
            var toSend = "I have currently " + _pluginManager.Plugins.Count + " plugins loaded. (" + string.Join(", ", _pluginManager.Plugins.Select(x => x.GetType().Name).ToArray()) + ")";
			Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, toSend);
        }

        [ClientCommand("unload", ClientCommand.MessageMode.Private)]
        public void Unload(MessageReceivedEventArgs eventArgs, Message e)
        {
            if (e.Params == null)
            {
                _pluginManager.UnloadPlugins();
                return;
            }

            if(_pluginManager.UnloadPlugin(e.Params[0]))
                Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, "Plugin " + e.Params[0] + " successfully unloaded");
        }
    }
}
