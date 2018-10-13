using System;
using System.Linq;
using TeamSpeak3ModularBot.PluginCore;
using TeamSpeak3ModularBotPlugin.Helper;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server;
using TS3QueryLib.Core.Server.Entities;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TeamSpeak3ModularBot.Plugins
{
    internal class CommonStuff : AdminPlugin
    {
        public CommonStuff(QueryRunner queryRunner, PluginManager manager) : base(queryRunner, manager)
        {

        }

        public override string Author => "Nicer";

        [ClientCommand("commands")]
        public void ListPlugins(MessageReceivedEventArgs eventArgs, string[] e)
        {
            var clientDatabaseId = Ts3Instance
                .GetClientNameAndDatabaseIdByUniqueId(eventArgs.InvokerUniqueId).ClientDatabaseId;
            if (clientDatabaseId != null)
            {
                var groups = Ts3Instance.GetServerGroupsByClientId((uint)clientDatabaseId).Select(x => (int)x.Id).ToArray();
                var commands = PluginManager.CommandList.Where(x => x.Command.Groups.Intersect(groups).Any() || !x.Command.Groups.Any()).ToArray();
                if (commands.Any())
                    Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, "Available commands: " + string.Join(", ", commands.OrderBy(x => x.Command.Message).Select(x => "!" + x.Command.Message)));
                else
                    Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, "You have no commands available!");
            }
        }

        [ClientCommand("rename", 145)]
        public void Rename(MessageReceivedEventArgs eventArgs, string[] e)
        {
            if (e.Length == 0)
                return;
            var response = Ts3Instance.UpdateCurrentQueryClient(new ClientModification { Nickname = e[0] });
            if (!response.IsErroneous)
                Console.WriteLine("Changed name to {0}", e[0]);
        }
    }
}
