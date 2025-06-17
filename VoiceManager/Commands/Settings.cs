using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;

namespace VoiceManager.Commands;

[CommandHandler(typeof(GroupChatParent))]
public class Settings : ICommand, IUsageProvider
{
	public string Command { get; } = "settings";
	public string[] Aliases { get; } = ["set"];
	public string Description { get; } = "Changes some plugin settings.";
	public string[] Usage { get; } = ["parameter", "value"];
	
	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
	{
		if (arguments.Count < 2)
		{
			var usage = string.Join(" ", Array.ConvertAll(Usage, u => $"[{u}]"));
			response = $"Usage: {GroupChatParent.CommandName} {Command} {usage}\n" +
			           Features.Settings.ToString();
			return false;
		}

		if (Features.Settings.TrySetSetting(arguments.At(0), arguments.At(1)))
		{
			response = $"Successfully set plugin setting.";
			return true;
		}
		
		response = $"Failed to set plugin setting.";
		return false;
	}
}