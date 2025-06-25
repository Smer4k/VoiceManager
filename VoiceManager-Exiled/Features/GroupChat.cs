using System.Collections.Generic;
using Exiled.API.Features;
using NorthwoodLib.Pools;
using PlayerRoles;
using Respawning.Objectives;
using VoiceManager.Features.EventArgs;

namespace VoiceManager.Features;

public class GroupChat
{
	public int Id { get; }
	public string Name { get; private set; }
	public HashSet<ChatMember> Members { get; } = new(32);
	public HashSet<ChatMember> TempMembers { get; } = new(32);

	public GroupChat(int id, string name)
	{
		Id = id;
		Name = name;
	}

	public bool TryAddMember(ChatMember member, bool isTemp = false)
	{
		AddingGroupMemberEventArgs ev = Events.OnAddingGroupMember(member, this, isTemp);
		if (!ev.IsAllowed)
			return false;
		
		if (ev.IsTemp)
		{
			Members.Remove(member);
			if (!TempMembers.Add(member))
				return false;
		}
		else
		{
			TempMembers.Remove(member);
			if (!Members.Add(member))
				return false;
		}

		member.AddGroup(this);
		Events.OnAddedGroupMember(member, this);
		return false;
	}
	
	public bool TryAddMember(Player player, bool isTemp = false)
	{
		return TryAddMember(ChatMember.Get(player), isTemp);
	}
	
	public bool TryAddMember(ReferenceHub hub, bool isTemp = false)
	{
		return TryAddMember(ChatMember.Get(hub), isTemp);
	}

	public bool TryRemoveMember(ChatMember member)
	{
		RemovingGroupMemberEventArgs ev = Events.OnRemovingGroupMember(member, this);
		if (!ev.IsAllowed)
			return false;
		
		if (!TempMembers.Remove(member) && !Members.Remove(member))
		{
			return false;
		}
	
		member.RemoveGroup(this);
		Events.OnRemovedGroupMember(member, this);
		return true;
	}
	
	public bool TryRemoveMember(Player player)
	{
		return TryRemoveMember(ChatMember.Get(player));
	}
	
	public bool TryRemoveMember(ReferenceHub hub)
	{
		return TryRemoveMember(ChatMember.Get(hub));
	}

	public void RemoveAllTempMembers()
	{
		foreach (var member in TempMembers)
		{
			TryRemoveMember(member);
		}
	}

	public void RemoveAllMembers()
	{
		foreach (var member in Members)
		{
			TryRemoveMember(member);
		}
	}

	public void SetName(string value)
	{
		Name = value;
	}

	public bool IsTempMember(ChatMember member)
	{
		return TempMembers.Contains(member);
	}

	public string GetAllMembersText()
	{
		var sb = StringBuilderPool.Shared.Rent();
		
		Dictionary<string, string> values = new()
		{
			["icon"] = "\ud83d\udd08",
			["nickname"] = "test",
			["role"] = "",
		};

		foreach (var member in Members)
		{
			values["icon"] = member.IsGroupMuted(this) ? "\ud83d\udd07" : "\ud83d\udd08";
			values["nickname"] = member.Hub.GetNickname();
			values["role"] = member.Hub.GetRoleId().ToString();
			sb.AppendLine(TranslationParser.ParseTemplate(VoiceEntry.Instance.Translation.HintGroupMembers, values));
		}
		foreach (var member in TempMembers)
		{
			values["icon"] = member.IsGroupMuted(this) ? "\ud83d\udd07" : "\ud83d\udd08";
			values["nickname"] = member.Hub.GetNickname();
			values["role"] = member.Hub.GetRoleId().ToString();
			sb.AppendLine(TranslationParser.ParseTemplate(VoiceEntry.Instance.Translation.HintGroupMembers, values));
		}
		return StringBuilderPool.Shared.ToStringReturn(sb);
	}

	public override bool Equals(object obj)
	{
		if (obj is not GroupChat chatGroup)
			return false;
		return Id == chatGroup.Id;
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}
}