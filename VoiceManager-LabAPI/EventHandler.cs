using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Extensions;
using LabApi.Features.Wrappers;
using VoiceChat;
using VoiceChat.Networking;
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

	public override void OnPlayerUsingIntercom(PlayerUsingIntercomEventArgs ev)
	{
		if (!ChatMember.TryGet(ev.Player.ReferenceHub, out ChatMember member))
			return;
		member.SetGroupChat(false);
		member.UsingIntercom = true;
	}

	public override void OnPlayerUsedIntercom(PlayerUsedIntercomEventArgs ev)
	{
		if (!ChatMember.TryGet(ev.Player.ReferenceHub, out ChatMember member))
			return;
		member.UsingIntercom = false;
	}

	public override void OnPlayerSendingVoiceMessage(PlayerSendingVoiceMessageEventArgs ev)
	{
		if (ev.Message.Channel is not VoiceChatChannel.ScpChat and not VoiceChatChannel.Proximity
		    and not VoiceChatChannel.Spectator)
			return;

		if (!ChatMember.TryGet(ev.Player.ReferenceHub, out ChatMember member))
			return;

		if (member.ProximityChat)
		{
			ChatManager.SendMessage(member, ev.Message);
			ev.IsAllowed = false;
			return;
		}

		if (member.Groups.Count < 1)
			return;

		if (!member.GroupChat)
			return;

		ChatManager.SendMessage(member, ev.Message);
		ev.IsAllowed = false;
	}

	public override void OnPlayerChangedRole(PlayerChangedRoleEventArgs ev)
	{
		ChatMember.TryGet(ev.Player.ReferenceHub, out ChatMember member);

		if (member != null)
		{
			if (!ev.NewRole.RoleTypeId.IsScp())
				member.SetHasProximityChat(false);

			member.SetProximityChat(false);
			member.SetGroupChat(false);
		}

		if (VoiceEntry.Instance.Config.AutoInitProximityChatRoles &&
		    VoiceEntry.Instance.Config.ProximityChatRoles.Contains(ev.NewRole.RoleTypeId))
		{
			member ??= ChatMember.Get(ev.Player);
			member.SetHasProximityChat(true);
		}

		if (VoiceEntry.Instance.Config.SendBroadcastOnRoleChange)
		{
			ev.Player.SendBroadcast(VoiceEntry.Instance.Translation.BroadcastMessage,
				VoiceEntry.Instance.Config.BroadcastDuration);
		}
	}
}