namespace TeamSpeak3ModularBot.Config
{
    internal class Ts3Config
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Host { get; set; }

        public ushort Port { get; set; }

        public string DisplayName { get; set; }

        public ushort ServerPort { get; set; }

        public bool IgnoreWarnings { get; set; }
    }
}
