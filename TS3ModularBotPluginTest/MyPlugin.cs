using System;
using System.Collections.Generic;
using TeamSpeak3ModularBotPlugin;
using TS3QueryLib.Core.Server;
using TeamSpeak3ModularBotPlugin.Helper;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server.Entities;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TS3ModularBotPluginTest
{
    public class MyPlugin : Plugin
    {
        public MyPlugin(QueryRunner queryRunner) : base(queryRunner)
        {

        }

        public override string Author => "Nicer";

        [ClientCommand("hello", ClientCommand.MessageMode.Private | ClientCommand.MessageMode.Channel)]
        public void SendMessage(MessageReceivedEventArgs eventArgs, string[] e)
        {
            Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId,
                $"Hello {eventArgs.InvokerNickname}.");
        }

        [ClientCommand("config get", ClientCommand.MessageMode.Private | ClientCommand.MessageMode.Channel)]
        public void GetConfig(MessageReceivedEventArgs eventArgs, string[] e)
        {
            if (e.Length == 0)
                return;
            var value = GetConfigValue(e[0]);
            if (value == null)
            {
                Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId,
                    $"{e[0]} is not in the config!");
                return;
            }
            Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId,
                $"{e[0]}'s value is {value}");
        }

        [ClientCommand("config set", ClientCommand.MessageMode.Private | ClientCommand.MessageMode.Channel)]
        public void SetConfig(MessageReceivedEventArgs eventArgs, string[] e)
        {
            if (e.Length < 2)
                return;
            SetConfigValue(e[0], e[1]);
            Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId,
                $"{e[0]}'s value is {GetConfigValue(e[0])}");
        }
    }
}
