namespace VoiceManager.Features.EventArgs;

public class CreatingChatMemberEventArgs
{
	public CreatingChatMemberEventArgs(ReferenceHub hub)
	{
		Hub = hub;
	}
	
	public ReferenceHub Hub { get; }
	public bool IsAllowed { get; set; } = true;
}