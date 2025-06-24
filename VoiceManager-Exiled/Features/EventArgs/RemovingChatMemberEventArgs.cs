namespace VoiceManager.Features.EventArgs;

public class RemovingChatMemberEventArgs
{
	public RemovingChatMemberEventArgs(ChatMember member)
	{
		ChatMember = member;
	}
	
	public ChatMember ChatMember { get; }
	public bool IsAllowed { get; set; } = true;
}