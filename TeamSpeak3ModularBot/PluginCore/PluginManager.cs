using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TeamSpeak3ModularBot.Plugins;
using TeamSpeak3ModularBotPlugin;
using TeamSpeak3ModularBotPlugin.AttributeClasses;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server;

namespace TeamSpeak3ModularBot.PluginCore
{
    public class PluginManager : IDisposable
    {
        private readonly List<PluginDomain> _pluginDomains = new List<PluginDomain>();

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
            return _pluginDomains.SelectMany(x => x.Plugins).Count();
        }

        public string GetPluginList()
        {
            var plugins = new List<Plugin>();
            _pluginDomains.ForEach(x => plugins.AddRange(x.Plugins));
            return string.Join(", ", plugins.OrderBy(x => x.GetType().Name).Select(x => x.GetType().Name));
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
            var domainInfo = new AppDomainSetup
            {
                ApplicationBase = Environment.CurrentDirectory + "/plugins",
            };
            var evidence = AppDomain.CurrentDomain.Evidence;
            var domain = AppDomain.CreateDomain(new FileInfo(file).Name, evidence, domainInfo);
            var asm = domain.Load(AssemblyName.GetAssemblyName(file));
            var plugins = new List<Plugin>();
            foreach (var type in asm.GetTypes().Where(x => !x.IsAbstract || typeof(Plugin).IsAssignableFrom(x) || plugins.All(y => y.GetType().Name != x.Name)))
            {
                try
                {
                    var instance = (Plugin)Activator.CreateInstance(type, _queryRunner);

                    AddCustomMethods(type, instance);
                    Console.WriteLine($"Plugin {instance.GetType().Name}{(instance.Author == null ? "" : " by " + instance.Author)} has been successfully loaded!");
                    plugins.Add(instance);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occured during plugin loading! Bad plugin: {type.Name} Error message: {ex.Message}{Environment.NewLine}Stack trace: {ex.StackTrace}");
                }
            }
            _pluginDomains.Add(new PluginDomain(domain, plugins));
        }

        private void AddCustomMethods(Type type, Plugin instance)
        {
            var methods = type.GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(ClientCommand), false).Length > 0 && m.Name != "OnLoad");

            foreach (var method in methods)
            {
                var attributes = (ClientCommand[])method.GetCustomAttributes(typeof(ClientCommand), false);
                var serverGroup = (ServerGroups)method.GetCustomAttributes(typeof(ServerGroups), false).FirstOrDefault();
                foreach (var clientCommand in attributes)
                {
                    if (clientCommand != null)
                        CommandList.Add(new CommandStruct(_queryRunner, instance, method, clientCommand, serverGroup));
                }
            }
        }

        private void UnloadCommandsFromPlugin(Plugin plugin)
        {
            var indexesToRemove = new List<int>();
            for (var i = CommandList.Count - 1; i >= 0; i--)
            {
                var current = CommandList[i];
                if (current.Class == plugin)
                {
                    indexesToRemove.Add(i);
                    current.Dispose();
                }
            }

            foreach (var i in indexesToRemove)
            {
                CommandList.RemoveAt(i);
            }
        }

        public bool UnloadPlugin(string name)
        {
            var pluginDomain = _pluginDomains.FirstOrDefault(x => x.Plugins.Any(y => y.GetType().Name == name));
            if (pluginDomain == null)
                return false;

            foreach (var plugin in pluginDomain.Plugins)
            {
                UnloadCommandsFromPlugin(plugin);
                plugin.Dispose();
            }
            AppDomain.Unload(pluginDomain.AppDomain);
            _pluginDomains.Remove(pluginDomain);
            GC.Collect();
            return true;
        }

        public bool ReloadPlugin(string name)
        {
            var pluginDomain = _pluginDomains.FirstOrDefault(x => x.Plugins.Any(y => y.GetType().Name == name));
            var plugin = pluginDomain?.Plugins.First(x => x.GetType().Name == name);
            if (plugin == null)
                return false;
            UnloadCommandsFromPlugin(plugin);
            var newPlugin = (Plugin)Activator.CreateInstance(plugin.GetType(), _queryRunner);
            plugin.Dispose();
            pluginDomain.Plugins.Remove(plugin);
            AddCustomMethods(newPlugin.GetType(), newPlugin);
            pluginDomain.Plugins.Add(newPlugin);
            return true;
        }

        public void Dispose()
        {
            CommandList.Clear();

            _pluginDomains.ForEach(x => x.Plugins.ForEach(y => y.Dispose()));
            _adminPlugins.ForEach(x => x.Dispose());
        }
    }
}
