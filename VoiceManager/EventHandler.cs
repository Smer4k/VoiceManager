using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Extensions;
using VoiceChat;
using VoiceManager.Features;

namespace VoiceManager;

public class EventHandler : CustomEventsHandler
{
	public override void OnServerRoundRestarted()
	{
		ChatManager.DeleteAllGroupChats();
		ChatManager.DeleteAllChatMembers();
	}

	public override void OnPlayerLeft(PlayerLeftEventArgs ev)
	{
		ChatMember.Remove(ev.Player);
		OpusHandler.Remove(ev.Player);
	}

	public override void OnPlayerDeath(PlayerDeathEventArgs ev)
	{
		if (!ChatMember.TryGet(ev.Player.ReferenceHub, out ChatMember member)) 
			return;

		foreach (var group in member.Groups.ToArray())
		{
			if (group.IsTempMember(member)) group.TryRemoveMember(member);
		}
	}

	public override void OnPlayerSendingVoiceMessage(PlayerSendingVoiceMessageEventArgs ev)
	{
		if (ev.Message.Channel is not VoiceChatChannel.ScpChat and not VoiceChatChannel.Proximity)
			return;

		if (!ChatMember.TryGet(ev.Player.ReferenceHub, out ChatMember member)) 
			return;

		if (member.ProximityChatEnabled)
		{
			ChatManager.SendMessage(member, ev.Message);
			ev.IsAllowed = false;
			return;
		}

		if (member.Groups.Count < 1)
			return;

		if (!member.GroupChatEnabled)
			return;

		ChatManager.SendMessage(member, ev.Message);
		ev.IsAllowed = false;
	}

	public override void OnPlayerChangingRole(PlayerChangingRoleEventArgs ev)
	{
		ChatMember.TryGet(ev.Player.ReferenceHub, out ChatMember member);

		if (member != null)
		{
			if (!ev.NewRole.IsScp())
				member.SetProximityChat(false);
			
			member.SetProximityChatEnabled(false);
			member.SetGroupChatEnabled(false);
		}

		if (VoiceManager.VConfig.AutoInitProximityChatRoles &&
		    VoiceManager.VConfig.ProximityChatRoles.Contains(ev.NewRole))
		{
			member ??= ChatMember.Get(ev.Player);
			member.SetProximityChat(true);
		}

		if (VoiceManager.VConfig.SendBroadcastOnRoleChange)
		{
			ev.Player.SendBroadcast(VoiceManager.VConfig.BroadcastMessage, VoiceManager.VConfig.BroadcastDuration);
		}

		member?.UpdateDisplay();
	}
}