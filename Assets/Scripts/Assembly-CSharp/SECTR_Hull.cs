using System;
using UnityEngine;

public abstract class SECTR_Hull : MonoBehaviour
{
	private Mesh previousMesh;

	private Vector3[] vertsCW;

	private Vector3 meshCentroid = Vector3.zero;

	protected Vector3 meshNormal = Vector3.forward;

	[SECTR_ToolTip("Convex, planar mesh that defines the portal shape.")]
	public Mesh HullMesh;

	public Vector3[] VertsCW
	{
		get
		{
			ComputeVerts();
			return vertsCW;
		}
	}

	public Vector3 Normal
	{
		get
		{
			ComputeVerts();
			return base.transform.rotation * meshNormal;
		}
	}

	public Vector3 ReverseNormal
	{
		get
		{
			ComputeVerts();
			return base.transform.rotation * -meshNormal;
		}
	}

	public Vector3 Center
	{
		get
		{
			ComputeVerts();
			return base.transform.localToWorldMatrix.MultiplyPoint3x4(meshCentroid);
		}
	}

	public Plane HullPlane
	{
		get
		{
			ComputeVerts();
			return new Plane(Normal, Center);
		}
	}

	public Plane ReverseHullPlane
	{
		get
		{
			ComputeVerts();
			return new Plane(ReverseNormal, Center);
		}
	}

	public Bounds BoundingBox
	{
		get
		{
			Bounds result = new Bounds(base.transform.position, Vector3.zero);
			if ((bool)HullMesh)
			{
				ComputeVerts();
				if (vertsCW != null)
				{
					Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
					int num = vertsCW.Length;
					for (int i = 0; i < num; i++)
					{
						result.Encapsulate(localToWorldMatrix.MultiplyPoint3x4(vertsCW[i]));
					}
				}
			}
			return result;
		}
	}

	public bool IsPointInHull(Vector3 p, float distanceTolerance)
	{
		ComputeVerts();
		Vector3 vector = base.transform.worldToLocalMatrix.MultiplyPoint3x4(p);
		Vector3 vector2 = vector - Vector3.Dot(vector - meshCentroid, meshNormal) * meshNormal;
		if (vertsCW != null && Vector3.SqrMagnitude(vector - vector2) < distanceTolerance * distanceTolerance)
		{
			float num = (float)Math.PI * 2f;
			int num2 = vertsCW.Length;
			for (int i = 0; i < num2; i++)
			{
				Vector3 lhs = vertsCW[i] - vector2;
				Vector3 rhs = vertsCW[(i + 1) % num2] - vector2;
				float magnitude = lhs.magnitude;
				float magnitude2 = rhs.magnitude;
				float num3 = magnitude * magnitude2;
				if (num3 < 0.001f)
				{
					return true;
				}
				float f = Vector3.Dot(lhs, rhs) / num3;
				num -= Mathf.Acos(f);
			}
			return Mathf.Abs(num) < 0.001f;
		}
		return false;
	}

	protected void ComputeVerts()
	{
		if (!(HullMesh != previousMesh))
		{
			return;
		}
		if ((bool)HullMesh)
		{
			int vertexCount = HullMesh.vertexCount;
			vertsCW = new Vector3[vertexCount];
			meshCentroid = Vector3.zero;
			for (int i = 0; i < vertexCount; i++)
			{
				Vector3 vector = HullMesh.vertices[i];
				vertsCW[i] = vector;
				meshCentroid += vector;
			}
			meshCentroid /= (float)HullMesh.vertexCount;
			meshNormal = Vector3.zero;
			int num = HullMesh.normals.Length;
			for (int j = 0; j < num; j++)
			{
				meshNormal += HullMesh.normals[j];
			}
			meshNormal /= (float)HullMesh.normals.Length;
			meshNormal.Normalize();
			bool flag = true;
			for (int k = 0; k < vertexCount; k++)
			{
				Vector3 vector2 = vertsCW[k];
				Vector3 vector3 = vector2 - Vector3.Dot(vector2 - meshCentroid, meshNormal) * meshNormal;
				flag = flag && Vector3.SqrMagnitude(vector2 - vector3) < 0.001f;
				vertsCW[k] = vector3;
			}
			if (!flag)
			{
				Debug.LogWarning("Occluder mesh of " + base.name + " is not planar!");
			}
			Array.Sort(vertsCW, (Vector3 a, Vector3 b) => SECTR_Geometry.CompareVectorsCW(a, b, meshCentroid, meshNormal) * -1);
			if (!SECTR_Geometry.IsPolygonConvex(vertsCW))
			{
				Debug.LogWarning("Occluder mesh of " + base.name + " is not convex!");
			}
		}
		else
		{
			meshNormal = Vector3.zero;
			meshCentroid = Vector3.zero;
			vertsCW = null;
		}
		previousMesh = HullMesh;
	}
}
