using PlayerRoles;
using UserSettings.ServerSpecific;
using UserSettings.ServerSpecific.Examples;
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
		ServerSpecificSettingsSync.DefinedSettings =
		[
			new SSGroupHeader(translation.ServerSpecificSettingHeading),
			new SSKeybindSetting((int?)BindId.ProximityChat, translation.ServerSpecificProximity,
				hint: translation.ServerSpecificProximityHint),
			new SSKeybindSetting((int?)BindId.GroupChat, translation.ServerSpecificGroup,
				hint: translation.ServerSpecificGroupHint),
			new SSKeybindSetting((int?)BindId.NextGroupChat, translation.ServerSpecificNextGroup,
				hint: translation.ServerSpecificNextGroupHint),
			new SSKeybindSetting((int?)BindId.MuteUnmute, translation.ServerSpecificMuteUnmute,
				hint: translation.ServerSpecificMuteUnmuteHint),
			new SSTwoButtonsSetting((int?)BindId.ActivationMode, translation.ServerSpecificActivationMode,
				translation.ServerSpecificActivationModeA,
				translation.ServerSpecificActivationModeB, hint: translation.ServerSpecificActivationModeHint)
		];
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
					OnEnableProximityChat(member, !member.ProximityChatEnabled);
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
					OnEnableGroupChat(member, !member.GroupChatEnabled);
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
		member.SetProximityChatEnabled(state);
		if (member.GroupChatEnabled)
		{
			member.SetGroupChatEnabled(false);
		}
	}

	public static void OnEnableGroupChat(ChatMember member, bool state)
	{
		member.SetGroupChatEnabled(state);
		if (member.ProximityChatEnabled)
		{
			member.SetProximityChatEnabled(false);
		}
	}
}