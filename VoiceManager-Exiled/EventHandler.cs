using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using VoiceChat;
using VoiceManager.Features;

namespace VoiceManager;

public class EventHandler
{
	public void OnServerRoundRestarted()
	{
		ChatManager.DeleteAllGroupChats();
		ChatManager.DeleteAllChatMembers();
	}

	public void OnPlayerLeft(LeftEventArgs ev)
	{
		ChatMember.Remove(ev.Player);
		OpusHandler.Remove(ev.Player);
	}

	public void OnPlayerDeath(DiedEventArgs ev)
	{
		if (!ChatMember.TryGet(ev.Player.ReferenceHub, out ChatMember member)) 
			return;

		foreach (var group in member.Groups.ToArray())
		{
			if (group.IsTempMember(member)) group.TryRemoveMember(member);
		}
	}

	public void OnPlayerSendingVoiceMessage(VoiceChattingEventArgs ev)
	{
		if (ev.VoiceMessage.Channel is not VoiceChatChannel.ScpChat and not VoiceChatChannel.Proximity and not VoiceChatChannel.Spectator)
			return;
		
		if (Intercom.Speaker == ev.Player)
			return;
		
		if (!ChatMember.TryGet(ev.Player.ReferenceHub, out ChatMember member)) 
			return;

		if (member.ProximityChat)
		{
			ChatManager.SendMessage(member, ev.VoiceMessage);
			ev.IsAllowed = false;
			return;
		}

		if (member.Groups.Count < 1)
			return;

		if (!member.GroupChat)
			return;

		ChatManager.SendMessage(member, ev.VoiceMessage);
		ev.IsAllowed = false;
	}

	public void OnPlayerChangingRole(ChangingRoleEventArgs ev)
	{
		ChatMember.TryGet(ev.Player.ReferenceHub, out ChatMember member);

		if (member != null)
		{
			if (!ev.NewRole.IsScp())
				member.SetHasProximityChat(false);
			
			member.SetProximityChat(false);
			member.SetGroupChat(false);
		}

		if (VoiceEntry.Instance.Config.AutoInitProximityChatRoles &&
		    VoiceEntry.Instance.Config.ProximityChatRoles.Contains(ev.NewRole))
		{
			member ??= ChatMember.Get(ev.Player);
			member.SetHasProximityChat(true);
		}

		if (VoiceEntry.Instance.Config.SendBroadcastOnRoleChange)
		{
			ev.Player.Broadcast(VoiceEntry.Instance.Config.BroadcastDuration, VoiceEntry.Instance.Translation.BroadcastMessage);
		}
	}
}