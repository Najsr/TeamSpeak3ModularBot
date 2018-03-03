using TeamSpeak3ModularBotPlugin;

namespace TeamSpeak3ModularBot.PluginCore
{
    internal interface IPrivatePlugin : IPlugin
	{
	    void OnPluginManager(PluginManager pluginManager);
	}
}
