using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using VoiceManager.Features;

namespace VoiceManager.Commands;

[CommandHandler(typeof(GroupChatParent))]
public class Delete : ICommand, IUsageProvider
{
	public string Command { get; } = "delete";
	public string[] Aliases { get; } = ["d"];
	public string[] Usage { get; } = ["group id/ALL"];
	public string Description { get; } = "Removes the specified group.";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
	{
		if (arguments.Count < 1)
		{
			var usage = string.Join(" ", Array.ConvertAll(Usage, u => $"[{u}]"));
			response = $"Usage: {GroupChatParent.CommandName} {Command} {usage}\n" +
			           $"(use \"all\" if you want to delete all groups)";
			return false;
		}

		if (arguments.At(0).ToLower() == "all")
		{
			ChatManager.DeleteAllGroupChats();
			response = $"All group chats have been removed.";
			return true;
		}

		if (!int.TryParse(arguments.At(0), out int groupId))
		{
			response = "Player ID must consist of numbers only.";
			return false;
		}

		if (ChatManager.RemoveGroupChat(groupId))
		{
			response = $"Group with ID {groupId} has been removed.";
			return true;
		}

		response = $"Group with ID {groupId} was not found.";
		return false;
	}
}