using System;
using System.Linq;
using TeamSpeak3ModularBot.PluginCore;
using TeamSpeak3ModularBotPlugin.Helper;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TeamSpeak3ModularBot.Plugins
{
    internal class CommonStuff : IAdminPlugin
    {
        private PluginManager _pluginManager;

        public void Dispose()
        {

        }

        public string Author => "Nicer";

        public Version Version => new Version(1, 0, 0, 0);

        public QueryRunner Ts3Instance { get; set; }
        public void OnLoad(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        [ClientCommand("commands", ClientCommand.MessageMode.Private)]
        public void ListPlugins(MessageReceivedEventArgs eventArgs, string[] e)
        {
            var clientDatabaseId = Ts3Instance
                .GetClientNameAndDatabaseIdByUniqueId(eventArgs.InvokerUniqueId).ClientDatabaseId;
            if (clientDatabaseId != null)
            {
                var groups = Ts3Instance.GetServerGroupsByClientId((uint)clientDatabaseId).Select(x => (int)x.Id).ToArray();
                var commands = _pluginManager.CommandList.Where(x => x.Command.Groups.Intersect(groups).Any() || !x.Command.Groups.Any()).ToArray();
                if (commands.Any())
                    Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, "Available commands: " + string.Join(", ", commands.Select(x => "!" + x.Command.Message)));
                else
                    Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, "You have no commands available!");
            }
        }

        public void OnLoad() { }
    }
}
