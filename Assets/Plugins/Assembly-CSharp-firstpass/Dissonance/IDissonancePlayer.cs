using UnityEngine;

namespace Dissonance
{
	public interface IDissonancePlayer
	{
		string PlayerId { get; }

		Vector3 Position { get; }

		Quaternion Rotation { get; }

		NetworkPlayerType Type { get; }

		bool IsTracking { get; }
	}
}
