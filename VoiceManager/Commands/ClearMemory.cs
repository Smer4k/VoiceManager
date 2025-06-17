using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using VoiceManager.Features;

namespace VoiceManager.Commands;

[CommandHandler(typeof(GroupChatParent))]
public class ClearMemory : ICommand
{
	public string Command { get; } = "ClearMemory";
	public string[] Aliases { get; } = ["cm"];
	public string Description { get; } = "Clears plugin data. (Groups, players, etc.)";
	private readonly string[] _enable = ["y", "enable", "e", "1"];
	private readonly string[] _disable = ["n", "disable", "d", "0"];
	
	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
	{
		ChatManager.DeleteAllGroupChats();
		ChatManager.DeleteAllChatMembers();
		response = "Done! Data cleared.";
		return true;
	}
}