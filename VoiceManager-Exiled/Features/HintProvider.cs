using System.Collections.Generic;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;

namespace VoiceManager.Features;

public static class HintProvider
{
	public static Dictionary<ReferenceHub, List<AbstractHint>> Hints { get; } = new();

	public static bool TryGet(ReferenceHub referenceHub, out List<AbstractHint> hints)
	{
		return Hints.TryGetValue(referenceHub, out hints);
	}

	public static void AddHint(ReferenceHub hub, AbstractHint hint)
	{
		if (hint == null || hub == null) return;
		
		if (!Hints.TryGetValue(hub, out var hints))
		{
			hints = new();
			Hints.Add(hub, hints);
		}

		hints.Add(hint);
		PlayerDisplay.Get(hub).AddHint(hint);
	}

	public static void CreateDefaultHints(ReferenceHub hub)
	{
		if (Hints.TryGetValue(hub, out _))
			return;
		var hintInfo = new DynamicHint()
		{
			AutoText = arg => ChatMember.GetHintInfoText(arg.PlayerDisplay.ReferenceHub),
			SyncSpeed = HintSyncSpeed.Fast,
			TargetY = VoiceEntry.Instance.Config.HintPosition.y,
			TargetX = VoiceEntry.Instance.Config.HintPosition.x,
			FontSize = VoiceEntry.Instance.Config.HintSize,
		};
		
		Settings.TryGetSetting(nameof(VoiceEntry.Instance.Config.DisplayGroupMembers), out bool displayGroupMembers);
		if (displayGroupMembers)
		{
			var hintList = new DynamicHint()
			{
				AutoText = arg => ChatMember.GetHintGroupMembersText(arg.PlayerDisplay.ReferenceHub),
				SyncSpeed = HintSyncSpeed.Fast,
				TargetX = VoiceEntry.Instance.Config.HintPositionGroupMembers.x,
				TargetY = VoiceEntry.Instance.Config.HintPositionGroupMembers.y,
				FontSize = VoiceEntry.Instance.Config.HintSizeGroupMembers,
			};
			AddHint(hub, hintList);
		}
		
		AddHint(hub, hintInfo);
	}

	public static void RemoveHint(ReferenceHub hub, AbstractHint hint)
	{
		if (Hints.TryGetValue(hub, out var hints) && hints.Remove(hint))
		{
			PlayerDisplay.Get(hub).RemoveHint(hint);
		}
	}

	public static void RemoveHints(ReferenceHub hub)
	{
		if (!Hints.TryGetValue(hub, out var hints)) return;
		var display = PlayerDisplay.Get(hub);
		foreach (var hint in hints)
		{
			display.RemoveHint(hint);
		}

		Hints.Remove(hub);
	}
}