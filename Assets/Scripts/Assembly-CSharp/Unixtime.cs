using System;
using UnityEngine;

public class Unixtime : MonoBehaviour
{
	private void Start()
	{
		ResetTime();
	}

	public static void ResetTime()
	{
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		long startTimestamp = (long)(DateTime.UtcNow - dateTime).TotalSeconds;
		DiscordController.presence.startTimestamp = startTimestamp;
	}
}
