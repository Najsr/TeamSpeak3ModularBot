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
        public void ListPlugins(MessageReceivedEventArgs eventArgs)
        {
            var clientDatabaseId = Ts3Instance
                .GetClientNameAndDatabaseIdByUniqueId(eventArgs.InvokerUniqueId).ClientDatabaseId;
            if (clientDatabaseId != null)
            {
                var groups = Ts3Instance.GetServerGroupsByClientId((uint)clientDatabaseId).Select(x => x.Name);
                var commands = PluginManager.CommandList.Where(x =>
                {
                    if (x.ServerGroups == null)
                        return x.ServerGroups == null;
                    return (x.ServerGroups?.Groups.Intersect(groups).Any()).Value;
                }).ToArray();
                if (commands.Any())
                    Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, "Available commands: " + string.Join(", ", commands.OrderBy(x => x.Command.Message).Select(x => "!" + x.Command.Message)));
                else
                    Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, "You have no commands available!");
            }
        }

        [ServerGroups("Bot Manager")]
        [ClientCommand("rename")]
        public void Rename(MessageReceivedEventArgs eventArgs, string name)
        {
            var response = Ts3Instance.UpdateCurrentQueryClient(new ClientModification { Nickname = name });
            if (!response.IsErroneous)
                Console.WriteLine("Changed name to {0}", name);
        }
    }
}
