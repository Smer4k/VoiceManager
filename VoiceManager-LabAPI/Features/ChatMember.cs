using System.Collections.Generic;
using System.Text.RegularExpressions;
using LabApi.Features.Wrappers;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using Respawning.Objectives;
using VoiceManager.Features.EventArgs;

namespace VoiceManager.Features;

public class ChatMember
{
	private static Dictionary<ReferenceHub, ChatMember> Members { get; } = new();
	public ReferenceHub Hub { get; }
	public List<GroupChat> Groups { get; } = new();
	public HashSet<GroupChat> MutedGroups { get; } = new();
	public GroupChat CurrentGroup { get; private set; }
	public SpeakerToy SpeakerToy { get; }
	public bool GroupChat { get; private set; }
	public bool HasProximityChat { get; private set; }
	public bool ProximityChat { get; private set; }
	public bool UsingIntercom { get; set; }

	private ChatMember(ReferenceHub hub)
	{
		Hub = hub;
		SpeakerToy = SpeakerToy.Create(hub.GetPosition());
		SpeakerToy.ControllerId = (byte)hub.PlayerId;
	}

	public static bool TryGet(ReferenceHub hub, out ChatMember member)
	{
		return Members.TryGetValue(hub, out member);
	}

	public static ChatMember Get(ReferenceHub hub)
	{
		if (hub.IsHost) return null;
		if (Members.TryGetValue(hub, out var value))
			return value;

		CreatingChatMemberEventArgs ev = Events.OnCreatingChatMember(hub);
		if (!ev.IsAllowed)
			return null;

		var member = new ChatMember(hub);
		Members.Add(hub, member);
		if (VoiceEntry.Instance.Config.AutoDefaultHints) HintProvider.CreateDefaultHints(hub);
		Events.OnCreatedChatMember(member);
		return member;
	}

	public static ChatMember Get(Player player) => Get(player?.ReferenceHub);

	public static void Remove(ReferenceHub hub)
	{
		if (!Members.TryGetValue(hub, out var member)) return;
		RemovingChatMemberEventArgs ev = Events.OnRemovingChatMember(member);
		if (!ev.IsAllowed)
			return;

		member.SetHasProximityChat(false);
		member.RemoveAllGroups();
		member.SpeakerToy.Destroy();
		if (VoiceEntry.Instance.Config.AutoDefaultHints) HintProvider.RemoveHints(hub);
		Members.Remove(hub);
		Events.OnRemovedChatMember(hub);
	}

	public static void Remove(Player player) => Remove(player?.ReferenceHub);

	internal void AddGroup(GroupChat group)
	{
		Groups.Add(group);
		if (CurrentGroup == null)
		{
			SetCurrentGroup(group);
		}
	}

	internal void RemoveGroup(GroupChat group)
	{
		Groups.Remove(group);
		MutedGroups.Remove(group);
		if (Groups.Count > 0)
		{
			if (CurrentGroup.Equals(group))
				SetCurrentGroup(Groups[0]);
		}
		else
		{
			SetCurrentGroup(null);
		}
	}

	public void RemoveAllGroups()
	{
		foreach (var group in Groups.ToArray())
		{
			group.TryRemoveMember(this);
		}
	}

	public bool SetMuteCurrentGroup(bool mute)
	{
		ChangingMuteGroupEventArgs ev = Events.OnChangingMuteGroup(this, IsGroupMuted(CurrentGroup), mute);
		if (!ev.IsAllowed)
			return false;

		if (mute)
		{
			if (!MutedGroups.Add(CurrentGroup)) return false;
			return true;
		}

		if (!MutedGroups.Remove(CurrentGroup)) return false;
		return true;
	}

	public bool SetMuteGroup(GroupChat groupChat, bool mute)
	{
		var isGroupMuted = IsGroupMuted(groupChat);
		if (mute && isGroupMuted)
			return false;

		ChangingMuteGroupEventArgs ev = Events.OnChangingMuteGroup(this, isGroupMuted, mute);
		if (!ev.IsAllowed)
			return false;

		foreach (var group in Groups)
		{
			if (!group.Equals(groupChat)) continue;
			var result = mute ? MutedGroups.Add(group) : MutedGroups.Remove(group);
			return result;
		}

		return false;
	}

