namespace VoiceManager.Features.EventArgs;

public class CreatingGroupEventArgs
{
	public CreatingGroupEventArgs(int groupId, string groupName)
	{
		GroupId = groupId;
		GroupName = groupName;
	}
	
	public string GroupName { get; }
	public int GroupId { get; }
	public bool IsAllowed { get; set; } = true;
}