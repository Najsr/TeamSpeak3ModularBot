using System;
using System.Collections.Generic;
using System.Timers;
using TeamSpeak3ModularBotPlugin;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server;
using TS3QueryLib.Core.Server.Entities;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TS3ModularBotPluginTest
{
    class ClientReservation : IPlugin
    {
        private readonly List<uint> _channelGroups = new List<uint> { 9, 138, 12 };
        private Timer _timer;
        private uint _clients;
        public void Dispose()
        {
            _timer.Dispose();
            Ts3Instance.Notifications.ClientJoined -= ClientJoined;
        }

        public void OnLoad()
        {
            _timer = new Timer(1000);
            _timer.Elapsed += TimerElapsed;
            _timer.Enabled = true;
            _timer.AutoReset = true;
            Ts3Instance.Notifications.ClientJoined += ClientJoined;
        }

        private void ClientJoined(object sender, ClientJoinedEventArgs args)
        {
            if (args.Nickname.ToLower().Contains("mertym"))
            {
                var nickname = Ts3Instance.SendWhoAmI().ClientNickName;
                Ts3Instance.UpdateCurrentQueryClient(new ClientModification { Nickname = "JanyIheGnger" });
                Ts3Instance.SendTextMessage(MessageTarget.Client, args.ClientId, "Ahoj ty zmrde jeden!");
                Ts3Instance.UpdateCurrentQueryClient(new ClientModification
                {
                    Nickname = nickname
                });
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            var dd = Ts3Instance.GetServerInfo();
            var clients = dd.NumberOfClientsOnline - dd.NumberOfQueryClientsOnline;
            if (clients == _clients)
                return;
            _clients = clients;
            if (clients > 29)
            {
                _channelGroups.ForEach(x => Ts3Instance.AddChannelGroupPermission(x, new NamedPermission() { Name = "i_client_max_idletime", Value = 1800 }));
            }
            else if (clients > 25)
            {
                _channelGroups.ForEach(x => Ts3Instance.AddChannelGroupPermission(x, new NamedPermission() { Name = "i_client_max_idletime", Value = 7200 }));
            }
            else
            {
                _channelGroups.ForEach(x => Ts3Instance.DeleteChannelGroupPermission(x, "i_client_max_idletime"));
            }
        }

        public string Author => "Nicer";
        public Version Version => new Version(1, 0, 0, 0);
        public QueryRunner Ts3Instance { get; set; }
    }
}
