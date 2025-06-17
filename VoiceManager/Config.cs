using System.Collections.Generic;
using System.ComponentModel;
using PlayerRoles;

namespace VoiceManager;

public class Config
{
	public bool IsEnabled { get; set; } = true;
	public bool Debug { get; set; } = false;
	public bool Use3DProximityChat { get; set; } = true;
	public float Volume3DProximityChat { get; set; } = 3f;
	public float MinProximityDistance { get; set; } = 2f;
	public float MaxProximityDistance { get; set; } = 10f;
	
	[Description("Automatically add players with allowed role")]
	public bool AutoInitProximityChatRoles { get; set; } = false;

	[Description("Roles that will have access to Proximity Chat")]
	public HashSet<RoleTypeId> ProximityChatRoles { get; set; } =
	[
		RoleTypeId.Scp049,
		RoleTypeId.Scp096,
		RoleTypeId.Scp106,
		RoleTypeId.Scp173,
		RoleTypeId.Scp0492,
		RoleTypeId.Scp939,
		RoleTypeId.Scp3114,
	];
	
	[Description("Use default plugin hints when enabling/disabling chats? Disable if you have your own hint implementation.")]
	public bool UseDefaultHints { get; set; } = true;

	[Description("Use default Server-Specific in plugin? Disable if you have your own Server-Specific for this plugin.")]
	public bool UseDefaultServerSpecific { get; set; } = true;
	public bool SendBroadcastOnRoleChange { get; set; } = false;
	public ushort BroadcastDuration { get; set; } = 5;
	public string BroadcastMessage { get; set; } = "<b>You can enable Proximity Chat, set the key to <color=red>Server-specific</color></b>.";
}