namespace VoiceManager.Features.EventArgs;

public class RemovingGroupMemberEventArgs
{
	public RemovingGroupMemberEventArgs(ChatMember chatMember, GroupChat group)
	{
		ChatMember = chatMember;
		Group = group;
	}

	public ChatMember ChatMember { get; }
	public GroupChat Group { get; }
	public bool IsAllowed { get; set; } = true;
}