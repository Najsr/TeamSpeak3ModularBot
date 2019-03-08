using System;
using System.Collections.Generic;
using TeamSpeak3ModularBotPlugin;

namespace TeamSpeak3ModularBot.PluginCore
{
    class PluginDomain
    {
        public PluginDomain(AppDomain domain, List<Plugin> plugins)
        {
            AppDomain = domain;
            Plugins = plugins;
        }

        public AppDomain AppDomain { get; set; }

        public List<Plugin> Plugins { get; set; }
    }
}
