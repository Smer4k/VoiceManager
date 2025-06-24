namespace VoiceManager.Features.EventArgs;

public class ChangingHasProximityChatEventArgs
{
	public ChangingHasProximityChatEventArgs(ChatMember chatMember, bool oldValue, bool newValue)
	{
		ChatMember = chatMember;
		OldValue = oldValue;
		NewValue = newValue;
	}
	public ChatMember ChatMember { get; }
	public bool OldValue { get; }
	public bool NewValue { get; }
	public bool IsAllowed { get; set; } = true;
}