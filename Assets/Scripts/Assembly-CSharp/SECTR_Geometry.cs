using System;
using System.Collections.Generic;
using UnityEngine;

public static class SECTR_Geometry
{
	public const float kVERTEX_EPSILON = 0.001f;

	public const float kBOUNDS_CHEAT = 0.01f;

	public static Bounds ComputeBounds(Light light)
	{
		Bounds result;
		if ((bool)light)
		{
			switch (light.type)
			{
			case LightType.Spot:
			{
				Vector3 position = light.transform.position;
				result = new Bounds(position, Vector3.zero);
				Vector3 up = light.transform.up;
				Vector3 right = light.transform.right;
				Vector3 vector = position + light.transform.forward * light.range;
				float num2 = Mathf.Tan(light.spotAngle * 0.5f * ((float)Math.PI / 180f)) * light.range;
				result.Encapsulate(vector);
				Vector3 vector2 = vector + up * num2;
				Vector3 vector3 = vector + up * (0f - num2);
				Vector3 vector4 = right * num2;
				Vector3 vector5 = right * (0f - num2);
				result.Encapsulate(vector2 + vector4);
				result.Encapsulate(vector2 + vector5);
				result.Encapsulate(vector3 + vector4);
				result.Encapsulate(vector3 + vector5);
				break;
			}
			case LightType.Point:
			{
				float num = light.range * 2f;
				result = new Bounds(light.transform.position, new Vector3(num, num, num));
				break;
			}
			default:
				result = new Bounds(light.transform.position, new Vector3(0.01f, 0.01f, 0.01f));
				break;
			}
		}
		else
		{
			result = new Bounds(light.transform.position, new Vector3(0.01f, 0.01f, 0.01f));
		}
		return result;
	}

	public static Bounds ComputeBounds(Terrain terrain)
	{
		if ((bool)terrain)
		{
			Vector3 size = ((!(terrain.terrainData != null)) ? Vector3.zero : terrain.terrainData.size);
			Vector3 position = terrain.transform.position;
			Vector3 center = new Vector3(position.x + size.x * 0.5f, position.y + size.y * 0.5f, position.z + size.z * 0.5f);
			return new Bounds(center, size);
		}
		return default(Bounds);
	}

	public static bool FrustumIntersectsBounds(Bounds bounds, List<Plane> frustum, int inMask, out int outMask)
	{
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		outMask = 0;
		int num = 0;
		for (int i = 1; i <= inMask; i += i)
		{
			if ((i & inMask) != 0)
			{
				Plane plane = frustum[num];
				float num2 = center.x * plane.normal.x + center.y * plane.normal.y + center.z * plane.normal.z + plane.distance;
				float num3 = extents.x * Mathf.Abs(plane.normal.x) + extents.y * Mathf.Abs(plane.normal.y) + extents.z * Mathf.Abs(plane.normal.z);
				if (num2 + num3 < 0f)
				{
					return false;
				}
				outMask |= i;
			}
			num++;
		}
		return true;
	}

	public static bool FrustumContainsBounds(Bounds bounds, List<Plane> frustum)
	{
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		int count = frustum.Count;
		for (int i = 0; i < count; i++)
		{
			Plane plane = frustum[i];
			float num = center.x * plane.normal.x + center.y * plane.normal.y + center.z * plane.normal.z + plane.distance;
			float num2 = extents.x * Mathf.Abs(plane.normal.x) + extents.y * Mathf.Abs(plane.normal.y) + extents.z * Mathf.Abs(plane.normal.z);
			if (num + num2 < 0f || num - num2 < 0f)
			{
				return false;
			}
		}
		return true;
	}

	public static bool BoundsContainsBounds(Bounds container, Bounds contained)
	{
		return container.Contains(contained.min) && container.Contains(contained.max);
	}

	public static bool BoundsIntersectsSphere(Bounds bounds, Vector3 sphereCenter, float sphereRadius)
	{
		Vector3 vector = Vector3.Min(Vector3.Max(sphereCenter, bounds.min), bounds.max);
		float num = Vector3.SqrMagnitude(vector - sphereCenter);
		return num <= sphereRadius * sphereRadius;
	}

	public static Bounds ProjectBounds(Bounds bounds, Vector3 projection)
	{
		Vector3 point = bounds.min + projection;
		Vector3 point2 = bounds.max + projection;
		bounds.Encapsulate(point);
		bounds.Encapsulate(point2);
		return bounds;
	}

	public static bool IsPointInFrontOfPlane(Vector3 position, Vector3 center, Vector3 normal)
	{
		Vector3 normalized = (position - center).normalized;
		return Vector3.Dot(normal, normalized) > 0f;
	}

	public static bool IsPolygonConvex(Vector3[] verts)
	{
		int num = verts.Length;
		if (num < 3)
		{
			return false;
		}
		float num2 = (float)(num - 2) * (float)Math.PI;
		for (int i = 0; i < num; i++)
		{
			Vector3 vector = verts[i];
			Vector3 vector2 = verts[(i + 1) % num];
			Vector3 vector3 = verts[(i + 2) % num];
			Vector3 lhs = vector - vector2;
			lhs.Normalize();
			Vector3 rhs = vector3 - vector2;
			rhs.Normalize();
			num2 -= Mathf.Acos(Vector3.Dot(lhs, rhs));
		}
		return Mathf.Abs(num2) < 0.001f;
	}

	public static int CompareVectorsCW(Vector3 a, Vector3 b, Vector3 centroid, Vector3 normal)
	{
		Vector3 rhs = Vector3.Cross(a - centroid, b - centroid);
		float magnitude = rhs.magnitude;
		if (magnitude > 0.001f)
		{
			rhs /= magnitude;
			return (Vector3.Dot(normal, rhs) > 0f) ? 1 : (-1);
		}
		return 0;
	}
}
