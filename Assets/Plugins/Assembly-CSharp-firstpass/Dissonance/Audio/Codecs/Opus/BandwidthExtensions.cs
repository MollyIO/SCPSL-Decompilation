using System;

namespace Dissonance.Audio.Codecs.Opus
{
	internal static class BandwidthExtensions
	{
		private static readonly Log Log = Logs.Create(LogCategory.Core, typeof(BandwidthExtensions).Name);

		public static int SampleRate(this OpusNative.Bandwidth bandwidth)
		{
			switch (bandwidth)
			{
			case OpusNative.Bandwidth.Narrowband:
				return 8000;
			case OpusNative.Bandwidth.Mediumband:
				return 12000;
			case OpusNative.Bandwidth.Wideband:
				return 16000;
			case OpusNative.Bandwidth.SuperWideband:
				return 24000;
			case OpusNative.Bandwidth.Fullband:
				return 48000;
			default:
				throw new ArgumentOutOfRangeException("bandwidth", Log.PossibleBugMessage(string.Format("{0} is not a valid value", bandwidth), "B534C9B2-6A9B-455E-875E-A01D93B278C8"));
			}
		}
	}
}
