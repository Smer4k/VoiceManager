using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using Exiled.API.Features;
using VoiceManager.Features;

namespace VoiceManager.Commands;

[CommandHandler(typeof(GroupChatParent))]
public class Remove : ICommand, IUsageProvider
{
	public string Command { get; } = "remove";
	public string[] Aliases { get; } = ["r"];
	public string[] Usage { get; } = ["player id", "group id/PROX"];
	public string Description { get; } = "Removes a player from a group or permission for proximity chat";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
	{
		if (arguments.Count < 2)
		{
			var usage = string.Join(" ", Array.ConvertAll(Usage, u => $"[{u}]"));
			response = $"Usage: {GroupChatParent.CommandName} {Command} {usage}\n" +
			           "(use PROX if you need to remove permission to use Proximity chat)";
			return false;
		}

		var members = new List<ChatMember>();
		foreach (var id in arguments.At(0).Split('.'))
		{
			if (!int.TryParse(id, out var playerId)) continue;
			var player = Player.Get(playerId);
			if (player == null) continue;
			members.Add(ChatMember.Get(player));
		}

		bool isProx = arguments.At(1).ToLower() == "prox";
		GroupChat group = null;
		if (!isProx)
		{
			if (!int.TryParse(arguments.At(1), out int groupId))
			{
				response = "Group ID must consist of numbers only.";
				return false;
			}

			group = ChatManager.GetGroupChat(groupId);
			if (group == null)
			{
				response = $"Group with ID {arguments.At(1)} was not found.";
				return false;
			}
		}

		var countRemoved = 0;
		foreach (var member in members)
		{
			if (!isProx)
			{
				group.TryRemoveMember(member);
				countRemoved++;
				continue;
			}

			if (!member.HasProximityChat) continue;
			member.SetHasProximityChat(false);
			countRemoved++;
		}

		response = isProx
			? $"Done! Disabled proximity chat for {countRemoved} players."
			: $"Done! Removed {countRemoved} players from {group.Name}";
		return true;
	}
}