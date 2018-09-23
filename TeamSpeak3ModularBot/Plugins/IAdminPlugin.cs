using System;
using TeamSpeak3ModularBot.PluginCore;
using TeamSpeak3ModularBotPlugin;
using TS3QueryLib.Core.Server;

namespace TeamSpeak3ModularBot.Plugins
{
    public interface IAdminPlugin : IPlugin
    {
        void OnLoad(PluginManager pluginManager);
    }
}
