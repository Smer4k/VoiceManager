using LabApi.Features.Wrappers;
using VoiceManager.Features.MonoBehaviours;

namespace VoiceManager.Features;

public static class Extensions
{
	public static ChatMember GetChatMember(this ReferenceHub hub)
	{
		return hub.GetComponent<ChatMember>();
	}
	
	public static ChatMember GetChatMember(this Player player)
	{
		return player.ReferenceHub.GetComponent<ChatMember>();
	}
}