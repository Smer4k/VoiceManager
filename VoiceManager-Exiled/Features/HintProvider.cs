using System.Collections.Generic;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;

namespace VoiceManager.Features;

public static class HintProvider
{
	public static IHintProvider Provider { get; } = GetProvider();

	private static IHintProvider GetProvider()
	{
		return new ServiceMeowHintProvider();
	}
}

public interface IHintProvider
{
	public void CreateHint(ReferenceHub hub);
	public void RemoveHint(ReferenceHub hub);
}

public class ServiceMeowHintProvider : IHintProvider
{
	public Dictionary<ReferenceHub, DynamicHint> Hints { get; } = new();
	
	public void CreateHint(ReferenceHub hub)
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
	
	public void RemoveHint(ReferenceHub hub)
	{
		if (!Hints.TryGetValue(hub, out var hint)) return;
		PlayerDisplay.Get(hub).RemoveHint(hint);
		Hints.Remove(hub);
	}
}
