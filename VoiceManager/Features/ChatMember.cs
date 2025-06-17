using System.Collections.Generic;
using LabApi.Features.Wrappers;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using RueI.Displays;
using RueI.Elements;
using RueI.Extensions.HintBuilding;
using Display = RueI.Displays.Display;

namespace VoiceManager.Features;

public class ChatMember
{
	private static Dictionary<ReferenceHub, ChatMember> Members { get; } = new();
	public ReferenceHub Hub { get; }
	public List<GroupChat> Groups { get; } = new();
	public HashSet<GroupChat> MutedGroups { get; } = new();
	public GroupChat CurrentGroup { get; private set; }
	public Display Display { get; }
	public SpeakerToy SpeakerToy { get; }
	public bool GroupChatEnabled { get; private set; }
	public bool ProximityChat { get; private set; }
	public bool ProximityChatEnabled { get; private set; }
	public bool TempProximityChat { get; private set; }

	private ChatMember(ReferenceHub hub)
	{
		Hub = hub;
		SpeakerToy = SpeakerToy.Create(hub.GetPosition());
		SpeakerToy.ControllerId = (byte)hub.PlayerId;
		Display = new Display(DisplayCore.Get(hub));
		Display.Elements.Add(new SetElement(75, ""));
	}

	public static ChatMember Get(ReferenceHub hub)
	{
		if (Members.TryGetValue(hub, out var value))
			return value;

		var member = new ChatMember(hub);
		Members.Add(hub, member);
		return member;
	}

	public static ChatMember Get(Player player) => Get(player?.ReferenceHub);
	public static bool Contains(ReferenceHub hub) => Members.ContainsKey(hub);
	public static bool Contains(Player player) => Members.ContainsKey(player.ReferenceHub);

	public static void Remove(ReferenceHub hub)
	{
		if (Members.TryGetValue(hub, out var member))
		{
			member.SetProximityChat(false);
			member.RemoveAllGroups();
			member.SpeakerToy.Destroy();
			Members.Remove(hub);
		}
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
		foreach (var group in Groups)
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
			UpdateDisplay();
			return true;
		}

		if (!MutedGroups.Remove(CurrentGroup)) return false;
		UpdateDisplay();
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
			UpdateDisplay();
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

		UpdateDisplay();
	}

	public void SetCurrentGroup(GroupChat group)
	{
		if (CurrentGroup != null && CurrentGroup.Equals(group))
			return;

		CurrentGroup = group;
		UpdateDisplay();
	}

	public void SetGroupChatEnabled(bool value)
	{
		if (Groups.Count < 1)
			return;
		GroupChatEnabled = value;
		UpdateDisplay();
	}

	public void SetProximityChatEnabled(bool value)
	{
		if (!ProximityChat || !Hub.IsSCP())
			return;

		ProximityChatEnabled = value;
		UpdateDisplay();
	}

	public void SetProximityChat(bool state, bool isTemp = false)
	{
		if (ProximityChat == state || !Hub.IsSCP()) return;
		ProximityChat = state;
		TempProximityChat = isTemp;

		Settings.TryGetSetting(nameof(VoiceManager.VConfig.MinProximityDistance), out float minProximityDistance);
		Settings.TryGetSetting(nameof(VoiceManager.VConfig.MaxProximityDistance), out float maxProximityDistance);
		SpeakerToy.MinDistance = minProximityDistance;
		SpeakerToy.MaxDistance = maxProximityDistance;

		if (!state) ProximityChatEnabled = false;

		UpdateDisplay();
	}

	public void UpdateDisplay()
	{
		if (Display == null) return;
		if (!ProximityChat && Groups.Count < 1)
		{
			((SetElement)Display.Elements[0]).Content = "";
			Display.Update();
			return;
		}

		var pos = 0;
		var sb = StringBuilderPool.Shared.Rent();
		sb.SetSize(25);
		sb.SetLineHeight(25);
		sb.SetAlignment(HintBuilding.AlignStyle.Left);
		sb.SetMargins(60);
		if (CurrentGroup != null && Groups.Count > 0)
		{
			sb.AppendLine($"Current group: {CurrentGroup.Name}");
			var muted =
				$"Muted: {(IsGroupMuted(CurrentGroup) ? "<color=green>True</color>" : "<color=red>False</color>")}";
			sb.AppendLine(muted);
			var color = "red";
			if (GroupChatEnabled) color = "green";
			var groupChat = $"Group chat: <color={color}>{(GroupChatEnabled ? "Enabled" : "Disabled")}</color>";
			sb.AppendLine(groupChat);
			pos += 50;
		}

		if (ProximityChat)
		{
			var color = "red";
			if (ProximityChatEnabled) color = "green";
			sb.AppendLine($"Proximity chat: <color={color}>{(ProximityChatEnabled ? "Enabled" : "Disabled")}</color>");
			pos += 25;
		}

		sb.CloseLineHeight();
		sb.CloseSize();
		sb.CloseAlign();
		var elem = (SetElement)Display.Elements[0];
		elem.Content = StringBuilderPool.Shared.ToStringReturn(sb);
		elem.Position = pos;
		Display.Update();
	}
}