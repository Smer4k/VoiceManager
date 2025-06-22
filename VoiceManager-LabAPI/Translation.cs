namespace VoiceManager;

public class Translation
{
	public string ServerSpecificSettingHeading { get; set; } = "Voice Settings";
	public string ServerSpecificProximity { get; set; } = "Enable SCP proximity chat";
	public string ServerSpecificProximityHint { get; set; } = "When you enable proximity chat, you can communicate with people while being an SCP";
	public string ServerSpecificGroup { get; set; } = "Enable group chat";
	public string ServerSpecificGroupHint { get; set; } = "When you enable group chat, only members of the current group will hear you";
	public string ServerSpecificNextGroup { get; set; } = "Switch next group chat";
	public string ServerSpecificNextGroupHint { get; set; } = "Allows you to switch to the next group";
	public string ServerSpecificMuteUnmute { get; set; } = "Mute/Unmute group chat";
	public string ServerSpecificMuteUnmuteHint { get; set; } = "Allows you to mute or unmute the current group";
	public string ServerSpecificActivationMode { get; set; } = "Activation Mode";
	public string ServerSpecificActivationModeHint { get; set; } = "Determines how group and proximity chat is activated";
	public string ServerSpecificActivationModeA { get; set; } = "Toggle";
	public string ServerSpecificActivationModeB { get; set; } = "Hold";
	public string BroadcastMessage { get; set; } = "<b>You can enable Proximity Chat, set the key to <color=red>Server-specific</color></b>.";
	public string HintGroup { get; set; } = "Current group: {groupName}\nMuted: {muted}\nGroup chat: {enabled}";
	public string HintProximity { get; set; } = "Proximity chat: {proximity}";
}