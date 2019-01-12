using System;
using System.Diagnostics;
using TeamSpeak3ModularBotPlugin;
using TeamSpeak3ModularBotPlugin.AttributeClasses;
using TS3QueryLib.Core.Server;
using TS3QueryLib.Core.CommandHandling;

namespace TS3ModularBotPluginTest
{
    public class MyPlugin : Plugin
    {
        public MyPlugin(QueryRunner queryRunner) : base(queryRunner)
        {

        }

        private int _helloCount;

        public override string Author => "Nicer";

        [ClientCommand("hello", MessageTarget.Client | MessageTarget.Channel)]
        public void SendMessage(string clientNickname, uint clId, string input = null)
        {
            _helloCount++;
            Ts3Instance.SendTextMessage(MessageTarget.Client, clId,
                $"Hello {clientNickname}. You greeted me for the {_helloCount}. time. {(input != null ? "You just said: " + input : "")}");
        }

        [ClientCommand("config get", MessageTarget.Client | MessageTarget.Channel)]
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

        [ClientCommand("config set", MessageTarget.Client | MessageTarget.Channel)]
        public void SetConfig(uint clId, string key, string value)
        {
            SetConfigValue(key, value);
            Ts3Instance.SendTextMessage(MessageTarget.Client, clId,
                $"{key}'s value is {(string)GetConfigValue(key)}");
        }

        [ClientCommand("uptime", MessageTarget.Client | MessageTarget.Channel)]
        public void Uptime(uint clId)
        {
            Ts3Instance.SendTextMessage(MessageTarget.Client, clId, $"Been running for {DateTime.Now - Process.GetCurrentProcess().StartTime}");
        }
    }
}
