global using Log = LabApi.Features.Console.Logger;
using System;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using VoiceManager.SpecificSettings;
using VoiceManager.Features;


namespace VoiceManager;

public class VoiceEntry : Plugin
{
	public override string Name { get; } = "VoiceManager";
	public override string Description { get; } = "Allows you to create group chats or give Proximity Chat to Scp.";
	public override string Author { get; } = "Smer4k";
	public override Version Version { get; } = new Version(1, 0, 3);
	public override Version RequiredApiVersion { get; } = LabApiProperties.CurrentVersion;
	public override LoadPriority Priority => LoadPriority.High;
	public static VoiceEntry Instance;
	private static EventHandler _eventHandler = new();
	private static SSChatController _ssChatController;
	public Config Config { get; private set; }
	public Translation Translation { get; private set; }

	public override void Enable()
	{
		if (!Config.IsEnabled)
			return;

		Instance = this;
		CustomHandlersManager.RegisterEventsHandler(_eventHandler);
		Settings.InitDefault();
		
		_ssChatController = new SSChatController();
		_ssChatController.Activate();
	}

	public override void LoadConfigs()
	{
		this.TryLoadConfig("config.yml", out Config config);
		Config = config ?? new Config();
		this.TryLoadConfig("translation.yml", out Translation translation);
		Translation = translation ?? new Translation();
	}

	public override void Disable()
	{
		CustomHandlersManager.UnregisterEventsHandler(_eventHandler);
		_eventHandler = null;
		Instance = null;
		_ssChatController.Deactivate();
		_ssChatController = null;
	}
}