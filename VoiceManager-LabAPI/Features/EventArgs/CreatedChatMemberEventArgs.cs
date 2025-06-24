namespace VoiceManager.Features.EventArgs;

public class CreatedChatMemberEventArgs
{
	public CreatedChatMemberEventArgs(ChatMember chatMember)
	{
		ChatMember = chatMember;
	}
	
	public ChatMember ChatMember { get; }
}