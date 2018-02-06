using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamSpeak3ModularBot.PluginCore;
using TeamSpeak3ModularBotPlugin;
using TS3QueryLib.Core.Server;

namespace TeamSpeak3ModularBot.Plugins
{
	internal class PrivatePlugin : IPlugin
	{
		public virtual void OnPluginManager(PluginManager pluginManager)
		{

		}

		public void Dispose()
		{

		}

		public void OnLoad()
		{

		}

		public string Author { get; }

		public Version Version { get; }

		public QueryRunner Ts3Instance { get; set; }
	}
}
