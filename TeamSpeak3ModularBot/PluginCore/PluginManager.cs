using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TeamSpeak3ModularBot.Plugins;
using TeamSpeak3ModularBotPlugin;
using TeamSpeak3ModularBotPlugin.Helper;
using TS3QueryLib.Core.Server;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TeamSpeak3ModularBot.PluginCore
{
    public class PluginManager : IDisposable
    {
        private readonly List<Plugin> _plugins = new List<Plugin>();

        private readonly List<AdminPlugin> _adminPlugins = new List<AdminPlugin>();

        public readonly List<CommandStruct> CommandList = new List<CommandStruct>();

        private readonly QueryRunner _queryRunner;

        private readonly string _pluginDirectory = AppDomain.CurrentDomain.BaseDirectory + "plugins";

        public PluginManager(QueryRunner qr)
        {
            _queryRunner = qr;
        }

        public int PluginsCount()
        {
            return _plugins.Count;
        }

        public string GetPluginList()
        {
            return string.Join(", ", _plugins.OrderBy(x => x.GetType().Name).Select(x => x.GetType().Name).ToArray());
        }

        public void LoadPlugins()
        {
            var adminPlugins = Assembly.GetExecutingAssembly().GetTypes().Where(x => !x.IsAbstract && typeof(AdminPlugin).IsAssignableFrom(x));
            foreach (var adminPlugin in adminPlugins)
            {
                var instance = (AdminPlugin)Activator.CreateInstance(adminPlugin, _queryRunner, this);
                _adminPlugins.Add(instance);
                AddCustomMethods(adminPlugin, instance);
            }
            Console.WriteLine("Admin plugins loaded successfully!");
            var plugins = Directory.GetFiles(_pluginDirectory, "*.dll");
            foreach (var plugin in plugins)
            {
                LoadDll(plugin);
            }
        }

        public void LoadDll(string file)
        {
            var asm = Assembly.LoadFile(file);
            foreach (var type in asm.GetTypes())
            {
                try
                {
                    if (type.IsAbstract || !typeof(Plugin).IsAssignableFrom(type) ||
                        _plugins.Any(x => x.GetType().Name == type.Name))
                        continue;

                    var instance = (Plugin)Activator.CreateInstance(type, _queryRunner);

                    AddCustomMethods(type, instance);
                    Console.WriteLine($"Plugin {instance.GetType().Name}{(instance.Author == null ? "" : " by " + instance.Author)} has been successfully loaded!");
                    _plugins.Add(instance);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occured during plugin loading! Bad plugin: {type.Name} Error message: {ex.Message}{Environment.NewLine}Stack trace: {ex.StackTrace}");
                }
            }
        }

        private void AddCustomMethods(Type type, Plugin instance)
        {
            var methods = type.GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(ClientCommand), false).Length > 0 && m.Name != "OnLoad");

            foreach (var method in methods)
            {
                if(method.Name == "OnLoad")
                    continue;
                var attribute = (ClientCommand)method.GetCustomAttributes(typeof(ClientCommand), false).FirstOrDefault();
                var serverGroup = (ServerGroups)method.GetCustomAttributes(typeof(ServerGroups), false).FirstOrDefault();
                if (attribute != null)
                {
                    CommandList.Add(new CommandStruct(instance, method, attribute, serverGroup));
                }
            }
        }

        public bool UnloadPlugin(string name)
        {
            var pluginIndex = _plugins.FindIndex(x => x.GetType().Name == name);
            if (pluginIndex == -1)
                return false;
            var plugin = _plugins[pluginIndex];

            for (var i = CommandList.Count - 1; i >= 0; i--)
            {
                if (CommandList[i].Class != plugin)
                    continue;
                CommandList[i] = default(CommandStruct);
                CommandList.RemoveAt(i);
            }
            plugin.Dispose();
            _plugins.RemoveAt(pluginIndex);
            GC.Collect();
            return true;
        }

        public void ReloadPlugin(string name)
        {
            var pluginIndex = _plugins.FindIndex(x => x.GetType().Name == name);
            if (pluginIndex == -1)
                return;
            var plugin = _plugins[pluginIndex];
            var newPlugin = (Plugin)Activator.CreateInstance(plugin.GetType(), _queryRunner);
            plugin.Dispose();
            _plugins[pluginIndex] = newPlugin;

        }

        public void Dispose()
        {
            CommandList.Clear();
            _plugins.ForEach(x => x.Dispose());
            _adminPlugins.ForEach(x => x.Dispose());
        }

        public struct CommandStruct
        {
            public Plugin Class { get; }

            public MethodInfo Method { get; }

            public ClientCommand Command { get; }

            public ServerGroups ServerGroups { get; }

            public CommandStruct(Plugin class_, MethodInfo methodName, ClientCommand command, ServerGroups serverGroups = null)
            {
                Class = class_;
                Method = methodName;
                Command = command;
                ServerGroups = serverGroups;
            }

            internal object Invoke(MessageReceivedEventArgs eArgs, string[] v)
            {
                return Method.Invoke(Class, new object[] { eArgs, v });
            }
        }
    }
}
