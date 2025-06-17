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
	
	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
	{
		ChatManager.DeleteAllGroupChats();
		ChatManager.DeleteAllChatMembers();
		response = "Done! Data cleared.";
		return true;
	}
}