using System.Collections.Generic;
using LabApi.Features.Wrappers;
using NorthwoodLib.Pools;
using VoiceManager.Features.MonoBehaviours;

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
		if (isTemp)
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
		return true;
	}
	
	public bool TryAddMember(Player player, bool isTemp = false)
	{
		return TryAddMember(player.GetChatMember(), isTemp);
	}
	
	public bool TryAddMember(ReferenceHub hub, bool isTemp = false)
	{
		return TryAddMember(hub.GetChatMember(), isTemp);
	}

	public bool TryRemoveMember(ChatMember member)
	{
		if (!TempMembers.Remove(member) && !Members.Remove(member))
		{
			return false;
		}
	
		member.RemoveGroup(this);
		return true;
	}
	
	public bool TryRemoveMember(Player player)
	{
		return TryRemoveMember(player.GetChatMember());
	}
	
	public bool TryRemoveMember(ReferenceHub hub)
	{
		return TryRemoveMember(hub.GetChatMember());
	}

	public void RemoveAllTempMembers()
	{
		foreach (var member in TempMembers)
		{
			member.RemoveGroup(this);
		}
		TempMembers.Clear();
	}

	public void RemoveAllMembers()
	{
		foreach (var member in Members)
		{
			member.RemoveGroup(this);
		}
		Members.Clear();
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
		foreach (var member in Members)
		{
			sb.AppendLine(member.ToString());
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