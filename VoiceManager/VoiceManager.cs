global using Log = LabApi.Features.Console.Logger;
using System;
using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Loader.Features.Plugins;
using VoiceManager.SpecificSettings;
using RueI;
using VoiceManager.Features;


namespace VoiceManager;

public class VoiceManager : Plugin<Config>
{
	public override string Name { get; } = "VoiceManager";
	public override string Description { get; } = "SCPs can talk to people!";
	public override string Author { get; } = "Smer4k";
	public override Version Version { get; } = new Version(1, 0, 0);
	public override Version RequiredApiVersion { get; } = LabApiProperties.CurrentVersion;
	public static VoiceManager Singleton;
	public static bool RueiEnabled;
	private static EventHandler _eventHandler = new();
	private static SSChatController _ssChatController;
	public static Config VConfig => Singleton.Config;

	public override void Enable()
	{
		if (!Config.IsEnabled)
			return;

		Singleton = this;
		CustomHandlersManager.RegisterEventsHandler(_eventHandler);
		Settings.InitDefault();
		
		try
		{
			RueIMain.EnsureInit();
			RueiEnabled = true;
		}
		catch (Exception e)
		{
			RueiEnabled = false;
		}

		if (!Config.UseDefaultServerSpecific) return;
		_ssChatController = new SSChatController();
		_ssChatController.Activate();
	}

	public override void Disable()
	{
		CustomHandlersManager.UnregisterEventsHandler(_eventHandler);
		_eventHandler = null;
		Singleton = null;

		if (!Config.UseDefaultServerSpecific) return;
		_ssChatController.Deactivate();
		_ssChatController = null;
	}
}