namespace VoiceManager.Features.EventArgs;

public class AddedGroupMemberEventArgs
{
	public AddedGroupMemberEventArgs(ChatMember chatMember, GroupChat group)
	{
		ChatMember = chatMember;
		Group = group;
	}
	
	public ChatMember ChatMember { get; }
	public GroupChat Group { get; }
}