using System.Collections.Generic;
using System.Text.RegularExpressions;
using Exiled.API.Interfaces;

namespace VoiceManager;

public class Translation : ITranslation
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
	public string HintGroup { get; set; } = "<align=\"left\">Current group: {groupName}\nMute: {muted}\nGroup chat: {enabled}</align>";
	public string HintProximity { get; set; } = "<align=\"left\">Proximity chat: {proximity}</align>";
	public string HintGroupMembers { get; set; } = "<align=\"left\">{icon}{nickname:12}|{role}</align>";
}

public static class TranslationParser
{
	public static string ParseTemplate(string template, Dictionary<string, string> values)
	{
		var pattern = new Regex(@"\{(\w+)(?::(\d+))?\}");

		return pattern.Replace(template, m =>
		{
			var key = m.Groups[1].Value;
			var maxLenGroup = m.Groups[2];
			if (!values.TryGetValue(key, out var val))
				return m.Value;

			if (maxLenGroup.Success && int.TryParse(maxLenGroup.Value, out var maxLen))
			{
				if (val.Length > maxLen)
					return val.Substring(0, maxLen) + "…";
			}

			return val;
		});
	}
}