using System;
using System.Collections;
using System.Collections.Generic;
using LabApi.Features.Wrappers;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using Respawning.Objectives;
using RueI.Displays;
using RueI.Elements;
using RueI.Extensions.HintBuilding;
using UnityEngine;
using Display = RueI.Displays.Display;

namespace VoiceManager.Features.MonoBehaviours;

public class ChatMember : MonoBehaviour
{
	public HashSet<uint> CloseHubsNetIds { get; } = new(32);
	public ReferenceHub Hub { get; private set; }
	public IFpcRole FpcRole { get; private set; }
	public List<GroupChat> Groups { get; } = new();
	public GroupChat CurrentGroup { get; private set; }
	public Display Display { get; private set; }
	public bool GroupChatEnabled { get; private set; }
	public bool CanUseProximityChat { get; private set; }
	public bool ProximityChatEnabled { get; private set; }
	public bool TempCanUseProximityChat { get; private set; }
	private readonly HashSet<GroupChat> _mutedGroups = new();
	private readonly Collider[] _buffer = new Collider[33];
	private Coroutine _checkerCloseHubsCoroutine;

	public static ChatMember Init(ReferenceHub hub)
	{
		if (hub.TryGetComponent<ChatMember>(out var chatMember))
			return chatMember;

		var member = hub.gameObject.AddComponent<ChatMember>(); 
		if (VoiceManager.Singleton.RueiEnabled && VoiceManager.Singleton.Config.UseDefaultHints)
		{
			var core = DisplayCore.Get(hub);
			member.Display = new Display(core);
			member.Display.Elements.Add(new SetElement(75, ""));
		}
		member.Hub = hub;
		member.OnPlayerChangedRole(hub.roleManager.CurrentRole);
		return member;
	}

	public bool AddGroup(GroupChat group)
	{
		if (Groups.Contains(group))
			return false;
		Groups.Add(group);
		if (CurrentGroup == null)
		{
			SetCurrentGroup(group);
		}

		return true;
	}

	public bool RemoveGroup(GroupChat group)
	{
		if (!Groups.Remove(group)) return false;
		if (Groups.Count > 0)
		{
			if (CurrentGroup.Equals(group))
				SetCurrentGroup(Groups[0]);
		}
		else
		{
			SetCurrentGroup(null);
		}

		return true;
	}

	public void RemoveAllGroups()
	{
		foreach (var group in Groups)
		{
			group.TryRemoveMember(this);
		}
		
		Groups.Clear();
	}

	public bool SetMuteCurrentGroup(bool mute)
	{
		if (mute)
		{
			if (!_mutedGroups.Add(CurrentGroup)) return false;
			UpdateDisplay();
			return true;
		}

		if (!_mutedGroups.Remove(CurrentGroup)) return false;
		UpdateDisplay();
		return true;
	}

	public bool SetMuteGroup(GroupChat groupChat, bool mute)
	{
		foreach (var group in Groups)
		{
			if (!group.Equals(groupChat)) continue;
			var result = mute ? _mutedGroups.Add(group) : _mutedGroups.Remove(group);
			UpdateDisplay();
			return result;
		}

		return false;
	}

	public bool SetMuteGroup(int groupId, bool mute)
	{
		foreach (var group in Groups)
		{
			if (group.Id != groupId) continue;
			var result = mute ? _mutedGroups.Add(group) : _mutedGroups.Remove(group);
			UpdateDisplay();
			return result;
		}

		return false;
	}

	public bool IsGroupMuted(GroupChat group)
	{
		return _mutedGroups.Contains(group);
	}

	public bool IsGroupMuted(int groupId)
	{
		return IsGroupMuted(new GroupChat(groupId, ""));
	}

	public void SelectNextGroup()
	{
		if (Groups.Count == 0)
			return;

		if (CurrentGroup == null)
			return;

		var index = Groups.IndexOf(CurrentGroup);

		if (index == -1 || index == Groups.Count - 1)
		{
			SetCurrentGroup(Groups[0]);
		}
		else
		{
			SetCurrentGroup(Groups[index + 1]);
		}

		UpdateDisplay();
	}

	public void SetCurrentGroup(GroupChat group)
	{
		if (CurrentGroup != null && CurrentGroup.Equals(group))
			return;

		CurrentGroup = group;
		UpdateDisplay();
	}

	public void SetGroupChatEnabled(bool value)
	{
		if (Groups.Count < 1)
			return;
		GroupChatEnabled = value;
		UpdateDisplay();
	}

	public void SetProximityChatEnabled(bool value)
	{
		if (!CanUseProximityChat)
			return;

		ProximityChatEnabled = value;
		UpdateDisplay();
	}

