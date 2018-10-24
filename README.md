# TS3 Modular Bot

## Introduction

A simple to use auto loading bot with many features!

## Requirements

This module requires the following modules/libraries:

* [TS3QueryLib.Net](https://github.com/Scordo/TS3QueryLib.Net) - already included with License

## Installation

Download latest version of Modular Bot and build it. After you are done, you have a working bot with examples

## Configuration

Create a file called __config.json__ and paste in this code and edit it by your login details...:
```
{
    "Username": "serveradmin",
    "Password": "password",
    "Host": "my_server.com",
    "Port": 10011,
    "DisplayName": ".NET modular bot",
    "ServerPort": 9987
}
```
## Injectable data types (in Command's parameter)

* [MessageReceivedEventArgs](https://github.com/Najsr/TeamSpeak3ModularBot/blob/c5bebf790687146bc055ab96b54d887cb3b7db21/TS3QueryLib.Core.Framework/Server/Notification/EventArgs/MessageReceivedEventArgs.cs) - whole received command
* string[] or List\<string\> - All the parameters passed separated by space
* uint - clientID of client who executed the command
* [PluginManager](https://github.com/Najsr/TeamSpeak3ModularBot/blob/master/TeamSpeak3ModularBot/PluginCore/PluginManager.cs) - Class used to manage plugins
* [MessageTarget](https://github.com/Najsr/TeamSpeak3ModularBot/blob/c5bebf790687146bc055ab96b54d887cb3b7db21/TS3QueryLib.Core.Framework/CommandHandling/MessageTarget.cs) - Enum value that contains on which type of chat the command was received
* string
    * if it has no default value set it is used as a required parameter
    * if default value is null, it means that it is optional parameter
    * if name of the string is __uniqueid__ then the bot injects invoker's UID
    * if name of the string is __clientnickname__ then the bot injects invoker's nickname
    * if invoker didn't pass enough parameters it will execute the __onFailedMessage__ string 
    
## Example Plugin
```
 public class MyPlugin : Plugin
 {
    public MyPlugin(QueryRunner queryRunner) : base(queryRunner)
    {
		//Can be used for initializing since it is a constructor
    }

	//Author of plugin; Not required
    public override string Author => "Only ME";

	//Limits execution for specified groups
	[ServerGroups("Server Admin", "Normal")]
    //Implements chat command itself
    //Parameters: 
    //string Command - it is the command itself (if you want more on a single method, use more ClientCommand classes)
    //MessageMode = MessageMode.All - defines on which type of chat it will be triggered (it is bit based enum, so bitwise operators can be used)
    //string onFailedMessage = null - replied to a executing user when he does not input enough parameters (defined by string parameters without default value)
    [ClientCommand("hello", MessageMode.Private | MessageMode.Channel)]
    
    public void SendMessage(string clientNickname, uint clId, string input = null)
    {
        Ts3Instance.SendTextMessage(MessageTarget.Client, clId,
            $"Hello {clientNickname}. {(input != null ? "You just said: " + input : "")}");
    }
}
```