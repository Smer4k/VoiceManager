using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CommandSystem;
using NorthwoodLib.Pools;

namespace VoiceManager.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class GroupChatParent : ParentCommand
{
	public override string Command { get; } = "groupchat";
	public override string[] Aliases { get; } = ["gc"];
	public override string Description { get; } = "Commands for control group chats";
	
	public override void LoadGeneratedCommands()
	{
		RegisterCommand(new SetActive());
		RegisterCommand(new Add());
		RegisterCommand(new Create());
		RegisterCommand(new Remove());
		RegisterCommand(new List());
		RegisterCommand(new Delete());
	}

	protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
	{
		StringBuilder sb = StringBuilderPool.Shared.Rent();
		sb.AppendLine();
		sb.Append("Please enter a valid subcommand:");

		foreach (ICommand command in AllCommands)
		{
			sb.Append($"\n\n<color=yellow><b>- {command.Command} ({string.Join(", ", command.Aliases)})</b></color>\n<color=white>{command.Description}</color>");
		}
		
		response = StringBuilderPool.Shared.ToStringReturn(sb);
		return false;
	}
}