using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using LabApi.Events.Handlers;
using VoiceManager.SpecificSettings;
using VoiceManager.Features;


namespace VoiceManager;

public class VoiceEntry : Plugin<Config, Translation>
{
	public override string Name { get; } = "VoiceManager";
	public override string Author { get; } = "Smer4k";
	public override Version Version { get; } = new Version(1, 0, 3);
	public override string Prefix => "VoiceManager";
	public override PluginPriority Priority => PluginPriority.High;
	public static VoiceEntry Instance;
	private EventHandler _eventHandler;
	private SSChatController _ssChatController;

	public override void OnEnabled()
	{
		Instance = this;
		Settings.InitDefault();
		RegisterEvents();
		_ssChatController = new SSChatController();
		_ssChatController.Activate();
		base.OnEnabled();
	}

	public override void OnDisabled()
	{
		_eventHandler = null;
		Instance = null;
		UnregisterEvents();
		_ssChatController.Deactivate();
		_ssChatController = null;
		base.OnDisabled();
	}

	private void RegisterEvents()
	{
		_eventHandler = new();
		Exiled.Events.Handlers.Server.RestartingRound += _eventHandler.OnServerRoundRestarted;
		Exiled.Events.Handlers.Player.Left += _eventHandler.OnPlayerLeft;
		Exiled.Events.Handlers.Player.Died += _eventHandler.OnPlayerDeath;
		Exiled.Events.Handlers.Player.VoiceChatting += _eventHandler.OnPlayerSendingVoiceMessage;
		PlayerEvents.ChangedRole += _eventHandler.OnPlayerChangedRole;
	}

	private void UnregisterEvents()
	{
		Exiled.Events.Handlers.Server.RestartingRound -= _eventHandler.OnServerRoundRestarted;
		Exiled.Events.Handlers.Player.Left -= _eventHandler.OnPlayerLeft;
		Exiled.Events.Handlers.Player.Died -= _eventHandler.OnPlayerDeath;
		Exiled.Events.Handlers.Player.VoiceChatting -= _eventHandler.OnPlayerSendingVoiceMessage;
		PlayerEvents.ChangedRole -= _eventHandler.OnPlayerChangedRole;
		_eventHandler = null;
	}
}