using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TeamSpeak3ModularBotPlugin;
using TeamSpeak3ModularBotPlugin.AttributeClasses;
using TS3QueryLib.Core.CommandHandling;
using TS3QueryLib.Core.Server;
using TS3QueryLib.Core.Server.Notification.EventArgs;

namespace TeamSpeak3ModularBot.PluginCore
{
    public sealed class CommandStruct : IDisposable
    {
        private QueryRunner Ts3Instance { get; }

        public Plugin Class { get; }

        private MethodInfo Method { get; }

        public ClientCommand Command { get; }

        public ServerGroups ServerGroups { get; }

        public void Dispose()
        {
            Class.Dispose();
        }

        ~CommandStruct()
        {
            Dispose();
        }

        public CommandStruct(QueryRunner instance, Plugin class_, MethodInfo methodName, ClientCommand command, ServerGroups serverGroups = null)
        {
            Ts3Instance = instance;
            Class = class_;
            Method = methodName;
            Command = command;
            ServerGroups = serverGroups;
        }

        internal void Invoke(MessageReceivedEventArgs eArgs, string[] inputStrings, MessageTarget mode)
        {
            var parameters = Method.GetParameters();
            var stringCount = 0;
            var injectedParameters = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var type = parameter.ParameterType;
                if (type == typeof(MessageReceivedEventArgs))
                    injectedParameters[i] = eArgs;
                else if (type == typeof(string[]))
                    injectedParameters[i] = inputStrings;
                else if (type == typeof(List<string>))
                    injectedParameters[i] = inputStrings.ToList();
                else if (type == typeof(uint))
                    injectedParameters[i] = eArgs.InvokerClientId;
                else if (type == typeof(PluginManager))
                    injectedParameters[i] = this;
                else if (type == typeof(MessageTarget))
                    injectedParameters[i] = mode;
                else if (type == typeof(string))
                {
                    switch (parameter.Name.ToLower())
                    {
                        case "uniqueid":
                            injectedParameters[i] = eArgs.InvokerUniqueId;
                            continue;
                        case "clientnickname":
                            injectedParameters[i] = eArgs.InvokerNickname;
                            continue;
                    }
                    var canBeNull = parameter.HasDefaultValue && parameter.DefaultValue == null;
                    if (inputStrings.Length > stringCount)
                    {
                        if (parameter.DefaultValue != null && parameter.HasDefaultValue)
                        {
                            var regex = Regex.Match((string)parameter.DefaultValue, "^(?:{(?=.+})(?<name>.+)})?(?<parameters>.*)?",
                                RegexOptions.IgnoreCase);
                            if (regex.Groups["parameters"].Success)
                            {
                                var allowedValues = new[] { regex.Groups["parameters"].Value };
                                if (allowedValues[0].Contains('|'))
                                {
                                    allowedValues = allowedValues[0].Split('|');
                                }

                                if (!allowedValues.Contains(inputStrings[stringCount], StringComparer.InvariantCultureIgnoreCase))
                                {
                                    var output = $"You can only enter {string.Join(" / ", allowedValues)} in the {stringCount + 1}. parameter.";
                                    SendRensponse(mode, mode == MessageTarget.Client ? eArgs.InvokerClientId : 0, output);
                                    return;
                                }
                            }
                        }
                        injectedParameters[i] = inputStrings[stringCount];
                        stringCount++;
                    }
                    else
                    {
                        if (canBeNull)
                            injectedParameters[i] = null;
                        else
                        {
                            if (string.IsNullOrWhiteSpace(Command.OnFailedMessage))
                                return;

                            var output = Command.OnFailedMessage;
                            FormatString(ref output);
                            SendRensponse(mode, mode == MessageTarget.Client ? eArgs.InvokerClientId : 0, output);
                            return;
                        }
                    }
                }
                else
                    throw new Exception("Unknown parameter type");
            }
            Method.Invoke(Class, injectedParameters);
        }

        private void FormatString(ref string formattedString)
        {
            formattedString = formattedString.Replace("{command}", "!" + Command.Message);
        }

        private void SendRensponse(MessageTarget target, uint clid, string message)
        {
            Ts3Instance.SendTextMessage(target, clid, message);
        }
    }
}
