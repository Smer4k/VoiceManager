using PlayerRoles;
using UserSettings.ServerSpecific;
using VoiceManager.Features;

namespace VoiceManager.SpecificSettings;

public class SSChatController
{
	public string Name { get; } = "Group chat";

	public void Activate()
	{
		ServerSpecificSettingsSync.DefinedSettings = new ServerSpecificSettingBase[6]
		{
			new SSGroupHeader(Name),
			new SSKeybindSetting(0, "Enable SCP proximity chat",
				hint: "Enables proximity chat so people can hear you."),
			new SSKeybindSetting(1, "Enable group chat"),
			new SSKeybindSetting(2, "Switch next group chat"),
			new SSKeybindSetting(3, "Mute/Unmute group chat"),
			new SSTwoButtonsSetting(4, "Activation Mode", "Toggle", "Hold"),
		};
		ServerSpecificSettingsSync.SendToAll();
		ServerSpecificSettingsSync.ServerOnSettingValueReceived += ProcessUserInput;
	}

	public void Deactivate()
	{
		ServerSpecificSettingsSync.ServerOnSettingValueReceived -= ProcessUserInput;
	}

	private void ProcessUserInput(ReferenceHub sender, ServerSpecificSettingBase setting)
	{
		if (!sender.IsAlive()) return;

		var member = ChatMember.Get(sender);
		
		switch (setting.SettingId)
		{
			case 0:
				if (setting is not SSKeybindSetting ssKeybindSetting1)
					break;
				if (ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(sender, 4).SyncIsA)
				{
					if (!ssKeybindSetting1.SyncIsPressed)
						break;
					OnEnableProximityChat(member, !member.ProximityChatEnabled);
					break;
				}

				OnEnableProximityChat(member, ssKeybindSetting1.SyncIsPressed);
				break;
			case 1:
				if (setting is not SSKeybindSetting ssKeybindSetting2)
					break;
				if (ServerSpecificSettingsSync.GetSettingOfUser<SSTwoButtonsSetting>(sender, 4).SyncIsA)
				{
					if (!ssKeybindSetting2.SyncIsPressed)
						break;
					OnEnableGroupChat(member, !member.GroupChatEnabled);
					break;
				}

				OnEnableGroupChat(member, ssKeybindSetting2.SyncIsPressed);
				break;
			case 2:
				if (setting is not SSKeybindSetting ssKeybindSetting3)
					break;
				if (!ssKeybindSetting3.SyncIsPressed)
					break;
				member.SelectNextGroup();
				break;
			case 3:
				if (setting is not SSKeybindSetting ssKeybindSetting4)
					break;
				if (!ssKeybindSetting4.SyncIsPressed)
					break;
				member.SetMuteCurrentGroup(!member.IsGroupMuted(member.CurrentGroup));
				break;
			case 4:
				OnEnableGroupChat(member, false);
				OnEnableProximityChat(member, false);
				break;
		}
	}
	
	private static void OnEnableProximityChat(ChatMember member, bool state)
	{
		if (!member.ProximityChat)
		{
			return;
		}

		member.SetProximityChatEnabled(state);
		if (member.GroupChatEnabled)
		{
			member.SetGroupChatEnabled(false);
		}
	}

	private static void OnEnableGroupChat(ChatMember member, bool state)
	{
		if (member.Groups.Count < 1)
		{
			return;
		}

		member.SetGroupChatEnabled(state);
		if (member.ProximityChatEnabled)
		{
			member.SetProximityChatEnabled(false);
		}
	}
}