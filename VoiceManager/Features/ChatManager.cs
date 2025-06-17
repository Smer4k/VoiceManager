using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Spectating;
using PlayerRoles.Voice;
using UnityEngine;
using VoiceChat;
using VoiceChat.Networking;

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

	public static void DeleteAllChatMembers()
	{
		foreach (var hub in ReferenceHub.AllHubs)
		{
			ChatMember.Remove(hub);
			OpusHandler.Remove(hub);
		}
	}

	public static void SendMessage(ChatMember sender, VoiceMessage msg)
	{
		if (sender.CurrentGroup != null && sender.GroupChatEnabled)
		{
			msg.Channel = VoiceChatChannel.RoundSummary;
			foreach (var target in sender.CurrentGroup.Members)
			{
				if (target.Hub == sender.Hub) continue;
				if (target.IsGroupMuted(sender.CurrentGroup)) continue;
				target.Hub.connectionToClient.Send(msg);
			}
		}

		if (!sender.ProximityChat || !sender.ProximityChatEnabled || !sender.Hub.IsAlive()) return;

		Settings.TryGetSetting(nameof(VoiceManager.VConfig.Use3DProximityChat), out bool use3DProximityChat);

		if (use3DProximityChat)
		{
			sender.SpeakerToy.Position = sender.Hub.GetPosition();
			
			var opusHandler = OpusHandler.Get(sender.Hub);
			var decodedBuffer = new float[480];
			opusHandler.Decoder.Decode(msg.Data, msg.DataLength, decodedBuffer);

			Settings.TryGetSetting(nameof(VoiceManager.VConfig.Volume3DProximityChat),
				out float volumeProximityChat);

			for (var i = 0; i < decodedBuffer.Length; i++)
			{
				decodedBuffer[i] *= volumeProximityChat;
			}

			var encodedBuffer = new byte[512];
			var dataLen = opusHandler.Encoder.Encode(decodedBuffer, encodedBuffer);

			var audioMsg = new AudioMessage(sender.SpeakerToy.ControllerId, encodedBuffer, dataLen);
			foreach (var target in Player.ReadyList)
			{
				if (target.RoleBase is not IVoiceRole voiceRole)
					continue;

				if (voiceRole.VoiceModule.ValidateReceive(sender.Hub, VoiceChatChannel.Proximity) == VoiceChatChannel.None)
					continue;

				target.ReferenceHub.connectionToClient.Send(audioMsg);
			}

			return;
		}

		foreach (var target in Player.ReadyList)
		{
			if (target.RoleBase is not IVoiceRole voiceRole)
				continue;

			var targetPos = target.Position;
			if (target.RoleBase is SpectatorRole spectatorRole)
			{
				targetPos = spectatorRole.DeathPosition.Position;
				if (target.CurrentlySpectating != null)
				{
					targetPos = target.CurrentlySpectating.Position;
				}
			}

			var dist = Vector3.Distance(sender.Hub.GetPosition(), targetPos);
			Settings.TryGetSetting(nameof(VoiceManager.VConfig.MaxProximityDistance), out float maxProximityDistance);

			if (dist >= maxProximityDistance) 
				continue;

			if (voiceRole.VoiceModule.ValidateReceive(sender.Hub, VoiceChatChannel.Proximity) == VoiceChatChannel.None)
			 	continue;
			
			msg.Channel = VoiceChatChannel.Proximity;
			target.ReferenceHub.connectionToClient.Send(msg);
		}
	}
}