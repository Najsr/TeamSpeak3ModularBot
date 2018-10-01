using System;
using TeamSpeak3ModularBot.PluginCore;
using TeamSpeak3ModularBotPlugin;
using TS3QueryLib.Core.Server;

namespace TeamSpeak3ModularBot.Plugins
{
    public abstract class AdminPlugin : Plugin
    {
        protected PluginManager PluginManager { get; }

        protected AdminPlugin(QueryRunner queryRunner, PluginManager manager) : base(queryRunner)
        {
            PluginManager = manager;
        }
    }
}
