using System;
using System.Collections.Generic;
using System.Linq;
using TeamSpeak3ModularBotPlugin;
using TS3QueryLib.Core.Server;
using TeamSpeak3ModularBotPlugin.Helper;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server.Entities;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TS3ModularBotPluginTest
{
    public class MyPlugin : IPlugin
    {
        public string Author => "Nicer";

        public Version Version => new Version(1, 0, 0, 0);

        public QueryRunner Ts3Instance { get; set; }

        private readonly List<Client> _clids = new List<Client>();

        public void OnLoad()
        {
            Ts3Instance.Notifications.ClientJoined += NotificationsOnClientJoined;
            Ts3Instance.Notifications.ClientDisconnect += NotificationsOnClientDisconnect;
            Ts3Instance.Notifications.ClientConnectionLost += NotificationsOnClientConnectionLost;
        }

        private void HandleDisconnect(uint clid)
        {
            Client item;
            try
            {
                item = _clids.First(x => x.Clid == clid);
            }
            catch
            {
                return;
            }
            var connectionTime = GetTimestamp() - item.JoinTimeStamp;
            Console.WriteLine("{0} dced; was connected {1}s, clid: {2}", item.Nickname, connectionTime, item.Clid);
            _clids.Remove(item);
        }

        private void NotificationsOnClientConnectionLost(object sender, ClientConnectionLostEventArgs clientConnectionLostEventArgs)
        {
            HandleDisconnect(clientConnectionLostEventArgs.ClientId);
        }

        private void NotificationsOnClientDisconnect(object sender, ClientDisconnectEventArgs clientDisconnectEventArgs)
        {
            HandleDisconnect(clientDisconnectEventArgs.ClientId);
        }

        private void NotificationsOnClientJoined(object sender, ClientJoinedEventArgs clientJoinedEventArgs)
        {
            if (clientJoinedEventArgs.ClientType != 0) return;
            Console.Write("{0} joined; ", Ts3Instance.GetClientInfo(clientJoinedEventArgs.ClientId).Nickname);
            if (_clids.Count(x => x.Clid == clientJoinedEventArgs.ClientId) != 0)
            {
                Console.Write("client already in list" + Environment.NewLine);
                _clids.RemoveAll(x => x.Clid == clientJoinedEventArgs.ClientId);
            }
            Console.Write("client added to list" + Environment.NewLine);
            _clids.Add(new Client(Ts3Instance.GetClientInfo(clientJoinedEventArgs.ClientId).Nickname, clientJoinedEventArgs.ClientId, GetTimestamp()));
        }

        public void Dispose()
        {
            Ts3Instance.Notifications.ClientJoined -= NotificationsOnClientJoined;
            Ts3Instance.Notifications.ClientDisconnect -= NotificationsOnClientDisconnect;
            Ts3Instance.Notifications.ClientConnectionLost -= NotificationsOnClientConnectionLost;
        }

        [ClientCommand("count", ClientCommand.MessageMode.Private | ClientCommand.MessageMode.Channel)]
        public void Count(MessageReceivedEventArgs eventArgs, Message e)
        {
            Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId, _clids.Count.ToString());
        }

        [ClientCommand("hello", ClientCommand.MessageMode.Private | ClientCommand.MessageMode.Channel)]
        public void SendMessage(MessageReceivedEventArgs eventArgs, Message e)
        {
            Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId,
                $"Hello {eventArgs.InvokerNickname}");
        }

        [ClientCommand("rename", ClientCommand.MessageMode.Private)]
        public void Rename(MessageReceivedEventArgs eventArgs, Message e)
        {
            if (e.Params.Length == 0)
                return;
            var response = Ts3Instance.UpdateCurrentQueryClient(new ClientModification { Nickname = e.Params[0]});
            if(!response.IsErroneous)
                Console.WriteLine("Changed name to {0}", e.Params[0]);
        }

        private static int GetTimestamp()
        {
            return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public struct Client
        {
            public uint Clid;

            public string Nickname;

            public int JoinTimeStamp;

            public Client(string nickname, uint clid, int joinTimeStamp)
            {
                JoinTimeStamp = joinTimeStamp;
                Nickname = nickname;
                Clid = clid;
            }
        }
    }
}
