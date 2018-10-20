using System;
using TeamSpeak3ModularBot.PluginCore;
using TeamSpeak3ModularBotPlugin.AttributeClasses;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TeamSpeak3ModularBot.Plugins
{
    internal class PluginAdministration : AdminPlugin
    {
        public PluginAdministration(QueryRunner queryRunner, PluginManager manager) : base(queryRunner, manager)
        {

        }

        public override string Author => "Nicer";

        public Version Version => new Version(1, 0, 0, 0);

        [ServerGroups("Bot Manager")]
        [ClientCommand("plugin list", MessageMode.Private)]
        public void ListPlugins(MessageReceivedEventArgs eventArgs, string[] e)
        {
            Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, $"I have currently loaded {PluginManager.PluginsCount()} plugin(s). {PluginManager.GetPluginList()}");
        }

        [ServerGroups("Bot Manager")]
        [ClientCommand("plugin load", MessageMode.Private)]
        public void LoadPlugin(MessageReceivedEventArgs eventArgs, string[] e)
        {
            if (e.Length == 0)
            {
                Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId,
                    "You must specify which dll file to load!");
                return;
            }
            PluginManager.LoadDll(e[0]);
            Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, $"I have loaded {e[0]}, please check console");
        }

        [ServerGroups("Bot Manager")]
        [ClientCommand("plugin reload", MessageMode.Private)]
        public void ReloadPlugin(MessageReceivedEventArgs eventArgs, string[] e)
        {
            if (e.Length == 0)
            {
                Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId,
                    "You must specify which plugin to reload!");
                return;
            }
            PluginManager.ReloadPlugin(e[0]);
            Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, $"I have reloaded plugin {e[0]}");
        }

        [ServerGroups("Bot Manager")]
        [ClientCommand("plugin unload", MessageMode.Private)]
        public void UnloadPlugin(MessageReceivedEventArgs eventArgs, string[] e)
        {
            if (e.Length == 0)
            {
                Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId,
                    "You must specify which plugin to unload!");
                return;
            }

            var successfulRemoval = PluginManager.UnloadPlugin(e[0]);
            Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, $"{(successfulRemoval ? "Successfully" : "Unsuccessfully")} removed plugin {e[0]}.");
        }
    }
}
