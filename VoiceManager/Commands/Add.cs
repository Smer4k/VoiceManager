using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using LabApi.Features.Wrappers;
using VoiceManager.Features;
using VoiceManager.Features.MonoBehaviours;

namespace VoiceManager.Commands;

[CommandHandler(typeof(GroupChatParent))]
public class Add : ICommand, IUsageProvider
{
	public string Command { get; } = "add";
	public string[] Aliases { get; } = ["a"];
	public string[] Usage { get; } = ["player id", "group id/PROX"];
	public string Description { get; } = "Adds a player to the specified group";

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
	{
		if (!VoiceManager.AutoInitChatMembers)
		{
			response = "Plugin not initialized! Use: groupchat setactive enabled";
			return false;
		}
		
		if (arguments.Count < 2)
		{
			response = "Usage: groupchat add [<player id>] [<group id>/PROX] [(optional) 1 live? <y/n>]\n" +
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
			members.Add(player.GetChatMember());
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

			if (member.CanUseProximityChat) continue;
			member.SetCanUseProximityChat(true);
			countAdded++;
		}

		response = isProx
			? $"Done! Enabled proximity chat for {countAdded} players."
			: $"Done! Added {countAdded} players to {group.Name}";
		return true;
	}
}