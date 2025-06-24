namespace VoiceManager.Features.EventArgs;

public class AddingGroupMemberEventArgs
{
	public AddingGroupMemberEventArgs(ChatMember chatMember, GroupChat group, bool isTemp)
	{
		ChatMember = chatMember;
		Group = group;
		IsTemp = isTemp;
	}
	
	public ChatMember ChatMember { get; }
	public GroupChat Group { get; }
	public bool IsTemp { get; set; }
	public bool IsAllowed { get; set; } = true;
}