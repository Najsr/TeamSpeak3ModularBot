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
    public class PluginManager
    {
        private readonly List<IPlugin> _plugins = new List<IPlugin>();

        private readonly List<IAdminPlugin> _adminPlugins = new List<IAdminPlugin>();

        public readonly List<CommandStruct> CommandList = new List<CommandStruct>();

        private readonly QueryRunner _queryRunner;

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

            var adminPlugins = Assembly.GetExecutingAssembly().GetTypes().Where(x => !x.IsAbstract && typeof(IAdminPlugin).IsAssignableFrom(x));
            foreach (var adminPlugin in adminPlugins)
            {
                var instance = (IAdminPlugin)Activator.CreateInstance(adminPlugin);
                instance.Ts3Instance = _queryRunner;
                instance.OnLoad(this);
                _adminPlugins.Add(instance);
                AddCustomMethods(adminPlugin, instance);
            }
            Console.WriteLine("Admin plugins loaded successfully!");
            var plugins = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "plugins", "*.dll");
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
                    if (type.IsAbstract || !typeof(IPlugin).IsAssignableFrom(type) ||
                        _plugins.Any(x => x.GetType().Name == type.Name))
                        continue;

                    var instance = (IPlugin)Activator.CreateInstance(type);
                    instance.Ts3Instance = _queryRunner;
                    instance.OnLoad();

                    AddCustomMethods(type, instance);
                    Console.WriteLine("Plugin {0} ({1}) by {2} has been successfully loaded!", instance.GetType().Name,
                        instance.Version, instance.Author);
                    _plugins.Add(instance);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occured during plugin loading! Bad plugin: {type.Name} Error message: {ex.Message}");
                }
            }
        }

        private void AddCustomMethods(Type type, IPlugin instance)
        {
            var methods = type.GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(ClientCommand), false).Length > 0 && m.Name != "OnLoad").ToArray();
            if (methods.Length > 0)
            {
                foreach (var methodInfo in methods)
                {
                    var attribute = (ClientCommand)methodInfo.GetCustomAttributes(typeof(ClientCommand), false)[0];
                    CommandList.Add(new CommandStruct(instance, methodInfo, attribute));
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
            plugin = null;
            GC.Collect();
            return true;
        }

        public void ReloadPlugin(string name)
        {
            var pluginIndex = _plugins.FindIndex(x => x.GetType().Name == name);
            if (pluginIndex == -1)
                return;
            var plugin = _plugins[pluginIndex];
            var newPlugin = (IPlugin)Activator.CreateInstance(plugin.GetType());
            plugin.Dispose();
            newPlugin.Ts3Instance = _queryRunner;
            newPlugin.OnLoad();
            _plugins[pluginIndex] = newPlugin;

        }

        public struct CommandStruct
        {
            public IPlugin Class { get; }

            private MethodInfo Method { get; }

            public ClientCommand Command { get; }

            public CommandStruct(IPlugin class_, MethodInfo methodName, ClientCommand command)
            {
                Class = class_;
                Method = methodName;
                Command = command;
            }

            internal object Invoke(MessageReceivedEventArgs eArgs, string[] v)
            {
                return Method.Invoke(Class, new object[] { eArgs, v });
            }
        }
    }
}
