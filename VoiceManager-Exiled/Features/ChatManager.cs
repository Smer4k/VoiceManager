using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Spectating;
using PlayerRoles.Voice;
using UnityEngine;
using VoiceChat;
using VoiceChat.Networking;
using VoiceManager.Features.EventArgs;

namespace VoiceManager.Features;

public static class ChatManager
{
	public static HashSet<GroupChat> Groups { get; } = new();
	
	public static GroupChat CreateGroupChat(int id, string name)
	{
		CreatingGroupEventArgs ev = Events.OnCreatingGroup(id, name);
		if (!ev.IsAllowed)
			return null;
		var chatGroup = new GroupChat(id, name);
		return Groups.Add(chatGroup) ? chatGroup : null;
	}
	
	public static GroupChat CreateGroupChat(string name)
	{
		var usedIds = new HashSet<int>(Groups.Select(g => g.Id));
		var newId = 0;
		while (usedIds.Contains(newId))
		{
			newId++;
		}

		CreatingGroupEventArgs ev = Events.OnCreatingGroup(newId, name);
		if (!ev.IsAllowed)
			return null;
		
		var groupChat = new GroupChat(newId, name);
		Groups.Add(groupChat);
		return groupChat;
	}
	
	public static bool RemoveGroupChat(GroupChat groupChat)
	{
		if (groupChat == null)
			return false;

		RemovingGroupEventArgs ev = Events.OnRemovingGroup(groupChat);
		if (!ev.IsAllowed)
			return false;
		
		groupChat.RemoveAllMembers();
		groupChat.RemoveAllTempMembers();

		return Groups.Remove(groupChat);
	}
	
	public static bool RemoveGroupChat(int id)
	{
		foreach (var group in Groups)
		{
			if (group.Id != id) continue;
			return RemoveGroupChat(group);
		}

		return false;
	}
	
	public static GroupChat GetGroupChat(int id)
	{
		foreach (var group in Groups)
		{
			if (group.Id == id)
				return group;
		}

		return null;
	}
	
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
		if (sender.CurrentGroup != null && sender.GroupChat)
		{
			msg.Channel = VoiceChatChannel.RoundSummary;
			foreach (var target in sender.CurrentGroup.Members)
			{
				if (target.Hub == sender.Hub) continue;
				if (target.IsGroupMuted(sender.CurrentGroup)) continue;
				target.Hub.connectionToClient.Send(msg);
			}
		}

		if (!sender.HasProximityChat || !sender.ProximityChat || !sender.Hub.IsAlive()) return;

		Settings.TryGetSetting(nameof(VoiceEntry.Config.Use3DProximityChat), out bool use3DProximityChat);
		
		if (use3DProximityChat)
		{
			sender.SpeakerToy.Position = sender.Hub.GetPosition();
			
			var opusHandler = OpusHandler.Get(sender.Hub);
			var decodedBuffer = new float[480];
			opusHandler.Decoder.Decode(msg.Data, msg.DataLength, decodedBuffer);

			Settings.TryGetSetting(nameof(VoiceEntry.Config.Volume3DProximityChat),
				out float volumeProximityChat);

			for (var i = 0; i < decodedBuffer.Length; i++)
			{
				decodedBuffer[i] *= volumeProximityChat;
			}

			var encodedBuffer = new byte[512];
			var dataLen = opusHandler.Encoder.Encode(decodedBuffer, encodedBuffer);

			var audioMsg = new AudioMessage(sender.SpeakerToy.ControllerId, encodedBuffer, dataLen);
			foreach (var target in Player.List)
			{
				if (target.Role.Base is not IVoiceRole voiceRole)
					continue;

				if (voiceRole.VoiceModule.ValidateReceive(sender.Hub, VoiceChatChannel.Proximity) == VoiceChatChannel.None)
					continue;

				target.ReferenceHub.connectionToClient.Send(audioMsg);
			}

			return;
		}

		foreach (var target in Player.List)
		{
			if (target.Role.Base is not IVoiceRole voiceRole)
				continue;

			var targetPos = target.Position;
			if (target.Role.Base is SpectatorRole spectatorRole)
			{
				targetPos = spectatorRole.DeathPosition.Position;
				if (Player.TryGet(spectatorRole.SyncedSpectatedNetId, out var specPlayer))
				{
					targetPos = specPlayer.ReferenceHub.GetPosition();
				}
			}

			var dist = Vector3.Distance(sender.Hub.GetPosition(), targetPos);
			Settings.TryGetSetting(nameof(VoiceEntry.Config.MaxProximityDistance), out float maxProximityDistance);

			if (dist >= maxProximityDistance) 
				continue;

			if (voiceRole.VoiceModule.ValidateReceive(sender.Hub, VoiceChatChannel.Proximity) == VoiceChatChannel.None)
			 	continue;
			
			msg.Channel = VoiceChatChannel.Proximity;
			target.ReferenceHub.connectionToClient.Send(msg);
		}
	}
}