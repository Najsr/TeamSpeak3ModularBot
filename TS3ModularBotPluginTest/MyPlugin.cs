using System;
using System.Diagnostics;
using TeamSpeak3ModularBotPlugin;
using TS3QueryLib.Core.Server;
using TeamSpeak3ModularBotPlugin.Helper;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TS3ModularBotPluginTest
{
    public class MyPlugin : Plugin
    {
        public MyPlugin(QueryRunner queryRunner) : base(queryRunner)
        {

        }

        public override string Author => "Nicer";

        [ClientCommand("hello", MessageMode.Private | MessageMode.Channel)]
        public void SendMessage(MessageReceivedEventArgs eventArgs, string input = null)
        {
            Ts3Instance.SendTextMessage(MessageTarget.Client, eventArgs.InvokerClientId,
                $"Hello {eventArgs.InvokerNickname}. {(input != null ? "You just said: " + input : "")}");
        }

        [ClientCommand("config get", MessageMode.Private | MessageMode.Channel)]
        public void GetConfig(uint clId, string key)
        {
            var value = GetConfigValue(key);
            if (value == null)
            {
                Ts3Instance.SendTextMessage(MessageTarget.Client, clId,
                    $"{key} is not in the config!");
                return;
            }
            Ts3Instance.SendTextMessage(MessageTarget.Client, clId,
                $"{key}'s value is {value}");
        }

        [ClientCommand("config set", MessageMode.Private | MessageMode.Channel)]
        public void SetConfig(uint clId, string key, string value)
        {
            SetConfigValue(key, value);
            Ts3Instance.SendTextMessage(MessageTarget.Client, clId,
                $"{key}'s value is {(string)GetConfigValue(key)}");
        }

        [ClientCommand("uptime", MessageMode.Private | MessageMode.Channel)]
        public void Uptime(uint clId)
        {
            Ts3Instance.SendTextMessage(MessageTarget.Client, clId, $"Been running for {(DateTime.Now - Process.GetCurrentProcess().StartTime)}");
        }
    }
}
