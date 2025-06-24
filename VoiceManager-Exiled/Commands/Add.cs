using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using Exiled.API.Features;
using PlayerRoles;
using VoiceManager.Features;

namespace VoiceManager.Commands;

[CommandHandler(typeof(GroupChatParent))]
public class Add : ICommand, IUsageProvider
{
	public string Command { get; } = "add";
	public string[] Aliases { get; } = ["a"];
	public string[] Usage { get; } = ["player id", "group id/PROX", "(optional) temp? <y/n>"];
	public string Description { get; } = "Adds a player to the specified group or permission for proximity chat";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
	{
		if (arguments.Count < 2)
		{
			var usage = string.Join(" ", Array.ConvertAll(Usage, u => $"[{u}]"));
			response = $"Usage: {GroupChatParent.CommandName} {Command} {usage}\n" +
			           "(use PROX if you need to give permission to use Proximity chat)";
			return false;
		}

		var members = new List<ChatMember>();
		var strIds = arguments.At(0).Split('.');
		foreach (var id in strIds)
		{
			if (!int.TryParse(id, out int playerId)) continue;
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

		var isTemp = false;
		if (arguments.Count > 2)
		{
			var strTemp = arguments.At(2);
			if (strTemp.ToLower() == "y")
				isTemp = true;
		}

		var countAdded = 0;
		foreach (var member in members)
		{
			if (!isProx)
			{
				group.TryAddMember(member, isTemp);
				countAdded++;
				continue;
			}

			if (member.HasProximityChat || !member.Hub.IsSCP()) continue;
			member.SetHasProximityChat(true);
			countAdded++;
		}

		response = isProx
			? $"Done! Enabled proximity chat for {countAdded} players."
			: $"Done! Added {countAdded} players to {group.Name}";
		return true;
	}
}