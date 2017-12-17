using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TeamSpeak3ModularBotPlugin;
using TeamSpeak3ModularBotPlugin.Helper;
using TS3QueryLib.Core.Server;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TeamSpeak3ModularBot.PluginCore
{
    internal class PluginManager
    {
        public readonly List<IPlugin> Plugins = new List<IPlugin>();

        public readonly List<CommandStruct> CommandList = new List<CommandStruct>();

        private readonly QueryRunner _queryRunner;

        public PluginManager(QueryRunner qr)
        {
            _queryRunner = qr;
        }

        public void LoadPlugins()
        {
            try
            {
                var plugins = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "plugins", "*.dll");
                foreach (var plugin in plugins)
                {
                    LoadDll(plugin);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured during plugin load! Error message: {0}", ex.Message);
            }
        }

        public void LoadDll(string file)
        {
            var asm = Assembly.LoadFile(file);
            foreach (var type in asm.GetTypes())
            {
                if (type.IsAbstract || !typeof(IPlugin).IsAssignableFrom(type) || Plugins.Any(x => x.GetType().Name == type.Name))
                    continue;

                var instance = (IPlugin)Activator.CreateInstance(type);
                instance.Ts3Instance = _queryRunner;
                instance.OnLoad();

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
                Console.WriteLine("Plugin {0} ({1}) by {2} has been successfully loaded!", instance.GetType().Name, instance.Version, instance.Author);
                Plugins.Add(instance);
            }
        }

        public struct CommandStruct
        {
            private IPlugin Class { get; }

            private MethodInfo Method { get; }

            public ClientCommand Command { get; }

            public CommandStruct(IPlugin class_, MethodInfo methodname, ClientCommand command)
            {
                Class = class_;
                Method = methodname;
                Command = command;
            }

            public object Invoke(MessageReceivedEventArgs eventArgs, Message m)
            {
                return Method.Invoke(Class, new object[] { eventArgs, m });
            }
        }
    }
}
