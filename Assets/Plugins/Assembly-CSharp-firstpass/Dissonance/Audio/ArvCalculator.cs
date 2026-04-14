using System;

namespace Dissonance.Audio
{
	internal struct ArvCalculator
	{
		public float ARV { get; private set; }

		public void Reset()
		{
			ARV = 0f;
		}

		public void Update(ArraySegment<float> samples)
		{
			if (samples.Array == null)
			{
				throw new ArgumentNullException("samples");
			}
			float num = 0f;
			for (int i = 0; i < samples.Count; i++)
			{
				num += Math.Abs(samples.Array[samples.Offset + i]);
			}
			ARV = num / (float)samples.Count;
		}
	}
}
