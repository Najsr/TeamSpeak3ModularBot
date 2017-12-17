﻿using System.Linq;
using System.Text.RegularExpressions;

namespace TeamSpeak3ModularBotPlugin.Helper
{
    public class Message
    {
        public string Command { get; }

        public string[] Params { get; }

        public Message(string message)
        {
            var trimmedMessage = message.Trim();
            if (!trimmedMessage.StartsWith("!") || trimmedMessage.Length < 2)
                return;

            GetCommand(trimmedMessage, out var cmd, out var params_);
            Command = cmd;
            Params = params_;
        }

        public static implicit operator Message(string message)
        {
            return message == null ? null : new Message(message);
        }

        private string[] GetSsvParameters(string toSplit)
        {
            if (toSplit == string.Empty)
            {
                return null;
            }
            var myRegex = new Regex(@"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)");

            return myRegex.Split(toSplit).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Replace("\"", "")).ToArray();
        }

        private void GetCommand(string toGet, out string command, out string[] params_)
        {
            var myRegex = new Regex(@"^!([^\s]*)\s?(.*)");
            var match = myRegex.Match(toGet);
            command =  match.Success ? match.Groups[1].Value : null;
            params_ = match.Success ? GetSsvParameters(match.Groups[2].Value) : null;
        }
    }
}
