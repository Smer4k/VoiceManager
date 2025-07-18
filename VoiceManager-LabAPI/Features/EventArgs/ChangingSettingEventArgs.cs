namespace VoiceManager.Features.EventArgs;

public class ChangingSettingEventArgs
{
	public ChangingSettingEventArgs(string settingName, object value)
	{
		SettingName = settingName;
		Value = value;
	}
	public string SettingName { get; }
	public object Value { get; }
	public bool IsAllowed  { get; set; } = true;
}