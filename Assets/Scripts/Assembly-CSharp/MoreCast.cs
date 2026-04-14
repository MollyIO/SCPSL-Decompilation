using System.Collections.Generic;
using UnityEngine;

public class MoreCast : MonoBehaviour
{
	public static bool BeamCast(Vector3 start, Vector3 end, Vector3 beamRadius, float beamStep, out List<RaycastHit> hitInfo, int layerMask, bool any)
	{
		hitInfo = new List<RaycastHit>();
		Vector3 start2 = start;
		Vector3 end2 = end;
		start2 -= beamRadius;
		end2 -= beamRadius;
		for (float num = 0f - beamRadius.x; num < beamRadius.x; num += beamStep)
		{
			start2.y = start.y;
			end2.y = end.y;
			start2.x += beamStep;
			end2.x += beamStep;
			for (float num2 = 0f - beamRadius.y; num2 < beamRadius.x; num2 += beamStep)
			{
				start2.z = start.z;
				end2.z = end.z;
				start2.y += beamStep;
				end2.y += beamStep;
				for (float num3 = 0f - beamRadius.x; num3 < beamRadius.x; num3 += beamStep)
				{
					start2.z += beamStep;
					end2.z += beamStep;
					RaycastHit hitInfo2;
					bool flag = Physics.Linecast(start2, end2, out hitInfo2, layerMask);
					hitInfo.Add(hitInfo2);
					if (any && flag)
					{
						return true;
					}
					if (!flag && !any)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public static bool BeamCast(Vector3 start, Vector3 end, Vector3 beamRadius, float beamStep, int layerMask, bool any)
	{
		List<RaycastHit> hitInfo;
		return BeamCast(start, end, beamRadius, beamStep, out hitInfo, layerMask, any);
	}
}
