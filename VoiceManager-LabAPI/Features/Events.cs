using System;
using VoiceManager.Features.EventArgs;

namespace VoiceManager.Features;

public static class Events
{
	/// <summary>
	/// Invoked before adding a player to a group
	/// </summary>
	public static event Action<AddingGroupMemberEventArgs> AddingGroupMember;

	/// <summary>
	/// Invoked after adding player to group
	/// </summary>
	public static event Action<AddedGroupMemberEventArgs> AddedGroupMember;

	/// <summary>
	/// Invoked before removing a player from a group
	/// </summary>
	public static event Action<RemovingGroupMemberEventArgs> RemovingGroupMember;

	/// <summary>
	/// Invoked after player is removed from group
	/// </summary>
	public static event Action<RemovedGroupMemberEventArgs> RemovedGroupMember;

	/// <summary>
	/// Invoked before a new group is created.
	/// </summary>
	public static event Action<CreatingGroupEventArgs> CreatingGroup;

	/// <summary>
	/// Invoked before removing the group.
	/// </summary>
	public static event Action<RemovingGroupEventArgs> RemovingGroup;

	/// <summary>
	/// Invoked when the current group selection is about to change.
	/// </summary>
	public static event Action<ChangingCurrentGroupEventArgs> ChangingCurrentGroup;

	/// <summary>
	/// Invoked when a player attempts to enable or disable the group chat feature.
	/// </summary>
	public static event Action<ChangingGroupChatEventArgs> ChangingGroupChat;

	/// <summary>
	/// Invoked when a player attempts to enable or disable the proximity chat feature.
	/// </summary>
	public static event Action<ChangingProximityChatEventArgs> ChangingProximityChat;

	/// <summary>
	/// Invoked when a player is granted or revoked the ability to use proximity chat.
	/// </summary>
	public static event Action<ChangingHasProximityChatEventArgs> ChangingHasProximityChat;

	/// <summary>
	/// Invoked when a player attempts to mute or unmute the group.
	/// </summary>
	public static event Action<ChangingMuteGroupEventArgs> ChangingMuteGroup;

	/// <summary>
	/// Invoked after a new chat member has been created
	/// </summary>
	public static event Action<CreatedChatMemberEventArgs> CreatedChatMember;

	/// <summary>
	/// Invoked after a chat member has been removed
	/// </summary>
	public static event Action<RemovedChatMemberEventArgs> RemovedChatMember;

	/// <summary>
	/// Invoked before creating a new chat member
	/// </summary>
	public static event Action<CreatingChatMemberEventArgs> CreatingChatMember;

	/// <summary>
	/// Invoked before removing a chat member
	/// </summary>
	public static event Action<RemovingChatMemberEventArgs> RemovingChatMember;

	public static event Action<ChangingSettingEventArgs> ChangingSetting;

	public static AddingGroupMemberEventArgs OnAddingGroupMember(ChatMember member, GroupChat groupChat, bool isTemp)
	{
		AddingGroupMemberEventArgs ev = new(member, groupChat, isTemp);
		AddingGroupMember?.Invoke(ev);
		return ev;
	}

	public static void OnAddedGroupMember(ChatMember member, GroupChat groupChat)
	{
		AddedGroupMemberEventArgs ev = new(member, groupChat);
		AddedGroupMember?.Invoke(ev);
	}

	public static RemovingGroupMemberEventArgs OnRemovingGroupMember(ChatMember member, GroupChat groupChat)
	{
		RemovingGroupMemberEventArgs ev = new(member, groupChat);
		RemovingGroupMember?.Invoke(ev);
		return ev;
	}

	public static void OnRemovedGroupMember(ChatMember member, GroupChat groupChat)
	{
		RemovedGroupMemberEventArgs ev = new(member, groupChat);
		RemovedGroupMember?.Invoke(ev);
	}

	public static CreatingGroupEventArgs OnCreatingGroup(int groupId, string groupName)
	{
		CreatingGroupEventArgs ev = new(groupId, groupName);
		CreatingGroup?.Invoke(ev);
		return ev;
	}

	public static RemovingGroupEventArgs OnRemovingGroup(GroupChat groupChat)
	{
		RemovingGroupEventArgs ev = new(groupChat);
		RemovingGroup?.Invoke(ev);
		return ev;
	}

	public static ChangingCurrentGroupEventArgs OnChangingCurrentGroup(ChatMember member, GroupChat oldValue,
		GroupChat newValue)
	{
		ChangingCurrentGroupEventArgs ev = new(member, oldValue, newValue);
		ChangingCurrentGroup?.Invoke(ev);
		return ev;
	}

	public static ChangingGroupChatEventArgs OnChangingGroupChat(ChatMember member, bool oldValue, bool newValue)
	{
		ChangingGroupChatEventArgs ev = new(member, oldValue, newValue);
		ChangingGroupChat?.Invoke(ev);
		return ev;
	}

	public static ChangingProximityChatEventArgs OnChangingProximityChat(ChatMember member, bool oldValue,
		bool newValue)
	{
		ChangingProximityChatEventArgs ev = new(member, oldValue, newValue);
		ChangingProximityChat?.Invoke(ev);
		return ev;
	}

	public static ChangingHasProximityChatEventArgs OnChangingHasProximityChat(ChatMember member, bool oldValue,
		bool newValue)
	{
		ChangingHasProximityChatEventArgs ev = new(member, oldValue, newValue);
		ChangingHasProximityChat?.Invoke(ev);
		return ev;
	}

	public static ChangingMuteGroupEventArgs OnChangingMuteGroup(ChatMember member, bool oldValue, bool newValue)
	{
		ChangingMuteGroupEventArgs ev = new(member, oldValue, newValue);
		ChangingMuteGroup?.Invoke(ev);
		return ev;
	}

	public static void OnCreatedChatMember(ChatMember member)
	{
		CreatedChatMemberEventArgs ev = new(member);
		CreatedChatMember?.Invoke(ev);
	}

	public static void OnRemovedChatMember(ReferenceHub hub)
	{
		RemovedChatMemberEventArgs ev = new(hub);
		RemovedChatMember?.Invoke(ev);
	}

	public static CreatingChatMemberEventArgs OnCreatingChatMember(ReferenceHub hub)
	{
		CreatingChatMemberEventArgs ev = new(hub);
		CreatingChatMember?.Invoke(ev);
		return ev;
	}

	public static RemovingChatMemberEventArgs OnRemovingChatMember(ChatMember member)
	{
		RemovingChatMemberEventArgs ev = new(member);
		RemovingChatMember?.Invoke(ev);
		return ev;
	}

	public static ChangingSettingEventArgs OnChangingSetting(string settingName, object value)
	{
		ChangingSettingEventArgs ev = new(settingName, value);
		ChangingSetting?.Invoke(ev);
		return ev;
	}
}