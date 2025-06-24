namespace VoiceManager.Features.EventArgs;

public class RemovedChatMemberEventArgs
{
	public RemovedChatMemberEventArgs(ReferenceHub hub)
	{
		Hub = hub;
	}
	
	public ReferenceHub Hub { get; }
}