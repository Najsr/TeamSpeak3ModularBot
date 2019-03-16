using System.Linq;
using TeamSpeak3ModularBot.PluginCore;
using TeamSpeak3ModularBotPlugin.AttributeClasses;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server;

namespace TeamSpeak3ModularBot.Plugins
{
    internal class PluginAdministration : AdminPlugin
    {
        public PluginAdministration(QueryRunner queryRunner, PluginManager manager) : base(queryRunner, manager)
        {

        }

        [ServerGroups("Bot Manager")]
        [ClientCommand("plugin list", "Lists all the loaded plugins", MessageTarget.Client)]
        public void ListPlugins(uint clId)
        {
            Ts3Instance.SendTextMessage(MessageTarget.Client, clId, $"I have currently loaded {PluginManager.PluginsCount()} plugin(s). {PluginManager.GetPluginList()}");
        }

        [ServerGroups("Bot Manager")]
        [ClientCommand("plugin load", "Load plugin from given path", MessageTarget.Client, "You must specify which dll file to load!")]
        public void LoadPlugin(uint clId, string plugin = "{\"Absolute path to plugin\"}")
        {
            var plugins = PluginManager.LoadDll(plugin);
            Ts3Instance.SendTextMessage(MessageTarget.Client, clId, $"I have loaded {(plugins.Count > 0 ? string.Join(", ", plugins.Select(x => x.GetType().Name)) : "no plugins, please check console")}");
        }

        [ServerGroups("Bot Manager")]
        [ClientCommand("plugin reload", "Reloads plugin by name", MessageTarget.Client, "You must specify which plugin to reload!")]
        public void ReloadPlugin(uint clId, string plugin = "{\"Name of plugin\"}")
        {
            var success = PluginManager.ReloadPlugin(plugin);
            Ts3Instance.SendTextMessage(MessageTarget.Client, clId, $"I have {(success ? "successfully" : "failed to ")} reloaded plugin {plugin}");
        }

        [ServerGroups("Bot Manager")]
        [ClientCommand("plugin unload", "Unloads plugin by name", MessageTarget.Client, "You must specify which plugin to unload!")]
        public void UnloadPlugin(uint clId, string plugin = "{\"Name of plugin\"}")
        {

            var successfulRemoval = PluginManager.UnloadPlugin(plugin, out var unloadedPlugins);
            Ts3Instance.SendTextMessage(MessageTarget.Client, clId, $"{(successfulRemoval ? "Successfully" : "Unsuccessfully")} removed plugin(s) {unloadedPlugins}.");
        }
    }
}
