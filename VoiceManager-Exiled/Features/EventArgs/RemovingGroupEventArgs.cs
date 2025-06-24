namespace VoiceManager.Features.EventArgs;

public class RemovingGroupEventArgs
{
	public RemovingGroupEventArgs(GroupChat groupChat)
	{
		GroupChat = groupChat;
	}
	
	public GroupChat GroupChat { get; }
	public bool IsAllowed { get; set; } = true;
}