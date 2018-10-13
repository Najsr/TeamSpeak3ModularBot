using System;
using System.Diagnostics;
using TeamSpeak3ModularBot.Config;
using TeamSpeak3ModularBot.PluginCore;
using TS3QueryLib.Core;
using TS3QueryLib.Core.Common;
using TS3QueryLib.Core.Common.Responses;
using TS3QueryLib.Core.Communication;
using TS3QueryLib.Core.Server;
using TS3QueryLib.Core.Server.Entities;

namespace TeamSpeak3ModularBot
{
    internal class TeamSpeak3Bot : IDisposable
    {
        private Ts3Config Ts3Config { get; }

        private readonly Stopwatch _stopwatch = new Stopwatch();

        private AsyncTcpDispatcher QueryDispatcher { get; set; }

        private CommandManager CommandManager { get; set; }

        private PluginManager _pluginManager;

        private QueryRunner QueryRunner { get; set; }

        public bool Connected { get; private set; }

        public TeamSpeak3Bot(Ts3Config config)
        {
            Ts3Config = config;
        }

        ~TeamSpeak3Bot()
        {
            Disconnect();
        }

        public void Run()
        {
            _stopwatch.Start();
            QueryDispatcher = new AsyncTcpDispatcher(Ts3Config.Host, Ts3Config.Port);
            QueryDispatcher.BanDetected += QueryDispatcher_BanDetected;
            QueryDispatcher.ReadyForSendingCommands += QueryDispatcher_ReadyForSendingCommands;
            QueryDispatcher.ServerClosedConnection += QueryDispatcher_ServerClosedConnection;
            QueryDispatcher.SocketError += QueryDispatcher_SocketError;
            QueryDispatcher.Connect();
            Connected = true;
        }

        private void QueryDispatcher_ReadyForSendingCommands(object sender, EventArgs e)
        {
            if (!Connected) return;
            QueryRunner = new QueryRunner(QueryDispatcher);
            _pluginManager = new PluginManager(QueryRunner);

            var responses = new SimpleResponse[3];
            responses[0] = QueryRunner.Login(Ts3Config.Username, Ts3Config.Password);

            responses[1] = QueryRunner.SelectVirtualServerByPort(Ts3Config.ServerPort);
            responses[2] = QueryRunner.UpdateCurrentQueryClient(new ClientModification
            {
                Nickname = Ts3Config.DisplayName
            });

            foreach (var response in responses)
                if (response.ErrorId != 0)
                    Console.WriteLine("Error {0} occurred: {1}", response.ErrorId, response.ErrorMessage);

            QueryRunner.RegisterForNotifications(ServerNotifyRegisterEvent.Channel);
            QueryRunner.RegisterForNotifications(ServerNotifyRegisterEvent.Server);
            QueryRunner.RegisterForNotifications(ServerNotifyRegisterEvent.TextChannel);
            QueryRunner.RegisterForNotifications(ServerNotifyRegisterEvent.TextPrivate);
            QueryRunner.RegisterForNotifications(ServerNotifyRegisterEvent.TextServer);
            QueryRunner.RegisterForNotifications(ServerNotifyRegisterEvent.TokenUsed);

            Console.WriteLine("Successfully connected! It took [{0}:{1}:{2}.{3}]", _stopwatch.Elapsed.Hours, _stopwatch.Elapsed.Minutes, _stopwatch.Elapsed.Seconds, _stopwatch.Elapsed.Milliseconds);
            Console.WriteLine("Loading plugins...");
            _pluginManager.LoadPlugins();
            CommandManager = new CommandManager(QueryRunner, _pluginManager);
            Console.WriteLine("All plugins have been loaded.");
        }

        #region QueryDispatcher_Errors
        private void QueryDispatcher_SocketError(object sender, SocketErrorEventArgs e)
        {
            if (!Connected) return;
            Console.WriteLine("Something went wrong: {0}", e.SocketError);
            Disconnect();
        }

        private void QueryDispatcher_BanDetected(object sender, EventArgs<SimpleResponse> e)
        {
            if (!Connected) return;
            Console.WriteLine("Ban has been detected! {0}", e.Value.BanExtraMessage);
            Disconnect();
        }

        private void QueryDispatcher_ServerClosedConnection(object sender, EventArgs e)
        {
            if (!Connected) return;
            Console.WriteLine("Connection to the server has been lost!");
            Disconnect();
        }
        #endregion

        private void Disconnect()
        {
            lock (this)
            {
                if (Connected)
                {
                    _pluginManager.Dispose();
                    QueryDispatcher.Disconnect();
                    QueryDispatcher?.Dispose();
                    QueryRunner?.Dispose();

                    _stopwatch.Stop();
                    Console.WriteLine("Disconnected! Been connected for {0}:{1}:{2}.{3}", _stopwatch.Elapsed.Hours, _stopwatch.Elapsed.Minutes, _stopwatch.Elapsed.Seconds, _stopwatch.Elapsed.Milliseconds);
                }
                Connected = false;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