	public void SetCanUseProximityChat(bool state, bool isTemp = false)
	{
		if (CanUseProximityChat == state) return;
		CanUseProximityChat = state;
		TempCanUseProximityChat = isTemp;
		
		if (_checkerCloseHubsCoroutine != null)
			StopCoroutine(_checkerCloseHubsCoroutine);

		if (state)
			_checkerCloseHubsCoroutine = StartCoroutine(CheckCloseHubs());

		if (!state)
		{
			//CloseHubs.Clear();
			CloseHubsNetIds.Clear();
			ProximityChatEnabled = false;
		}

		UpdateDisplay();
	}

	public void OnPlayerChangedRole(PlayerRoleBase playerRole)
	{
		SetProximityChatEnabled(false);
		SetGroupChatEnabled(false);
		if (playerRole is not IFpcRole roleBase)
		{
			return;
		}
		var config = VoiceManager.Singleton.Config;

		if (config.AutoInitProximityChatRoles && config.ProximityChatRoles.Contains(playerRole.RoleTypeId))
		{
			SetCanUseProximityChat(true);
			if (config.SendBroadcastOnRoleChange)
				Player.Get(Hub)?.SendBroadcast(config.BroadcastMessage, config.BroadcastDuration);
		}

		FpcRole = roleBase;
		UpdateDisplay();
	}

	private IEnumerator CheckCloseHubs()
	{
		while (true)
		{
			yield return new WaitForSeconds(1f);
			try
			{
				var fpc = FpcRole.FpcModule.transform;
				var size = Physics.OverlapSphereNonAlloc(fpc.position, VoiceManager.Singleton.Config.MaxProximityDistance,
					_buffer, 4);
				
				// 4; 12; 20; 28; Water; RenderAfterFog; Grenade; Skybox
				if (VoiceManager.Singleton.Config.Debug)
					Log.Debug($"Size: {size}");
				
				for (var i = 0; i < _buffer.Length; i++)
				{
					if (_buffer[i] is null) continue;
					var hub = _buffer[i].GetComponent<ReferenceHub>();
					if (hub == Hub) continue;
					
					var dist = Vector3.Distance(fpc.position, hub.GetPosition());
					if (dist > VoiceManager.Singleton.Config.MaxProximityDistance)
					{
						if (CloseHubsNetIds.Remove(hub.netId))
						{
							if (VoiceManager.Singleton.Config.Debug)
								Log.Debug($"{hub.GetNickname()} is {dist} units away — too far. Removing from close hubs.");
							_buffer[i] = null;
						}

						continue;
					}

					if (CloseHubsNetIds.Add(hub.netId))
					{
						if (VoiceManager.Singleton.Config.Debug)
							Log.Debug($"Player {hub.GetNickname()} added to close hubs.");	
					}
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}

	private void UpdateDisplay()
	{
		if (Display == null) return;
		if (!CanUseProximityChat && Groups.Count < 1)
		{
			((SetElement)Display.Elements[0]).Content = "";
			Display.Update();
			return;
		}
		
		var pos = 0;
		var sb = StringBuilderPool.Shared.Rent();
		sb.SetSize(25);
		sb.SetLineHeight(25);
		sb.SetAlignment(HintBuilding.AlignStyle.Left);
		sb.SetMargins(60);
		if (CurrentGroup != null && Groups.Count > 0)
		{
			sb.AppendLine($"Current group: {CurrentGroup.Name}");
			var muted =
				$"Muted: {(IsGroupMuted(CurrentGroup) ? "<color=green>True</color>" : "<color=red>False</color>")}";
			sb.AppendLine(muted);
			var color = "red";
			if (GroupChatEnabled) color = "green";
			var groupChat = $"Group chat: <color={color}>{(GroupChatEnabled ? "Enabled" : "Disabled")}</color>";
			sb.AppendLine(groupChat);
			pos += 50;
		}

		if (CanUseProximityChat)
		{
			var color = "red";
			if (ProximityChatEnabled) color = "green";
			sb.AppendLine($"Proximity chat: <color={color}>{(ProximityChatEnabled ? "Enabled" : "Disabled")}</color>");
			pos += 25;
		}

		sb.CloseLineHeight();
		sb.CloseSize();
		sb.CloseAlign();
		var elem = (SetElement)Display.Elements[0];
		elem.Content = StringBuilderPool.Shared.ToStringReturn(sb);
		elem.Position = pos;
		Display.Update();
	}

	public void OnDisable()
	{
		SetCanUseProximityChat(false);
		RemoveAllGroups();
	}

	public override string ToString()
	{
		return $"{Hub.GetNickname()} | {Hub.GetRoleId()}";
	}
}