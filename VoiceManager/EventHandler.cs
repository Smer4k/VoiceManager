using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using VoiceChat;
using VoiceManager.Features;

namespace VoiceManager;

public class EventHandler : CustomEventsHandler
{
	public override void OnServerRoundRestarted()
	{
		ChatManager.DeleteAllGroupChats();
	}

	public override void OnPlayerLeft(PlayerLeftEventArgs ev)
	{
		ChatMember.Remove(ev.Player);
		OpusHandler.Remove(ev.Player);
	}

	public override void OnPlayerDeath(PlayerDeathEventArgs ev)
	{
		if (!ChatMember.Contains(ev.Player)) return;
		var member = ChatMember.Get(ev.Player);

		if (member.TempProximityChat)
			member.SetProximityChat(false);

		foreach (var group in member.Groups.ToArray())
		{
			if (group.IsTempMember(member)) group.TryRemoveMember(member);
		}
	}

	public override void OnPlayerSendingVoiceMessage(PlayerSendingVoiceMessageEventArgs ev)
	{
		if (ev.Message.Channel is not VoiceChatChannel.ScpChat and not VoiceChatChannel.Proximity)
			return;

		if (!ChatMember.Contains(ev.Player)) return;

		var member = ChatMember.Get(ev.Player);

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
		ChatMember member = null;

		if (ChatMember.Contains(ev.Player))
			member = ChatMember.Get(ev.Player);

		if (member != null)
		{
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