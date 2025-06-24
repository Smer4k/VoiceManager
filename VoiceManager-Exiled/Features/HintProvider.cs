using System.Collections.Generic;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;

namespace VoiceManager.Features;

public static class HintProvider
{
	public static Dictionary<ReferenceHub, DynamicHint> Hints { get; } = new();
	
	public static void CreateHint(ReferenceHub hub)
	{
		if (Hints.TryGetValue(hub, out var hint)) 
			return;
		
		hint = new DynamicHint()
		{
			AutoText = arg => ChatMember.GetHintText(arg.PlayerDisplay.ReferenceHub),
			SyncSpeed = HintSyncSpeed.Fast,
			TargetY = VoiceEntry.Instance.Config.HintPosition.y,
			TargetX = VoiceEntry.Instance.Config.HintPosition.x,
			FontSize = VoiceEntry.Instance.Config.HintSize,
		};
		Hints.Add(hub, hint);
		PlayerDisplay.Get(hub).AddHint(hint);
	}
	
	public static void RemoveHint(ReferenceHub hub)
	{
		if (!Hints.TryGetValue(hub, out var hint)) return;
		PlayerDisplay.Get(hub).RemoveHint(hint);
		Hints.Remove(hub);
	}
}