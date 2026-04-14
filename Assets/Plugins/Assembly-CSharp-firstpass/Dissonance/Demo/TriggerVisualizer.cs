using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Dissonance.Demo
{
	public class TriggerVisualizer : MonoBehaviour
	{
		private GameObject _visualisations;

		private BaseCommsTrigger[] _triggers;

		private Material _fillMaterial;

		private Material _outlineMaterial;

		private float _alpha;

		public Color Color;

		private void Awake()
		{
			_visualisations = new GameObject("Trigger Visualisations");
			_visualisations.transform.parent = base.gameObject.transform;
			_visualisations.transform.localPosition = Vector3.zero;
			_visualisations.transform.localRotation = Quaternion.identity;
			_fillMaterial = UnityEngine.Object.Instantiate(Resources.Load<Material>("TriggerMaterial"));
			_outlineMaterial = UnityEngine.Object.Instantiate(Resources.Load<Material>("TriggerEdgeMaterial"));
			_triggers = GetComponents<BaseCommsTrigger>();
			SphereCollider[] components = GetComponents<SphereCollider>();
			SphereCollider[] array = components;
			foreach (SphereCollider sphere in array)
			{
				CreateCircle(sphere);
			}
			BoxCollider[] components2 = GetComponents<BoxCollider>();
			BoxCollider[] array2 = components2;
			foreach (BoxCollider box in array2)
			{
				CreateBox(box);
			}
		}

		private void Update()
		{
			if (_triggers.Any((BaseCommsTrigger baseCommsTrigger) => baseCommsTrigger.CanTrigger))
			{
				_visualisations.SetActive(true);
				_alpha = ((!_triggers.Any((BaseCommsTrigger baseCommsTrigger) => baseCommsTrigger.IsColliderTriggered)) ? Mathf.Clamp01(_alpha - Time.deltaTime * 4f) : Mathf.Clamp01(_alpha + Time.deltaTime * 4f));
				float t = Mathf.Lerp(0.7f, 1f, _alpha);
				Color value = Color.Lerp(default(Color), Color, t);
				_fillMaterial.SetColor("_TintColor", value);
				_outlineMaterial.color = Color;
			}
			else
			{
				_visualisations.SetActive(false);
				_alpha = 1f;
			}
		}

		private void CreateCircle(SphereCollider sphere)
		{
			GameObject gameObject = new GameObject("sphere collider");
			gameObject.transform.parent = _visualisations.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			Mesh mesh = new Mesh();
			List<Vector3> list = new List<Vector3>();
			list.Add(Vector3.zero);
			List<Vector3> list2 = list;
			for (int i = 0; i < 64; i++)
			{
				Vector3 item = new Vector3(sphere.radius * Mathf.Sin((float)Math.PI * 2f * (float)i / 64f), 0.1f, sphere.radius * Mathf.Cos((float)Math.PI * 2f * (float)i / 64f));
				list2.Add(item);
			}
			List<Vector3> list3 = new List<Vector3>();
			for (int j = 0; j < list2.Count; j++)
			{
				list3.Add(Vector3.up);
			}
			List<Color> list4 = new List<Color>();
			for (int k = 0; k < list2.Count; k++)
			{
				list4.Add(new Color(1f, 1f, 1f, 0.2f));
			}
			List<int> list5 = new List<int>();
			for (int l = 0; l < 64; l++)
			{
				list5.Add(0);
				list5.Add(l);
				if (l < 63)
				{
					list5.Add(l + 1);
				}
				else
				{
					list5.Add(1);
				}
			}
			List<int> list6 = new List<int>();
			for (int m = 1; m < 64; m++)
			{
				list6.Add(m);
			}
			list6.Add(1);
			mesh.vertices = list2.ToArray();
			mesh.normals = list3.ToArray();
			mesh.colors = list4.ToArray();
			mesh.subMeshCount = 2;
			mesh.SetIndices(list5.ToArray(), MeshTopology.Triangles, 0);
			mesh.SetIndices(list6.ToArray(), MeshTopology.LineStrip, 1);
			meshFilter.mesh = mesh;
			meshRenderer.materials = new Material[2] { _fillMaterial, _outlineMaterial };
			meshRenderer.receiveShadows = false;
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		}

		private void CreateBox([NotNull] BoxCollider box)
		{
			GameObject gameObject = new GameObject("box collider");
			gameObject.transform.parent = _visualisations.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			Mesh mesh = new Mesh();
			Vector3 vector = box.center - box.size * 0.5f;
			Vector3 vector2 = box.center + box.size * 0.5f;
			List<Vector3> list = new List<Vector3>();
			list.Add(new Vector3(vector.x, 0.1f, vector.z));
			list.Add(new Vector3(vector.x, 0.1f, vector2.z));
			list.Add(new Vector3(vector2.x, 0.1f, vector2.z));
			list.Add(new Vector3(vector2.x, 0.1f, vector.z));
			List<Vector3> list2 = list;
			List<Vector3> list3 = new List<Vector3>();
			for (int i = 0; i < list2.Count; i++)
			{
				list3.Add(Vector3.up);
			}
			List<Color> list4 = new List<Color>();
			for (int j = 0; j < list2.Count; j++)
			{
				list4.Add(new Color(1f, 1f, 1f, 0.2f));
			}
			List<int> list5 = new List<int>();
			list5.Add(0);
			list5.Add(1);
			list5.Add(2);
			list5.Add(2);
			list5.Add(3);
			list5.Add(0);
			List<int> list6 = list5;
			list5 = new List<int>();
			list5.Add(0);
			list5.Add(1);
			list5.Add(2);
			list5.Add(3);
			list5.Add(0);
			List<int> list7 = list5;
			mesh.vertices = list2.ToArray();
			mesh.normals = list3.ToArray();
			mesh.colors = list4.ToArray();
			mesh.subMeshCount = 2;
			mesh.SetIndices(list6.ToArray(), MeshTopology.Triangles, 0);
			mesh.SetIndices(list7.ToArray(), MeshTopology.LineStrip, 1);
			meshFilter.mesh = mesh;
			meshRenderer.materials = new Material[2] { _fillMaterial, _outlineMaterial };
			meshRenderer.receiveShadows = false;
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		}
	}
}
