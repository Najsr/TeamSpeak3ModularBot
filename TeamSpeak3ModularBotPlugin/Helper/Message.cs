using System.Linq;
using System.Text.RegularExpressions;

namespace TeamSpeak3ModularBotPlugin.Helper
{
    public class Message
    {
        public string[] Params { get; set; }

        public string ParamString { get; }

        public Message(string message)
        {
            var trimmedMessage = message.Trim().ToLower();
            if (!trimmedMessage.StartsWith("!") || trimmedMessage.Length < 2)
                return;

            GetCommand(trimmedMessage.Substring(1), out var params_);
            Params = params_;
            ParamString = Params != null ? string.Join(" ", Params) : null;
        }

        public static implicit operator Message(string message)
        {
            return message == null ? null : new Message(message);
        }

        private string[] GetSsvParameters(string toSplit)
        {
            if (toSplit == string.Empty)
                return null;

            var myRegex = new Regex(@"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)");

            return myRegex.Split(toSplit).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Replace("\"", "")).ToArray();
        }

        private void GetCommand(string toGet, out string[] params_)
        {
            var myRegex = new Regex(@"!*('.*?'|"".*?""|\S+)");
            var match = myRegex.Match(toGet);
            params_ = match.Success ? GetSsvParameters(toGet) : null;
        }
    }
}
