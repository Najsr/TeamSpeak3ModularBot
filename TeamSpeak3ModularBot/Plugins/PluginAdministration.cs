using System;
using System.Linq;
using TeamSpeak3ModularBot.PluginCore;
using TeamSpeak3ModularBotPlugin;
using TeamSpeak3ModularBotPlugin.Helper;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TeamSpeak3ModularBot.Plugins
{
    internal class PluginAdministration : IAdminPlugin
    {
        private PluginManager _pluginManager;

        public void Dispose()
        {

        }

        public string Author => "Nicer";

        public Version Version => new Version(1, 0, 0, 0);

        public QueryRunner Ts3Instance { get; set; }
        public void OnLoad(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        [ClientCommand("plugin list", ClientCommand.MessageMode.Private)]
        public void ListPlugins(MessageReceivedEventArgs eventArgs, string[] e)
        {
            Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, $"I have currently loaded {_pluginManager.PluginsCount()} plugin(s). {_pluginManager.GetPluginList()}");
        }

        [ClientCommand("plugin load", ClientCommand.MessageMode.Private)]
        public void LoadPlugin(MessageReceivedEventArgs eventArgs, string[] e)
        {
            if (e.Length == 0)
            {
                Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId,
                    "You must specify which dll file to load!");
                return;
            }
            _pluginManager.LoadDll(e[0]);
            Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, $"I have loaded {e[0]}, please check console");
        }

        [ClientCommand("plugin reload", ClientCommand.MessageMode.Private)]
        public void ReloadPlugin(MessageReceivedEventArgs eventArgs, string[] e)
        {
            if (e.Length == 0)
            {
                Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId,
                    "You must specify which plugin to reload!");
                return;
            }
            _pluginManager.ReloadPlugin(e[0]);
            Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, $"I have reloaded plugin {e[0]}");
        }

        [ClientCommand("plugin unload", ClientCommand.MessageMode.Private)]
        public void UnloadPlugin(MessageReceivedEventArgs eventArgs, string[] e)
        {
            if (e.Length == 0)
            {
                Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId,
                    "You must specify which plugin to unload!");
                return;
            }

            var successfulRemoval = _pluginManager.UnloadPlugin(e[0]);
            Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, $"{(successfulRemoval ? "Successfully" : "Unsuccessfully")} removed plugin {e[0]}.");
        }



        public void OnLoad() { }
    }
}
