using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using VoiceManager.Features.EventArgs;

namespace VoiceManager.Features;

public static class Settings
{
	private static Dictionary<string, object> Storage { get; } = new();

	public static void InitDefault()
	{
		Storage.Clear();
		var config = VoiceEntry.Instance.Config;
		Storage.Add(nameof(config.Use3DProximityChat), config.Use3DProximityChat);
		Storage.Add(nameof(config.Volume3DProximityChat), config.Volume3DProximityChat);
		Storage.Add(nameof(config.MinProximityDistance), config.MinProximityDistance);
		Storage.Add(nameof(config.MaxProximityDistance), config.MaxProximityDistance);
		Storage.Add(nameof(config.DisplayGroupMembers), config.DisplayGroupMembers);
	}

	public static bool TrySetSetting(string key, string value)
	{
		if (!Storage.TryGetValue(key, out var oldValue)) return false;

		if (oldValue == null)
			return false;
		
		var targetType = oldValue.GetType();

		try
		{
			var converter = TypeDescriptor.GetConverter(targetType);
			if (!converter.CanConvertFrom(typeof(string)))
				return false;

			var newValue = converter.ConvertFromString(null, CultureInfo.InvariantCulture, value);

			ChangingSettingEventArgs ev = Events.OnChangingSetting(key, value);
			if (!ev.IsAllowed)
				return false;
			
			Storage[key] = newValue;
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool TrySetSetting(string key, object value)
	{
		if (!Storage.TryGetValue(key, out var oldValue)) return false;
		
		if (oldValue == null || value == null) return false;
		
		var targetType = oldValue.GetType();
		
		if (!targetType.IsInstanceOfType(value)) return false;

		ChangingSettingEventArgs ev = Events.OnChangingSetting(key, value);
		if (!ev.IsAllowed)
			return false;
		
		Storage[key] = value;
		return true;
	}

	public static bool TryGetSetting(string key, out object value)
	{
		if (!Storage.TryGetValue(key, out var oldValue))
		{
			value = null;
			return false;
		}
		value = oldValue;
		return true;
	}

	public static bool TryGetSetting<T>(string key, out T value)
	{
		value = default!;
		if (!Storage.TryGetValue(key, out var raw)) return false;

		if (raw is not T casted) return false;
		
		value = casted;
		return true;
	}

	public new static string ToString()
	{
		var sb = new StringBuilder(Storage.Count);
		foreach (var pair in Storage)
		{
			sb.AppendLine($"- {pair.Key}: {pair.Value}");
		}
		return sb.ToString();
	}
}