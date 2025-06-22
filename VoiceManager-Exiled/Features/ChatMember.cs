using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Exiled.API.Features;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using Respawning.Objectives;
using SpeakerToy = LabApi.Features.Wrappers.SpeakerToy;

namespace VoiceManager.Features;

public class ChatMember
{
	private static Dictionary<ReferenceHub, ChatMember> Members { get; } = new();
	public ReferenceHub Hub { get; }
	public List<GroupChat> Groups { get; } = new();
	public HashSet<GroupChat> MutedGroups { get; } = new();
	public GroupChat CurrentGroup { get; private set; }
	public SpeakerToy SpeakerToy { get; }
	public bool GroupChatEnabled { get; private set; }
	public bool ProximityChat { get; private set; }
	public bool ProximityChatEnabled { get; private set; }
	public static Action<ReferenceHub> OnMemberAdded { get; set; }
	public static Action<ReferenceHub> OnMemberRemoved { get; set; }
	public static Action<ChatMember> OnMemberChanged { get; set; }

	private ChatMember(ReferenceHub hub)
	{
		Hub = hub;
		SpeakerToy = SpeakerToy.Create(hub.GetPosition());
		SpeakerToy.ControllerId = (byte)hub.PlayerId;
	}
	
	public static bool TryGet(ReferenceHub hub, out ChatMember member)	{
		return Members.TryGetValue(hub, out member);
	}

	public static ChatMember Get(ReferenceHub hub)
	{
		if (Members.TryGetValue(hub, out var value))
			return value;

		var member = new ChatMember(hub);
		if (Members.Count == 0)
		{
			OnMemberAdded += HintProvider.Provider.CreateHint;
			OnMemberRemoved += HintProvider.Provider.RemoveHint;
		}
		Members.Add(hub, member);
		OnMemberAdded?.Invoke(hub);
		return member;
	}

	public static ChatMember Get(Player player) => Get(player?.ReferenceHub);

	public static void Remove(ReferenceHub hub)
	{
		if (!Members.TryGetValue(hub, out var member)) return;
		member.SetProximityChat(false);
		member.RemoveAllGroups();
		member.SpeakerToy.Destroy();
		OnMemberRemoved?.Invoke(hub);
		if (Members.Count == 1)
		{
			OnMemberAdded -= HintProvider.Provider.CreateHint;
			OnMemberRemoved -= HintProvider.Provider.RemoveHint;
		}
		Members.Remove(hub);
	}

	public static void Remove(Player player) => Remove(player?.ReferenceHub);

	public bool AddGroup(GroupChat group)
	{
		if (Groups.Contains(group))
			return false;
		Groups.Add(group);
		if (CurrentGroup == null)
		{
			SetCurrentGroup(group);
		}

		return true;
	}

	public bool RemoveGroup(GroupChat group)
	{
		if (!Groups.Remove(group)) return false;
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

		return true;
	}

	public void RemoveAllGroups()
	{
		foreach (var group in Groups.ToArray())
		{
			group.TryRemoveMember(this);
		}

		Groups.Clear();
	}

	public bool SetMuteCurrentGroup(bool mute)
	{
		if (mute)
		{
			if (!MutedGroups.Add(CurrentGroup)) return false;
			return true;
		}

		if (!MutedGroups.Remove(CurrentGroup)) return false;
		OnMemberChanged?.Invoke(this);
		return true;
	}

	public bool SetMuteGroup(GroupChat groupChat, bool mute)
	{
		if (mute && IsGroupMuted(groupChat))
			return false;
			
		foreach (var group in Groups)
		{
			if (!group.Equals(groupChat)) continue;
			var result = mute ? MutedGroups.Add(group) : MutedGroups.Remove(group);
			OnMemberChanged?.Invoke(this);
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

		CurrentGroup = group;
		OnMemberChanged?.Invoke(this);
	}

	public void SetGroupChatEnabled(bool value)
	{
		if (Groups.Count < 1)
			return;
		GroupChatEnabled = value;
		OnMemberChanged?.Invoke(this);
	}

	public void SetProximityChatEnabled(bool value)
	{
		if (!ProximityChat || !Hub.IsSCP())
			return;
		
		Settings.TryGetSetting(nameof(VoiceEntry.Config.MinProximityDistance), out float minProximityDistance);
		Settings.TryGetSetting(nameof(VoiceEntry.Config.MaxProximityDistance), out float maxProximityDistance);
		SpeakerToy.MinDistance = minProximityDistance;
		SpeakerToy.MaxDistance = maxProximityDistance;

		ProximityChatEnabled = value;
		OnMemberChanged?.Invoke(this);
	}

	public void SetProximityChat(bool state)
	{
		if (ProximityChat == state) return;
		if (state && !Hub.IsSCP()) return;
		ProximityChat = state;

		if (!state) ProximityChatEnabled = false;
		OnMemberChanged?.Invoke(this);
	}

	public static string GetHintText(ReferenceHub hub)
	{
		if (!ChatMember.TryGet(hub, out var member))
			return "";
		
		if (!member.ProximityChat && member.Groups.Count < 1)
			return "";
		
		var sb = StringBuilderPool.Shared.Rent();
		sb.Append("<align=\"left\">");
		
		if (member.CurrentGroup != null && member.Groups.Count > 0)
		{
			var mutedColor = member.IsGroupMuted(member.CurrentGroup)
				? "<color=green>True</color>"
				: "<color=red>False</color>";
			var chatColor = member.GroupChatEnabled
				? "<color=green>Enabled</color>"
				: "<color=red>Disabled</color>";
			
			var groupValues = new Dictionary<string, string>
			{
				["groupName"] = member.CurrentGroup.Name,
				["muted"]     = mutedColor,
				["enabled"]   = chatColor
			};
			
			var line = ParseTemplate(VoiceEntry.Instance.Translation.HintGroup, groupValues);
			sb.AppendLine(line);
		}

		if (member.ProximityChat)
		{
			var proxColor = member.ProximityChatEnabled
				? "<color=green>Enabled</color>"
				: "<color=red>Disabled</color>";
			
			var proxValues = new Dictionary<string, string>
			{
				["proximity"] = proxColor
			};

			var line = ParseTemplate(VoiceEntry.Instance.Translation.HintProximity, proxValues);
			sb.AppendLine(line);
		}
		return StringBuilderPool.Shared.ToStringReturn(sb);
	}
	
	public static string ParseTemplate(string template, Dictionary<string, string> values)
	{
		return Regex.Replace(template, @"\{(\w+)\}", m =>
		{
			var key = m.Groups[1].Value;
			return values.TryGetValue(key, out var val) ? val : m.Value;
		});
	}

	public override string ToString()
	{
		return $"{Hub.GetNickname()} | {Hub.GetRoleId()}";
	}
}