	public bool SetMuteGroup(int groupId, bool mute) => SetMuteGroup(new GroupChat(groupId, ""), mute);

	public bool IsGroupMuted(GroupChat group) => MutedGroups.Contains(group);

	public bool IsGroupMuted(int groupId) => IsGroupMuted(new GroupChat(groupId, ""));

	public void SelectNextGroup()
	{
		if (Groups.Count == 0)
			return;

		if (CurrentGroup == null)
			return;

		var index = Groups.IndexOf(CurrentGroup);

		if (index == -1 || index == Groups.Count - 1)
		{
			SetCurrentGroup(Groups[0]);
		}
		else
		{
			SetCurrentGroup(Groups[index + 1]);
		}
	}

	public void SetCurrentGroup(GroupChat group)
	{
		if (CurrentGroup != null && CurrentGroup.Equals(group))
			return;

		ChangingCurrentGroupEventArgs ev = Events.OnChangingCurrentGroup(this, CurrentGroup, group);
		if (!ev.IsAllowed)
			return;

		CurrentGroup = group;
	}

	public void SetGroupChat(bool value)
	{
		if (Groups.Count < 1 || UsingIntercom)
			return;

		ChangingGroupChatEventArgs ev = Events.OnChangingGroupChat(this, GroupChat, value);
		if (!ev.IsAllowed)
			return;

		GroupChat = value;
	}

	public void SetProximityChat(bool value)
	{
		if (!HasProximityChat || !Hub.IsSCP())
			return;

		ChangingProximityChatEventArgs ev = Events.OnChangingProximityChat(this, ProximityChat, value);
		if (!ev.IsAllowed)
			return;

		Settings.TryGetSetting(nameof(VoiceEntry.Config.MinProximityDistance), out float minProximityDistance);
		Settings.TryGetSetting(nameof(VoiceEntry.Config.MaxProximityDistance), out float maxProximityDistance);
		SpeakerToy.MinDistance = minProximityDistance;
		SpeakerToy.MaxDistance = maxProximityDistance;

		ProximityChat = value;
	}

	public void SetHasProximityChat(bool value)
	{
		if (HasProximityChat == value) return;
		if (value && !Hub.IsSCP()) return;

		ChangingHasProximityChatEventArgs ev = Events.OnChangingHasProximityChat(this, HasProximityChat, value);
		if (!ev.IsAllowed)
			return;

		HasProximityChat = value;

		if (!value) ProximityChat = false;
	}

	public static string GetHintInfoText(ReferenceHub hub)
	{
		if (!ChatMember.TryGet(hub, out var member))
			return "";

		if (!member.HasProximityChat && member.Groups.Count < 1)
			return "";

		var sb = StringBuilderPool.Shared.Rent();

		if (member.CurrentGroup != null && member.Groups.Count > 0)
		{
			var mutedColor = member.IsGroupMuted(member.CurrentGroup)
				? "<color=green>True</color>"
				: "<color=red>False</color>";
			var chatColor = member.GroupChat
				? "<color=green>Enabled</color>"
				: "<color=red>Disabled</color>";

			var groupValues = new Dictionary<string, string>
			{
				["groupName"] = member.CurrentGroup.Name,
				["muted"] = mutedColor,
				["enabled"] = chatColor
			};

			var line = TranslationParser.ParseTemplate(VoiceEntry.Instance.Translation.HintGroup, groupValues);
			sb.AppendLine(line);
		}

		if (member.HasProximityChat)
		{
			var proxColor = member.ProximityChat
				? "<color=green>Enabled</color>"
				: "<color=red>Disabled</color>";

			var proxValues = new Dictionary<string, string>
			{
				["proximity"] = proxColor
			};

			var line = TranslationParser.ParseTemplate(VoiceEntry.Instance.Translation.HintProximity, proxValues);
			sb.AppendLine(line);
		}

		return StringBuilderPool.Shared.ToStringReturn(sb);
	}

	public static string GetHintGroupMembersText(ReferenceHub hub)
	{
		if (!ChatMember.TryGet(hub, out var member))
			return "";
		return member.CurrentGroup?.GetAllMembersText();
	}

	public override string ToString()
	{
		return $"{Hub.GetNickname()} | {Hub.GetRoleId()}";
	}
}