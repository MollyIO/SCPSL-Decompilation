using UnityEngine;

namespace Dissonance.Datastructures
{
	internal class WindowDeviationCalculator : BaseWindowCalculator<float>
	{
		private float _sum;

		private float _sumOfSquares;

		public float StdDev { get; private set; }

		public float Mean { get; private set; }

		public float Confidence
		{
			get
			{
				return (float)base.Count / (float)base.Capacity;
			}
		}

		public WindowDeviationCalculator(uint size)
			: base(size)
		{
		}

		protected override void Updated(float? removed, float added)
		{
			if (removed.HasValue)
			{
				_sum -= removed.Value;
				_sumOfSquares -= removed.Value * removed.Value;
			}
			_sum += added;
			_sumOfSquares += added * added;
			StdDev = CalculateDeviation(_sum / (float)base.Count, _sumOfSquares / (float)base.Count);
			Mean = _sum / (float)base.Count;
		}

		private float CalculateDeviation(float mean, float meanOfSquares)
		{
			if (base.Count <= 1)
			{
				return 0f;
			}
			float b = meanOfSquares - mean * mean;
			return Mathf.Sqrt(Mathf.Max(0f, b));
		}

		public override void Clear()
		{
			_sum = 0f;
			_sumOfSquares = 0f;
			StdDev = 0f;
			base.Clear();
		}
	}
}
