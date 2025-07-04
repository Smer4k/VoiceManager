using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;
using PlayerRoles;
using UnityEngine;

namespace VoiceManager;

public class Config : IConfig
{
	public bool IsEnabled { get; set; } = true;
	public bool Debug { get; set; } = false;
	public bool Use3DProximityChat { get; set; } = true;
	public float Volume3DProximityChat { get; set; } = 3f;
	public float MinProximityDistance { get; set; } = 2f;
	public float MaxProximityDistance { get; set; } = 10f;

	[Description("Whether to display the list of members in the group on the screen")]
	public bool DisplayGroupMembers { get; set; } = false;

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
	[Description("Will the plugin automatically create hints by default?")]
	public bool AutoDefaultHints { get; set; } = true;
	[Description("Coordinates where the hint will be")]
	public Vector2 HintPosition { get; set; } = new Vector2(55, 1070);
	[Description("Text size hint")]
	public int HintSize { get; set; } = 25;
	[Description("Coordinates where the hint will be")]
	public Vector2 HintPositionGroupMembers { get; set; } = new Vector2(-350, 800);
	[Description("Text size hint")]
	public int HintSizeGroupMembers { get; set; } = 21;
	public bool SendBroadcastOnRoleChange { get; set; } = false;
	public ushort BroadcastDuration { get; set; } = 5;
}