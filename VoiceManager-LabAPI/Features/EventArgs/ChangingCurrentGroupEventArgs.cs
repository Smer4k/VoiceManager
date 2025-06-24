namespace VoiceManager.Features.EventArgs;

public class ChangingCurrentGroupEventArgs
{
	public ChangingCurrentGroupEventArgs(ChatMember chatMember, GroupChat oldValue, GroupChat newValue)
	{
		ChatMember = chatMember;
		OldValue = oldValue;
		NewValue = newValue;
	}
	
	public ChatMember ChatMember { get; }
	public GroupChat OldValue { get; }
	public GroupChat NewValue { get; }

	public bool IsAllowed { get; set; } = true;
}