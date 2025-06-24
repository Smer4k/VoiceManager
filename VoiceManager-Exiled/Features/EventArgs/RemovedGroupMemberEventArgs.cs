namespace VoiceManager.Features.EventArgs;

public class RemovedGroupMemberEventArgs
{
	public RemovedGroupMemberEventArgs(ChatMember chatMember, GroupChat group)
	{
		ChatMember = chatMember;
		Group = group;
	}
	
	public ChatMember ChatMember { get; }
	public GroupChat Group { get; }
}