using System.Collections.Generic;
using UserSettings.ServerSpecific;
using VoiceManager.Features;

namespace VoiceManager.SpecificSettings;

public class SSChatController
{
	public enum BindId
	{
		ProximityChat,
		GroupChat,
		NextGroupChat,
		MuteUnmute,
		ActivationMode,
	}

	public void Activate()
	{
		var translation = VoiceEntry.Instance.Translation;
		
		List<ServerSpecificSettingBase> settings = new(6);
		if (ServerSpecificSettingsSync.DefinedSettings != null)
			settings.AddRange(ServerSpecificSettingsSync.DefinedSettings);
		
		settings.Add(new SSGroupHeader(translation.ServerSpecificSettingHeading));
		settings.Add(new SSKeybindSetting((int?)BindId.ProximityChat, translation.ServerSpecificProximity,
			hint: translation.ServerSpecificProximityHint));
		settings.Add(new SSKeybindSetting((int?)BindId.GroupChat, translation.ServerSpecificGroup,
			hint: translation.ServerSpecificGroupHint));
		settings.Add(new SSKeybindSetting((int?)BindId.NextGroupChat, translation.ServerSpecificNextGroup,
			hint: translation.ServerSpecificNextGroupHint));
		settings.Add(new SSKeybindSetting((int?)BindId.MuteUnmute, translation.ServerSpecificMuteUnmute,
			hint: translation.ServerSpecificMuteUnmuteHint));
		settings.Add(new SSTwoButtonsSetting((int?)BindId.ActivationMode, translation.ServerSpecificActivationMode,
			translation.ServerSpecificActivationModeA,
			translation.ServerSpecificActivationModeB, hint: translation.ServerSpecificActivationModeHint));
		
		ServerSpecificSettingsSync.DefinedSettings = settings.ToArray();
		ServerSpecificSettingsSync.SendToAll();
		ServerSpecificSettingsSync.ServerOnSettingValueReceived += ProcessUserInput;
	}

	public void Deactivate()
	{
		ServerSpecificSettingsSync.ServerOnSettingValueReceived -= ProcessUserInput;
	}

	private void ProcessUserInput(ReferenceHub sender, ServerSpecificSettingBase setting)
	{
		if (!ChatMember.TryGet(sender, out ChatMember member)) return;

		switch (setting.SettingId)
		{
			case (int)BindId.ProximityChat:
				if (setting is not SSKeybindSetting ssKeybindSetting1)
					break;
				if (ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(sender, (int)BindId.ActivationMode)
				    .SyncIsA)
				{
					if (!ssKeybindSetting1.SyncIsPressed)
						break;
					OnEnableProximityChat(member, !member.ProximityChat);
					break;
				}

				OnEnableProximityChat(member, ssKeybindSetting1.SyncIsPressed);
				break;
			case (int)BindId.GroupChat:
				if (setting is not SSKeybindSetting ssKeybindSetting2)
					break;
				if (ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(sender, (int)BindId.ActivationMode)
				    .SyncIsA)
				{
					if (!ssKeybindSetting2.SyncIsPressed)
						break;
					OnEnableGroupChat(member, !member.GroupChat);
					break;
				}

				OnEnableGroupChat(member, ssKeybindSetting2.SyncIsPressed);
				break;
			case (int)BindId.NextGroupChat:
				if (setting is not SSKeybindSetting ssKeybindSetting3)
					break;
				if (!ssKeybindSetting3.SyncIsPressed)
					break;
				member.SelectNextGroup();
				break;
			case (int)BindId.MuteUnmute:
				if (setting is not SSKeybindSetting ssKeybindSetting4)
					break;
				if (!ssKeybindSetting4.SyncIsPressed)
					break;
				member.SetMuteCurrentGroup(!member.IsGroupMuted(member.CurrentGroup));
				break;
			case (int)BindId.ActivationMode:
				OnEnableGroupChat(member, false);
				OnEnableProximityChat(member, false);
				break;
		}
	}

	public static void OnEnableProximityChat(ChatMember member, bool state)
	{
		member.SetProximityChat(state);
		if (member.GroupChat)
		{
			member.SetGroupChat(false);
		}
	}

	public static void OnEnableGroupChat(ChatMember member, bool state)
	{
		member.SetGroupChat(state);
		if (member.ProximityChat)
		{
			member.SetProximityChat(false);
		}
	}
}