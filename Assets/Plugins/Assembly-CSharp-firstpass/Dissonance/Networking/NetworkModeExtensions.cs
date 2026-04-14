using System;

namespace Dissonance.Networking
{
	public static class NetworkModeExtensions
	{
		public static bool IsServerEnabled(this NetworkMode mode)
		{
			switch (mode)
			{
			case NetworkMode.Host:
			case NetworkMode.DedicatedServer:
				return true;
			case NetworkMode.None:
			case NetworkMode.Client:
				return false;
			default:
				throw new ArgumentOutOfRangeException("mode", mode, null);
			}
		}

		public static bool IsClientEnabled(this NetworkMode mode)
		{
			switch (mode)
			{
			case NetworkMode.Host:
			case NetworkMode.Client:
				return true;
			case NetworkMode.None:
			case NetworkMode.DedicatedServer:
				return false;
			default:
				throw new ArgumentOutOfRangeException("mode", mode, null);
			}
		}
	}
}
