using System.Collections.Generic;
using Exiled.API.Features;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;

namespace VoiceManager.Features;

public class OpusHandler
{
	private static readonly Dictionary<ReferenceHub, OpusHandler> Handlers = new();
	
	public OpusDecoder Decoder { get; } = new();
	public OpusEncoder Encoder { get; } = new(OpusApplicationType.Voip);

	public static OpusHandler Get(ReferenceHub hub)
	{
		if (Handlers.TryGetValue(hub, out OpusHandler handler))
			return handler;

		handler = new OpusHandler();
		Handlers.Add(hub, handler);
		return handler;
	}

	public static void Remove(ReferenceHub hub)
	{
		if (Handlers.TryGetValue(hub, out OpusHandler handler))
		{
			handler.Decoder.Dispose();
			handler.Encoder.Dispose();
			
			Handlers.Remove(hub);
		}
	}
	
	public static void Remove(Player player) => Remove(player?.ReferenceHub);
}