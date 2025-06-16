using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using VoiceChat;
using VoiceManager.Features;
using VoiceManager.Features.MonoBehaviours;

namespace VoiceManager;

public class EventHandler : CustomEventsHandler
{
	public override void OnServerRoundRestarted()
	{
		ChatManager.DeleteAllGroupChats();
	}

	public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
	{
		if (!VoiceManager.AutoInitChatMembers) return;
		if (VoiceManager.Singleton.Config.Debug)
			Log.Debug($"Player {ev.Player.Nickname} joined. Initialize new chat member");
		ChatMember.Init(ev.Player.ReferenceHub);
	}

	public override void OnPlayerDeath(PlayerDeathEventArgs ev)
	{
		var member = ev.Player.GetChatMember();
		if (member == null) return;
		if (member.TempCanUseProximityChat)
			member.SetCanUseProximityChat(false);
		
		foreach (var group in member.Groups)
		{
			group.TryRemoveMember(member, true);
		}
	}

	public override void OnPlayerSendingVoiceMessage(PlayerSendingVoiceMessageEventArgs ev)
	{
		if (ev.Message.Channel is not VoiceChatChannel.ScpChat and not VoiceChatChannel.Proximity)
			return;

		if (!ev.Player.IsAlive)
			return;

		var member = ev.Player.GetChatMember();

		if (member == null) return;

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

	public override void OnPlayerChangedRole(PlayerChangedRoleEventArgs ev)
	{
		var config = VoiceManager.Singleton.Config;

		var member = ev.Player.GetChatMember();
		if (member != null)
		{
			member.OnPlayerChangedRole(ev.NewRole);
			return;
		}

		if (!VoiceManager.AutoInitChatMembers) return;

		if (config.Debug)
			Log.Debug($"{ev.Player.Nickname} has no chat member. Initialize new chat member.");
		ChatMember.Init(ev.Player.ReferenceHub);
	}
}