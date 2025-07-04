using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using VoiceManager.Features;

namespace VoiceManager.Commands;

[CommandHandler(typeof(GroupChatParent))]
public class Create : ICommand, IUsageProvider
{
	public string Command { get; } = "create";
	public string[] Aliases { get; } = ["c"];
	public string[] Usage { get; } = ["group name"];
	public string Description { get; } = "Create a new group chat";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
	{
		if (arguments.Count < 1)
		{
			var usage = string.Join(" ", Array.ConvertAll(Usage, u => $"[{u}]"));
			response = $"Usage: {GroupChatParent.CommandName} {Command} {usage}";
			return false;
		}

		var name = "";
		foreach (var str in arguments)
		{
			name += $"{str} ";
		}

		name = name.Trim();
		var group = ChatManager.CreateGroupChat(name);
		response = $"Successfully created a new group chat '{group.Name}' with ID {group.Id}.";
		return true;
	}
}