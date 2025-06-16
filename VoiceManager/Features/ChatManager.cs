using System;
using System.Collections.Generic;
using System.Linq;
using PlayerRoles.Spectating;
using UnityEngine;
using VoiceChat;
using VoiceChat.Networking;
using VoiceManager.Features.MonoBehaviours;

namespace VoiceManager.Features;

public static class ChatManager
{
	public static HashSet<GroupChat> Groups { get; } = new();

	/// <summary>
	/// Creates a new group chat with the specified ID and name, 
	/// and adds it to the global group list if it doesn't already exist.
	/// </summary>
	/// <param name="id">The unique identifier for the group chat.</param>
	/// <param name="name">The name of the group chat.</param>
	/// <returns>
	/// The newly created <see cref="GroupChat"/> if added successfully; otherwise, <c>null</c>.
	/// </returns>
	public static GroupChat CreateGroupChat(int id, string name)
	{
		var chatGroup = new GroupChat(id, name);
		return Groups.Add(chatGroup) ? chatGroup : null;
	}

	/// <summary>
	/// Creates a new group chat with a unique automatically assigned ID and the specified name,
	/// and adds it to the global group list.
	/// </summary>
	/// <param name="name">The name of the group chat.</param>
	/// <returns>The newly created <see cref="GroupChat"/>.</returns>
	public static GroupChat CreateGroupChat(string name)
	{
		var usedIds = new HashSet<int>(Groups.Select(g => g.Id));
		var newId = 0;
		while (usedIds.Contains(newId))
		{
			newId++;
		}

		var groupChat = new GroupChat(newId, name);
		Groups.Add(groupChat);
		return groupChat;
	}

	/// <summary>
	/// Deletes the group chat with the specified ID, removing it from all members as well.
	/// </summary>
	/// <param name="id">The ID of the group chat to delete.</param>
	/// <returns><c>true</c> if the group chat was found and deleted; otherwise, <c>false</c>.</returns>
	public static bool DeleteGroupChat(int id)
	{
		foreach (var group in Groups)
		{
			if (group.Id != id) continue;
			group.RemoveAllMembers();
			group.RemoveAllTempMembers();
			
			return Groups.Remove(group);
		}

		return false;
	}

	/// <summary>
	/// Retrieves the group chat with the specified ID.
	/// </summary>
	/// <param name="id">The ID of the group chat to retrieve.</param>
	/// <returns>The <see cref="GroupChat"/> if found; otherwise, <c>null</c>.</returns>
	public static GroupChat GetGroupChat(int id)
	{
		foreach (var group in Groups)
		{
			if (group.Id == id)
				return group;
		}

		return null;
	}

	/// <summary>
	/// Deletes all group chats and removes each group from its members.
	/// </summary>
	public static void DeleteAllGroupChats()
	{
		foreach (var group in Groups)
		{
			group.RemoveAllMembers();
			group.RemoveAllTempMembers();
		}

		Groups.Clear();
	}

	public static void InitChatMembers()
	{
		foreach (var hub in ReferenceHub.AllHubs)
		{
			ChatMember.Init(hub);
		}
	}

	public static void DestroyChatMembers()
	{
		foreach (var hub in ReferenceHub.AllHubs)
		{
			var member = hub.GetChatMember();
			if (member == null) continue;
			GameObject.Destroy(member);
		}
	}
	
	public static void SendMessage(ChatMember sender, VoiceMessage msg)
	{
		if (sender.CurrentGroup != null && sender.GroupChatEnabled)
		{
			msg.Channel = VoiceChatChannel.RoundSummary;
			foreach (var member in sender.CurrentGroup.Members)
			{
				if (member.Hub == sender.Hub) continue;
				if (member.IsGroupMuted(sender.CurrentGroup)) continue;
				member.Hub.connectionToClient.Send(msg);
			}
		}

		// if (!sender.Hub.IsSCP())
		// 	return;

		if (sender.CanUseProximityChat && sender.ProximityChatEnabled)
		{
			msg.Channel = VoiceChatChannel.Proximity;
			foreach (var hub in ReferenceHub.AllHubs)
			{
				if (hub.roleManager.CurrentRole is SpectatorRole currentRole)
				{
					var specId = currentRole.SyncedSpectatedNetId;
					if (sender.CloseHubsNetIds.Contains(specId) || specId == sender.Hub.netId)
					{
						hub.connectionToClient.Send(msg);
					}
					continue;
				}
				if (!sender.CloseHubsNetIds.Contains(hub.netId)) continue;
				hub.connectionToClient.Send(msg);
			}
		}
	}
}