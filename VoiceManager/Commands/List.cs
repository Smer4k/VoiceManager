using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CommandSystem;
using NorthwoodLib.Pools;
using VoiceManager.Features;

namespace VoiceManager.Commands;

[CommandHandler(typeof(GroupChatParent))]
public class List : ICommand
{
	public string Command { get; } = "list";
	public string[] Aliases { get; } = ["l"];
	public string Description { get; } = "Shows a list of groups and their members.";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
	{
		if (!VoiceManager.AutoInitChatMembers)
		{
			response = "Plugin not initialized! Use: groupchat setactive enabled";
			return false;
		}
		
		if (ChatManager.Groups.Count == 0)
		{
			response = "There are no groups";
			return false;
		}

		var sb = StringBuilderPool.Shared.Rent();
		sb.AppendLine($"Count of groups: {ChatManager.Groups.Count}");
		foreach (var group in ChatManager.Groups)
		{
			sb.AppendLine($"Id: {group.Id} Group: {group.Name}");
			foreach (var member in group.Members)
			{
				sb.AppendLine($"- PlayerId: {member.Hub.PlayerId} | {member}");
			}
		}

		response = StringBuilderPool.Shared.ToStringReturn(sb);
		return true;
	}
}