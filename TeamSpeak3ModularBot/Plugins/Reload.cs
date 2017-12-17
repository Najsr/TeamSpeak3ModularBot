using System;
using TeamSpeak3ModularBotPlugin;
using TeamSpeak3ModularBotPlugin.Helper;
using TS3QueryLib.Core.Server;

namespace TeamSpeak3ModularBot.Plugins
{
    internal class Reload : IPlugin
    {
        public void OnLoad()
        {

        }

        public void Dispose()
        {
            
        }

        public string Author => "Nicer";

        public Version Version => new Version(1, 0, 0, 0);

        public QueryRunner Ts3Instance { get; set; }

        [ClientCommand("reload", ClientCommand.MessageMode.Private)]
        public void ReloadPlugins()
        {
            
        }
    }
}
