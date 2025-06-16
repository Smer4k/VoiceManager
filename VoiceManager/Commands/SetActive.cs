using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using VoiceManager.Features;

namespace VoiceManager.Commands;

[CommandHandler(typeof(GroupChatParent))]
public class SetActive : ICommand
{
	public string Command { get; } = "setactive";
	public string[] Aliases { get; } = ["sa"];
	public string Description { get; } = "Enable/Disable plugin";
	private readonly string[] _enable = ["y", "enable", "e", "1"];
	private readonly string[] _disable = ["n", "disable", "d", "0"];
	
	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
	{
		if (VoiceManager.Singleton.Config.AutoInitChatMembers)
		{
			response = "AutoInitChatMembers in config = true";
			return false;
		}

		if (arguments.Count < 1)
		{
			response = "Usage: groupchat setactive [<enable|disable>]";
			return false;
		}

		var state = arguments.At(0).ToLower();
		if (_enable.Contains(state))
		{
			if (VoiceManager.AutoInitChatMembers)
			{
				response = "Already enabled";
				return false;
			}
			ChatManager.InitChatMembers();
			VoiceManager.AutoInitChatMembers = true;
			response = "Enabled";
			return true;
		}

		if (_disable.Contains(state))
		{
			if (!VoiceManager.AutoInitChatMembers)
			{
				response = "Already disabled";
				return false;
			}
			VoiceManager.AutoInitChatMembers = false;
			ChatManager.DeleteAllGroupChats();
			ChatManager.DestroyChatMembers();
			response = "Disabled";
			return true;
		}
		
		response = "Usage: groupchat setactive [<enable|disable>]";
		return false;
	}
}