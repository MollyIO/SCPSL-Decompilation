using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
[AddComponentMenu("SECTR/Vis/SECTR Culling Camera")]
public class SECTR_CullingCamera : MonoBehaviour
{
	private struct VisibilityNode
	{
		public SECTR_Sector sector;

		public SECTR_Portal portal;

		public List<Plane> frustumPlanes;

		public bool forwardTraversal;

		public VisibilityNode(SECTR_CullingCamera cullingCamera, SECTR_Sector sector, SECTR_Portal portal, Plane[] frustumPlanes, bool forwardTraversal)
		{
			this.sector = sector;
			this.portal = portal;
			if (frustumPlanes == null)
			{
				this.frustumPlanes = null;
			}
			else if (cullingCamera.frustumPool.Count > 0)
			{
				this.frustumPlanes = cullingCamera.frustumPool.Pop();
				this.frustumPlanes.AddRange(frustumPlanes);
			}
			else
			{
				this.frustumPlanes = new List<Plane>(frustumPlanes);
			}
			this.forwardTraversal = forwardTraversal;
		}

		public VisibilityNode(SECTR_CullingCamera cullingCamera, SECTR_Sector sector, SECTR_Portal portal, List<Plane> frustumPlanes, bool forwardTraversal)
		{
			this.sector = sector;
			this.portal = portal;
			if (frustumPlanes == null)
			{
				this.frustumPlanes = null;
			}
			else if (cullingCamera.frustumPool.Count > 0)
			{
				this.frustumPlanes = cullingCamera.frustumPool.Pop();
				this.frustumPlanes.AddRange(frustumPlanes);
			}
			else
			{
				this.frustumPlanes = new List<Plane>(frustumPlanes);
			}
			this.forwardTraversal = forwardTraversal;
		}
	}

	private struct ClipVertex
	{
		public Vector4 vertex;

		public float side;

		public ClipVertex(Vector4 vertex, float side)
		{
			this.vertex = vertex;
			this.side = side;
		}
	}

	private struct ThreadCullData
	{
		public enum CullingModes
		{
			None = 0,
			Graph = 1,
			Shadow = 2
		}

		public SECTR_Sector sector;

		public Vector3 cameraPos;

		public List<Plane> cullingPlanes;

		public List<List<Plane>> occluderFrustums;

		public int baseMask;

		public float shadowDistance;

		public bool cullingSimpleCulling;

		public List<SECTR_Member.Child> sectorShadowLights;

		public CullingModes cullingMode;

		public ThreadCullData(SECTR_Sector sector, SECTR_CullingCamera cullingCamera, Vector3 cameraPos, List<Plane> cullingPlanes, List<List<Plane>> occluderFrustums, int baseMask, float shadowDistance, bool cullingSimpleCulling)
		{
			this.sector = sector;
			this.cameraPos = cameraPos;
			this.baseMask = baseMask;
			this.shadowDistance = shadowDistance;
			this.cullingSimpleCulling = cullingSimpleCulling;
			sectorShadowLights = null;
			lock (cullingCamera.threadOccluderPool)
			{
				this.occluderFrustums = ((cullingCamera.threadOccluderPool.Count <= 0) ? new List<List<Plane>>(occluderFrustums.Count) : cullingCamera.threadOccluderPool.Pop());
			}
			lock (cullingCamera.threadFrustumPool)
			{
				if (cullingCamera.threadFrustumPool.Count > 0)
				{
					this.cullingPlanes = cullingCamera.threadFrustumPool.Pop();
					this.cullingPlanes.AddRange(cullingPlanes);
				}
				else
				{
					this.cullingPlanes = new List<Plane>(cullingPlanes);
				}
				int count = occluderFrustums.Count;
				for (int i = 0; i < count; i++)
				{
					List<Plane> list = null;
					if (cullingCamera.threadFrustumPool.Count > 0)
					{
						list = cullingCamera.threadFrustumPool.Pop();
						list.AddRange(occluderFrustums[i]);
					}
					else
					{
						list = new List<Plane>(occluderFrustums[i]);
					}
					this.occluderFrustums.Add(list);
				}
			}
			cullingMode = CullingModes.Graph;
		}

		public ThreadCullData(SECTR_Sector sector, Vector3 cameraPos, List<SECTR_Member.Child> sectorShadowLights)
		{
			this.sector = sector;
			this.cameraPos = cameraPos;
			cullingPlanes = null;
			occluderFrustums = null;
			baseMask = 0;
			shadowDistance = 0f;
			cullingSimpleCulling = false;
			this.sectorShadowLights = sectorShadowLights;
			cullingMode = CullingModes.Shadow;
		}
	}

	private Camera myCamera;

	private SECTR_Member cullingMember;

	private Dictionary<int, SECTR_Member.Child> hiddenRenderers = new Dictionary<int, SECTR_Member.Child>(16);

	private Dictionary<int, SECTR_Member.Child> hiddenLights = new Dictionary<int, SECTR_Member.Child>(16);

	private Dictionary<int, SECTR_Member.Child> hiddenTerrains = new Dictionary<int, SECTR_Member.Child>(2);

	private int renderersCulled;

	private int lightsCulled;

	private int terrainsCulled;

	private bool didCull;

	private bool runOnce;

	private List<SECTR_Sector> initialSectors = new List<SECTR_Sector>(4);

	private Stack<VisibilityNode> nodeStack = new Stack<VisibilityNode>(10);

	private List<ClipVertex> portalVertices = new List<ClipVertex>(16);

	private List<Plane> newFrustum = new List<Plane>(16);

	private List<Plane> cullingPlanes = new List<Plane>(16);

	private List<List<Plane>> occluderFrustums = new List<List<Plane>>(10);

	private Dictionary<SECTR_Occluder, SECTR_Occluder> activeOccluders = new Dictionary<SECTR_Occluder, SECTR_Occluder>(10);

	private List<ClipVertex> occluderVerts = new List<ClipVertex>(10);

	private Dictionary<SECTR_Member.Child, int> shadowLights = new Dictionary<SECTR_Member.Child, int>(10);

	private List<SECTR_Sector> shadowSectors = new List<SECTR_Sector>(4);

	private Dictionary<SECTR_Sector, List<SECTR_Member.Child>> shadowSectorTable = new Dictionary<SECTR_Sector, List<SECTR_Member.Child>>(4);

	private Dictionary<int, SECTR_Member.Child> visibleRenderers = new Dictionary<int, SECTR_Member.Child>(1024);

	private Dictionary<int, SECTR_Member.Child> visibleLights = new Dictionary<int, SECTR_Member.Child>(256);

	private Dictionary<int, SECTR_Member.Child> visibleTerrains = new Dictionary<int, SECTR_Member.Child>(32);

	private Stack<List<Plane>> frustumPool = new Stack<List<Plane>>(32);

	private Stack<List<SECTR_Member.Child>> shadowLightPool = new Stack<List<SECTR_Member.Child>>(32);

	private Stack<Dictionary<int, SECTR_Member.Child>> threadVisibleListPool = new Stack<Dictionary<int, SECTR_Member.Child>>(4);

	private Stack<Dictionary<SECTR_Member.Child, int>> threadShadowLightPool = new Stack<Dictionary<SECTR_Member.Child, int>>(32);

	private Stack<List<Plane>> threadFrustumPool = new Stack<List<Plane>>(32);

	private Stack<List<List<Plane>>> threadOccluderPool = new Stack<List<List<Plane>>>(32);

	private List<Thread> workerThreads = new List<Thread>();

	private Queue<ThreadCullData> cullingWorkQueue = new Queue<ThreadCullData>(32);

	private int remainingThreadWork;

	private static List<SECTR_CullingCamera> allCullingCameras = new List<SECTR_CullingCamera>(4);

	[SECTR_ToolTip("Allows multiple culling cameras to be active at once, but at the cost of some performance.")]
	public bool MultiCameraCulling = true;

	[SECTR_ToolTip("Forces culling into a mode designed for 2D and iso games where the camera is always outside the scene.")]
	public bool SimpleCulling;

	[SECTR_ToolTip("Distance to draw clipped frustums.", 0f, 100f)]
	public float GizmoDistance = 10f;

	[SECTR_ToolTip("Material to use to render the debug frustum mesh.")]
	public Material GizmoMaterial;

	[SECTR_ToolTip("Makes the Editor camera display the Game view's culling while playing in editor.")]
	public bool CullInEditor;

	[SECTR_ToolTip("Set to false to disable shadow culling post pass.", true)]
	public bool CullShadows = true;

	[SECTR_ToolTip("Use another camera for culling properties.", true)]
	public Camera cullingProxy;

	[SECTR_ToolTip("Number of worker threads for culling. Do not set this too high or you may see hitching.", 0f, -1f)]
	public int NumWorkerThreads;

	public static List<SECTR_CullingCamera> All
	{
		get
		{
			return allCullingCameras;
		}
	}

	public int RenderersCulled
	{
		get
		{
			return renderersCulled;
		}
	}

	public int LightsCulled
	{
		get
		{
			return lightsCulled;
		}
	}

	public int TerrainsCulled
	{
		get
		{
			return terrainsCulled;
		}
	}

	public void ResetStats()
	{
		renderersCulled = 0;
		lightsCulled = 0;
		terrainsCulled = 0;
		runOnce = false;
	}

	private void OnEnable()
	{
		myCamera = GetComponent<Camera>();
		cullingMember = GetComponent<SECTR_Member>();
		allCullingCameras.Add(this);
		runOnce = false;
		int num = Mathf.Min(NumWorkerThreads, SystemInfo.processorCount);
		for (int i = 0; i < num; i++)
		{
			Thread thread = new Thread(_CullingWorker);
			thread.IsBackground = true;
			thread.Priority = System.Threading.ThreadPriority.Highest;
			thread.Start();
			workerThreads.Add(thread);
		}
	}

	private void OnDisable()
	{
		if (!MultiCameraCulling)
		{
			_UndoCulling();
		}
		allCullingCameras.Remove(this);
		int count = workerThreads.Count;
		for (int i = 0; i < count; i++)
		{
			workerThreads[i].Abort();
		}
	}

	private void OnDestroy()
	{
	}

	private void OnPreCull()
	{
		Camera camera = ((!(cullingProxy != null)) ? myCamera : cullingProxy);
		Vector3 position = camera.transform.position;
		float num = Mathf.Max(camera.fieldOfView, camera.fieldOfView * camera.aspect) * 0.5f;
		float num2 = Mathf.Cos(num * ((float)Math.PI / 180f));
		float num3 = camera.nearClipPlane / num2 * 1.001f;
		if ((bool)cullingProxy)
		{
			SECTR_CullingCamera component = cullingProxy.GetComponent<SECTR_CullingCamera>();
			if ((bool)component)
			{
				SimpleCulling = component.SimpleCulling;
				CullShadows = component.CullShadows;
				if (MultiCameraCulling != component.MultiCameraCulling)
				{
					runOnce = false;
				}
				MultiCameraCulling = component.MultiCameraCulling;
			}
		}
		int count = SECTR_LOD.All.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_LOD.All[i].SelectLOD(camera);
		}
		int num4 = 0;
		if (!SimpleCulling)
		{
			if ((bool)cullingMember && cullingMember.enabled)
			{
				initialSectors.Clear();
				initialSectors.AddRange(cullingMember.Sectors);
			}
			else
			{
				SECTR_Sector.GetContaining(ref initialSectors, new Bounds(position, new Vector3(num3, num3, num3)));
			}
			num4 = initialSectors.Count;
			for (int j = 0; j < num4; j++)
			{
				SECTR_Sector sECTR_Sector = initialSectors[j];
				if (sECTR_Sector.IsConnectedTerrain)
				{
					SimpleCulling = true;
					break;
				}
			}
		}
		if (SimpleCulling)
		{
			initialSectors.Clear();
			initialSectors.AddRange(SECTR_Sector.All);
			num4 = initialSectors.Count;
		}
		if (!base.enabled || !camera.enabled || num4 <= 0)
		{
			return;
		}
		int count2 = workerThreads.Count;
		if (!MultiCameraCulling)
		{
			if (!runOnce)
			{
				_HideAllMembers();
				runOnce = true;
			}
			else
			{
				_ApplyCulling(false);
			}
		}
		else
		{
			_HideAllMembers();
		}
		float shadowDistance = QualitySettings.shadowDistance;
		int count3 = SECTR_Member.All.Count;
		for (int k = 0; k < count3; k++)
		{
			SECTR_Member sECTR_Member = SECTR_Member.All[k];
			if (!sECTR_Member.ShadowLight)
			{
				continue;
			}
			int count4 = sECTR_Member.ShadowLights.Count;
			for (int l = 0; l < count4; l++)
			{
				SECTR_Member.Child child = sECTR_Member.ShadowLights[l];
				if ((bool)child.light)
				{
					child.shadowLightPosition = child.light.transform.position;
					child.shadowLightRange = child.light.range;
				}
				sECTR_Member.ShadowLights[l] = child;
			}
		}
		nodeStack.Clear();
		shadowLights.Clear();
		visibleRenderers.Clear();
		visibleLights.Clear();
		visibleTerrains.Clear();
		Plane[] array = GeometryUtility.CalculateFrustumPlanes(camera);
		for (int m = 0; m < num4; m++)
		{
			SECTR_Sector sector = initialSectors[m];
			nodeStack.Push(new VisibilityNode(this, sector, null, array, true));
		}
		while (nodeStack.Count > 0)
		{
			VisibilityNode visibilityNode = nodeStack.Pop();
			if (visibilityNode.frustumPlanes != null)
			{
				cullingPlanes.Clear();
				cullingPlanes.AddRange(visibilityNode.frustumPlanes);
				int count5 = cullingPlanes.Count;
				for (int n = 0; n < count5; n++)
				{
					Plane plane = cullingPlanes[n];
					Plane plane2 = cullingPlanes[(n + 1) % cullingPlanes.Count];
					float num5 = Vector3.Dot(plane.normal, plane2.normal);
					if (num5 < -0.9f && num5 > -0.99f)
					{
						Vector3 vector = plane.normal + plane2.normal;
						Vector3 vector2 = Vector3.Cross(plane.normal, plane2.normal);
						Vector3 inNormal = vector - Vector3.Dot(vector, vector2) * vector2;
						inNormal.Normalize();
						Matrix4x4 matrix4x = default(Matrix4x4);
						matrix4x.SetRow(0, new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, 0f));
						matrix4x.SetRow(1, new Vector4(plane2.normal.x, plane2.normal.y, plane2.normal.z, 0f));
						matrix4x.SetRow(2, new Vector4(vector2.x, vector2.y, vector2.z, 0f));
						matrix4x.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
						Vector3 inPoint = matrix4x.inverse.MultiplyPoint3x4(new Vector3(0f - plane.distance, 0f - plane2.distance, 0f));
						cullingPlanes.Insert(++n, new Plane(inNormal, inPoint));
					}
				}
				count5 = cullingPlanes.Count;
				int num6 = 0;
				for (int num7 = 0; num7 < count5; num7++)
				{
					num6 |= 1 << num7;
				}
				SECTR_Sector sector2 = visibilityNode.sector;
				if (SECTR_Occluder.All.Count > 0)
				{
					List<SECTR_Occluder> occludersInSector = SECTR_Occluder.GetOccludersInSector(sector2);
					if (occludersInSector != null)
					{
						int count6 = occludersInSector.Count;
						for (int num8 = 0; num8 < count6; num8++)
						{
							SECTR_Occluder sECTR_Occluder = occludersInSector[num8];
							if (!sECTR_Occluder.HullMesh || activeOccluders.ContainsKey(sECTR_Occluder))
							{
								continue;
							}
							Matrix4x4 cullingMatrix = sECTR_Occluder.GetCullingMatrix(position);
							Vector3[] vertsCW = sECTR_Occluder.VertsCW;
							Vector3 normalized = cullingMatrix.MultiplyVector(-sECTR_Occluder.MeshNormal).normalized;
							if (vertsCW == null || SECTR_Geometry.IsPointInFrontOfPlane(position, sECTR_Occluder.Center, normalized))
							{
								continue;
							}
							int num9 = vertsCW.Length;
							occluderVerts.Clear();
							Bounds bounds = new Bounds(sECTR_Occluder.transform.position, Vector3.zero);
							for (int num10 = 0; num10 < num9; num10++)
							{
								Vector3 point = cullingMatrix.MultiplyPoint3x4(vertsCW[num10]);
								bounds.Encapsulate(point);
								occluderVerts.Add(new ClipVertex(new Vector4(point.x, point.y, point.z, 1f), 0f));
							}
							int outMask;
							if (SECTR_Geometry.FrustumIntersectsBounds(sECTR_Occluder.BoundingBox, cullingPlanes, num6, out outMask))
							{
								List<Plane> list;
								if (frustumPool.Count > 0)
								{
									list = frustumPool.Pop();
									list.Clear();
								}
								else
								{
									list = new List<Plane>(num9 + 1);
								}
								_BuildFrustumFromHull(camera, true, occluderVerts, ref list);
								list.Add(new Plane(normalized, sECTR_Occluder.Center));
								occluderFrustums.Add(list);
								activeOccluders[sECTR_Occluder] = sECTR_Occluder;
							}
						}
					}
				}
				if (count2 > 0)
				{
					lock (cullingWorkQueue)
					{
						cullingWorkQueue.Enqueue(new ThreadCullData(sector2, this, position, cullingPlanes, occluderFrustums, num6, shadowDistance, SimpleCulling));
						Monitor.Pulse(cullingWorkQueue);
					}
					Interlocked.Increment(ref remainingThreadWork);
				}
				else
				{
					_FrustumCullSector(sector2, position, cullingPlanes, occluderFrustums, num6, shadowDistance, SimpleCulling, ref visibleRenderers, ref visibleLights, ref visibleTerrains, ref shadowLights);
				}
				int num11 = ((!SimpleCulling) ? visibilityNode.sector.Portals.Count : 0);
				for (int num12 = 0; num12 < num11; num12++)
				{
					SECTR_Portal sECTR_Portal = visibilityNode.sector.Portals[num12];
					bool flag = (sECTR_Portal.Flags & SECTR_Portal.PortalFlags.PassThrough) != 0;
					if ((!sECTR_Portal.HullMesh && !flag) || (sECTR_Portal.Flags & SECTR_Portal.PortalFlags.Closed) != 0)
					{
						continue;
					}
					bool flag2 = visibilityNode.sector == sECTR_Portal.FrontSector;
					SECTR_Sector sECTR_Sector2 = ((!flag2) ? sECTR_Portal.FrontSector : sECTR_Portal.BackSector);
					bool flag3 = !sECTR_Sector2;
					if (!flag3)
					{
						flag3 = SECTR_Geometry.IsPointInFrontOfPlane(position, sECTR_Portal.Center, sECTR_Portal.Normal) != flag2;
					}
					if (!flag3 && (bool)visibilityNode.portal)
					{
						Vector3 normalized2 = (sECTR_Portal.Center - visibilityNode.portal.Center).normalized;
						Vector3 rhs = ((!visibilityNode.forwardTraversal) ? visibilityNode.portal.Normal : visibilityNode.portal.ReverseNormal);
						flag3 = Vector3.Dot(normalized2, rhs) < 0f;
					}
					if (!flag3 && !flag)
					{
						int count7 = occluderFrustums.Count;
						for (int num13 = 0; num13 < count7; num13++)
						{
							if (SECTR_Geometry.FrustumContainsBounds(sECTR_Portal.BoundingBox, occluderFrustums[num13]))
							{
								flag3 = true;
								break;
							}
						}
					}
					if (flag3)
					{
						continue;
					}
					if (!flag)
					{
						portalVertices.Clear();
						Matrix4x4 localToWorldMatrix = sECTR_Portal.transform.localToWorldMatrix;
						Vector3[] vertsCW2 = sECTR_Portal.VertsCW;
						if (vertsCW2 != null)
						{
							int num14 = vertsCW2.Length;
							for (int num15 = 0; num15 < num14; num15++)
							{
								Vector3 vector3 = localToWorldMatrix.MultiplyPoint3x4(vertsCW2[num15]);
								portalVertices.Add(new ClipVertex(new Vector4(vector3.x, vector3.y, vector3.z, 1f), 0f));
							}
						}
					}
					newFrustum.Clear();
					if (!flag && !sECTR_Portal.IsPointInHull(position, num3))
					{
						int count8 = visibilityNode.frustumPlanes.Count;
						for (int num16 = 0; num16 < count8; num16++)
						{
							Plane plane3 = visibilityNode.frustumPlanes[num16];
							Vector4 a = new Vector4(plane3.normal.x, plane3.normal.y, plane3.normal.z, plane3.distance);
							bool flag4 = true;
							bool flag5 = true;
							for (int num17 = 0; num17 < portalVertices.Count; num17++)
							{
								Vector4 vertex = portalVertices[num17].vertex;
								float num18 = Vector4.Dot(a, vertex);
								portalVertices[num17] = new ClipVertex(vertex, num18);
								flag4 = flag4 && num18 > 0f;
								flag5 = flag5 && num18 <= -0.001f;
							}
							if (flag5)
							{
								portalVertices.Clear();
								break;
							}
							if (flag4)
							{
								continue;
							}
							int num19 = portalVertices.Count;
							for (int num20 = 0; num20 < num19; num20++)
							{
								int index = (num20 + 1) % portalVertices.Count;
								float side = portalVertices[num20].side;
								float side2 = portalVertices[index].side;
								if ((side > 0f && side2 <= -0.001f) || (side2 > 0f && side <= -0.001f))
								{
									Vector4 vertex2 = portalVertices[num20].vertex;
									Vector4 vertex3 = portalVertices[index].vertex;
									float num21 = side / Vector4.Dot(a, vertex2 - vertex3);
									Vector4 vertex4 = vertex2 + num21 * (vertex3 - vertex2);
									vertex4.w = 1f;
									portalVertices.Insert(num20 + 1, new ClipVertex(vertex4, 0f));
									num19++;
								}
							}
							int num22 = 0;
							while (num22 < num19)
							{
								if (portalVertices[num22].side < -0.001f)
								{
									portalVertices.RemoveAt(num22);
									num19--;
								}
								else
								{
									num22++;
								}
							}
						}
						_BuildFrustumFromHull(camera, flag2, portalVertices, ref newFrustum);
					}
					else
					{
						newFrustum.AddRange(array);
					}
					if (newFrustum.Count > 2)
					{
						nodeStack.Push(new VisibilityNode(this, sECTR_Sector2, sECTR_Portal, newFrustum, flag2));
					}
				}
			}
			if (visibilityNode.frustumPlanes != null)
			{
				visibilityNode.frustumPlanes.Clear();
				frustumPool.Push(visibilityNode.frustumPlanes);
			}
		}
		if (count2 > 0)
		{
			while (remainingThreadWork > 0)
			{
				while (cullingWorkQueue.Count > 0)
				{
					ThreadCullData cullData = default(ThreadCullData);
					lock (cullingWorkQueue)
					{
						if (cullingWorkQueue.Count > 0)
						{
							cullData = cullingWorkQueue.Dequeue();
						}
					}
					if (cullData.cullingMode == ThreadCullData.CullingModes.Graph)
					{
						_FrustumCullSectorThread(cullData);
						Interlocked.Decrement(ref remainingThreadWork);
					}
				}
			}
			remainingThreadWork = 0;
		}
		int count9 = shadowLights.Count;
		if (count9 > 0 && CullShadows)
		{
			shadowSectorTable.Clear();
			Dictionary<SECTR_Member.Child, int>.Enumerator enumerator = shadowLights.GetEnumerator();
			while (enumerator.MoveNext())
			{
				SECTR_Member.Child key = enumerator.Current.Key;
				List<SECTR_Sector> sectors;
				if ((bool)key.member && key.member.IsSector)
				{
					shadowSectors.Clear();
					shadowSectors.Add((SECTR_Sector)key.member);
					sectors = shadowSectors;
				}
				else if ((bool)key.member && key.member.Sectors.Count > 0)
				{
					sectors = key.member.Sectors;
				}
				else
				{
					SECTR_Sector.GetContaining(ref shadowSectors, key.lightBounds);
					sectors = shadowSectors;
				}
				int count10 = sectors.Count;
				for (int num23 = 0; num23 < count10; num23++)
				{
					SECTR_Sector key2 = sectors[num23];
					List<SECTR_Member.Child> value;
					if (!shadowSectorTable.TryGetValue(key2, out value))
					{
						value = ((shadowLightPool.Count <= 0) ? new List<SECTR_Member.Child>(16) : shadowLightPool.Pop());
						shadowSectorTable[key2] = value;
					}
					value.Add(key);
				}
			}
			Dictionary<SECTR_Sector, List<SECTR_Member.Child>>.Enumerator enumerator2 = shadowSectorTable.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				SECTR_Sector key3 = enumerator2.Current.Key;
				List<SECTR_Member.Child> value2 = enumerator2.Current.Value;
				if (count2 > 0)
				{
					lock (cullingWorkQueue)
					{
						cullingWorkQueue.Enqueue(new ThreadCullData(key3, position, value2));
						Monitor.Pulse(cullingWorkQueue);
					}
					Interlocked.Increment(ref remainingThreadWork);
				}
				else
				{
					_ShadowCullSector(key3, value2, ref visibleRenderers, ref visibleTerrains);
				}
			}
			if (count2 > 0)
			{
				while (remainingThreadWork > 0)
				{
					while (cullingWorkQueue.Count > 0)
					{
						ThreadCullData cullData2 = default(ThreadCullData);
						lock (cullingWorkQueue)
						{
							if (cullingWorkQueue.Count > 0)
							{
								cullData2 = cullingWorkQueue.Dequeue();
							}
						}
						if (cullData2.cullingMode == ThreadCullData.CullingModes.Shadow)
						{
							_ShadowCullSectorThread(cullData2);
							Interlocked.Decrement(ref remainingThreadWork);
						}
					}
				}
				remainingThreadWork = 0;
			}
			enumerator2 = shadowSectorTable.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				List<SECTR_Member.Child> value3 = enumerator2.Current.Value;
				value3.Clear();
				shadowLightPool.Push(value3);
			}
		}
		_ApplyCulling(true);
		int count11 = occluderFrustums.Count;
		for (int num24 = 0; num24 < count11; num24++)
		{
			occluderFrustums[num24].Clear();
			frustumPool.Push(occluderFrustums[num24]);
		}
		occluderFrustums.Clear();
		activeOccluders.Clear();
	}

	private void OnPostRender()
	{
		if (MultiCameraCulling)
		{
			_UndoCulling();
		}
	}

	private void _CullingWorker()
	{
		while (true)
		{
			ThreadCullData cullData = default(ThreadCullData);
			lock (cullingWorkQueue)
			{
				while (cullingWorkQueue.Count == 0)
				{
					Monitor.Wait(cullingWorkQueue);
				}
				cullData = cullingWorkQueue.Dequeue();
			}
			switch (cullData.cullingMode)
			{
			case ThreadCullData.CullingModes.Graph:
				_FrustumCullSectorThread(cullData);
				break;
			case ThreadCullData.CullingModes.Shadow:
				_ShadowCullSectorThread(cullData);
				break;
			}
			if (cullData.cullingMode == ThreadCullData.CullingModes.Graph || cullData.cullingMode == ThreadCullData.CullingModes.Shadow)
			{
				Interlocked.Decrement(ref remainingThreadWork);
			}
		}
	}

	private void _FrustumCullSectorThread(ThreadCullData cullData)
	{
		Dictionary<int, SECTR_Member.Child> dictionary = null;
		Dictionary<int, SECTR_Member.Child> dictionary2 = null;
		Dictionary<int, SECTR_Member.Child> dictionary3 = null;
		Dictionary<SECTR_Member.Child, int> dictionary4 = null;
		lock (threadVisibleListPool)
		{
			dictionary = ((threadVisibleListPool.Count <= 0) ? new Dictionary<int, SECTR_Member.Child>(32) : threadVisibleListPool.Pop());
			dictionary2 = ((threadVisibleListPool.Count <= 0) ? new Dictionary<int, SECTR_Member.Child>(32) : threadVisibleListPool.Pop());
			dictionary3 = ((threadVisibleListPool.Count <= 0) ? new Dictionary<int, SECTR_Member.Child>(32) : threadVisibleListPool.Pop());
		}
		lock (threadShadowLightPool)
		{
			dictionary4 = ((threadShadowLightPool.Count <= 0) ? new Dictionary<SECTR_Member.Child, int>(32) : threadShadowLightPool.Pop());
		}
		_FrustumCullSector(cullData.sector, cullData.cameraPos, cullData.cullingPlanes, cullData.occluderFrustums, cullData.baseMask, cullData.shadowDistance, cullData.cullingSimpleCulling, ref dictionary, ref dictionary2, ref dictionary3, ref dictionary4);
		lock (visibleRenderers)
		{
			Dictionary<int, SECTR_Member.Child>.Enumerator enumerator = dictionary.GetEnumerator();
			while (enumerator.MoveNext())
			{
				visibleRenderers[enumerator.Current.Key] = enumerator.Current.Value;
			}
		}
		lock (visibleLights)
		{
			Dictionary<int, SECTR_Member.Child>.Enumerator enumerator2 = dictionary2.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				visibleLights[enumerator2.Current.Key] = enumerator2.Current.Value;
			}
		}
		lock (visibleTerrains)
		{
			Dictionary<int, SECTR_Member.Child>.Enumerator enumerator3 = dictionary3.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				visibleTerrains[enumerator3.Current.Key] = enumerator3.Current.Value;
			}
		}
		lock (shadowLights)
		{
			Dictionary<SECTR_Member.Child, int>.Enumerator enumerator4 = dictionary4.GetEnumerator();
			while (enumerator4.MoveNext())
			{
				shadowLights[enumerator4.Current.Key] = 0;
			}
		}
		lock (threadVisibleListPool)
		{
			dictionary.Clear();
			threadVisibleListPool.Push(dictionary);
			dictionary2.Clear();
			threadVisibleListPool.Push(dictionary2);
			dictionary3.Clear();
			threadVisibleListPool.Push(dictionary3);
		}
		lock (threadShadowLightPool)
		{
			dictionary4.Clear();
			threadShadowLightPool.Push(dictionary4);
		}
		lock (threadFrustumPool)
		{
			cullData.cullingPlanes.Clear();
			threadFrustumPool.Push(cullData.cullingPlanes);
			int count = cullData.occluderFrustums.Count;
			for (int i = 0; i < count; i++)
			{
				cullData.occluderFrustums[i].Clear();
				threadFrustumPool.Push(cullData.occluderFrustums[i]);
			}
		}
		lock (threadOccluderPool)
		{
			cullData.occluderFrustums.Clear();
			threadOccluderPool.Push(cullData.occluderFrustums);
		}
	}

	private void _ShadowCullSectorThread(ThreadCullData cullData)
	{
		Dictionary<int, SECTR_Member.Child> dictionary = null;
		Dictionary<int, SECTR_Member.Child> dictionary2 = null;
		lock (threadVisibleListPool)
		{
			dictionary = ((threadVisibleListPool.Count <= 0) ? new Dictionary<int, SECTR_Member.Child>(32) : threadVisibleListPool.Pop());
			dictionary2 = ((threadVisibleListPool.Count <= 0) ? new Dictionary<int, SECTR_Member.Child>(32) : threadVisibleListPool.Pop());
		}
		_ShadowCullSector(cullData.sector, cullData.sectorShadowLights, ref dictionary, ref dictionary2);
		lock (visibleRenderers)
		{
			Dictionary<int, SECTR_Member.Child>.Enumerator enumerator = dictionary.GetEnumerator();
			while (enumerator.MoveNext())
			{
				visibleRenderers[enumerator.Current.Key] = enumerator.Current.Value;
			}
		}
		lock (visibleTerrains)
		{
			Dictionary<int, SECTR_Member.Child>.Enumerator enumerator2 = dictionary2.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				visibleTerrains[enumerator2.Current.Key] = enumerator2.Current.Value;
			}
		}
		lock (threadVisibleListPool)
		{
			dictionary.Clear();
			threadVisibleListPool.Push(dictionary);
			dictionary2.Clear();
			threadVisibleListPool.Push(dictionary2);
		}
	}

	private static void _FrustumCullSector(SECTR_Sector sector, Vector3 cameraPos, List<Plane> cullingPlanes, List<List<Plane>> occluderFrustums, int baseMask, float shadowDistance, bool forceGroupCull, ref Dictionary<int, SECTR_Member.Child> visibleRenderers, ref Dictionary<int, SECTR_Member.Child> visibleLights, ref Dictionary<int, SECTR_Member.Child> visibleTerrains, ref Dictionary<SECTR_Member.Child, int> shadowLights)
	{
		_FrustumCull(sector, cameraPos, cullingPlanes, occluderFrustums, baseMask, shadowDistance, forceGroupCull, ref visibleRenderers, ref visibleLights, ref visibleTerrains, ref shadowLights);
		int count = sector.Members.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Member sECTR_Member = sector.Members[i];
			if (sECTR_Member.HasRenderBounds || sECTR_Member.HasLightBounds)
			{
				_FrustumCull(sECTR_Member, cameraPos, cullingPlanes, occluderFrustums, baseMask, shadowDistance, forceGroupCull, ref visibleRenderers, ref visibleLights, ref visibleTerrains, ref shadowLights);
			}
		}
	}

	private static void _FrustumCull(SECTR_Member member, Vector3 cameraPos, List<Plane> frustumPlanes, List<List<Plane>> occluders, int baseMask, float shadowDistance, bool forceGroupCull, ref Dictionary<int, SECTR_Member.Child> visibleRenderers, ref Dictionary<int, SECTR_Member.Child> visibleLights, ref Dictionary<int, SECTR_Member.Child> visibleTerrains, ref Dictionary<SECTR_Member.Child, int> shadowLights)
	{
		int outMask = 0;
		int outMask2 = 0;
		bool flag = member.CullEachChild && !forceGroupCull;
		bool flag2 = member.HasRenderBounds && SECTR_Geometry.FrustumIntersectsBounds(member.RenderBounds, frustumPlanes, baseMask, out outMask);
		bool flag3 = member.HasLightBounds && SECTR_Geometry.FrustumIntersectsBounds(member.LightBounds, frustumPlanes, baseMask, out outMask2);
		int count = occluders.Count;
		for (int i = 0; i < count; i++)
		{
			if (!flag2 && !flag3)
			{
				break;
			}
			List<Plane> frustum = occluders[i];
			if (flag2)
			{
				flag2 = !SECTR_Geometry.FrustumContainsBounds(member.RenderBounds, frustum);
			}
			if (flag3)
			{
				flag3 = !SECTR_Geometry.FrustumContainsBounds(member.LightBounds, frustum);
			}
		}
		if (flag2)
		{
			int count2 = member.Renderers.Count;
			for (int j = 0; j < count2; j++)
			{
				SECTR_Member.Child child = member.Renderers[j];
				if (child.renderHash != 0 && !visibleRenderers.ContainsKey(child.renderHash) && (!flag || _IsVisible(child.rendererBounds, frustumPlanes, outMask, occluders)))
				{
					visibleRenderers.Add(child.renderHash, child);
				}
			}
			int count3 = member.Terrains.Count;
			for (int k = 0; k < count3; k++)
			{
				SECTR_Member.Child child2 = member.Terrains[k];
				if (child2.terrainHash != 0 && !visibleTerrains.ContainsKey(child2.terrainHash) && (!flag || _IsVisible(child2.terrainBounds, frustumPlanes, outMask, occluders)))
				{
					visibleTerrains.Add(child2.terrainHash, child2);
				}
			}
		}
		if (!flag3)
		{
			return;
		}
		int count4 = member.Lights.Count;
		for (int l = 0; l < count4; l++)
		{
			SECTR_Member.Child child3 = member.Lights[l];
			if (child3.lightHash != 0 && !visibleLights.ContainsKey(child3.lightHash) && (!flag || _IsVisible(child3.lightBounds, frustumPlanes, outMask, occluders)))
			{
				visibleLights.Add(child3.lightHash, child3);
				if (child3.shadowLight && !shadowLights.ContainsKey(child3) && Vector3.Distance(cameraPos, child3.shadowLightPosition) - child3.shadowLightRange <= shadowDistance)
				{
					shadowLights.Add(child3, 0);
				}
			}
		}
	}

	private static void _ShadowCullSector(SECTR_Sector sector, List<SECTR_Member.Child> sectorShadowLights, ref Dictionary<int, SECTR_Member.Child> visibleRenderers, ref Dictionary<int, SECTR_Member.Child> visibleTerrains)
	{
		if (sector.ShadowCaster)
		{
			_ShadowCull(sector, sectorShadowLights, ref visibleRenderers, ref visibleTerrains);
		}
		int count = sector.Members.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Member sECTR_Member = sector.Members[i];
			if (sECTR_Member.ShadowCaster)
			{
				_ShadowCull(sECTR_Member, sectorShadowLights, ref visibleRenderers, ref visibleTerrains);
			}
		}
	}

	private static void _ShadowCull(SECTR_Member member, List<SECTR_Member.Child> shadowLights, ref Dictionary<int, SECTR_Member.Child> visibleRenderers, ref Dictionary<int, SECTR_Member.Child> visibleTerrains)
	{
		int count = shadowLights.Count;
		int count2 = member.ShadowCasters.Count;
		if (member.CullEachChild)
		{
			for (int i = 0; i < count2; i++)
			{
				SECTR_Member.Child child = member.ShadowCasters[i];
				if (child.renderHash != 0 && !visibleRenderers.ContainsKey(child.renderHash))
				{
					for (int j = 0; j < count; j++)
					{
						SECTR_Member.Child child2 = shadowLights[j];
						if ((child2.shadowCullingMask & (1 << (int)child.layer)) != 0 && ((child2.shadowLightType == LightType.Spot && child.rendererBounds.Intersects(child2.lightBounds)) || (child2.shadowLightType == LightType.Point && SECTR_Geometry.BoundsIntersectsSphere(child.rendererBounds, child2.shadowLightPosition, child2.shadowLightRange))))
						{
							visibleRenderers.Add(child.renderHash, child);
							break;
						}
					}
				}
				if (child.terrainHash == 0 || visibleTerrains.ContainsKey(child.terrainHash))
				{
					continue;
				}
				for (int k = 0; k < count; k++)
				{
					SECTR_Member.Child child3 = shadowLights[k];
					if ((child3.shadowCullingMask & (1 << (int)child.layer)) != 0 && ((child3.shadowLightType == LightType.Spot && child.terrainBounds.Intersects(child3.lightBounds)) || (child3.shadowLightType == LightType.Point && SECTR_Geometry.BoundsIntersectsSphere(child.terrainBounds, child3.shadowLightPosition, child3.shadowLightRange))))
					{
						visibleTerrains.Add(child.terrainHash, child);
						break;
					}
				}
			}
			return;
		}
		for (int l = 0; l < count; l++)
		{
			SECTR_Member.Child child4 = shadowLights[l];
			if (!((child4.shadowLightType != LightType.Spot) ? SECTR_Geometry.BoundsIntersectsSphere(member.RenderBounds, child4.shadowLightPosition, child4.shadowLightRange) : member.RenderBounds.Intersects(child4.lightBounds)))
			{
				continue;
			}
			int shadowCullingMask = child4.shadowCullingMask;
			for (int m = 0; m < count2; m++)
			{
				SECTR_Member.Child child5 = member.ShadowCasters[m];
				if (child5.renderHash != 0 && child5.terrainHash != 0)
				{
					if ((shadowCullingMask & (1 << (int)child5.layer)) != 0)
					{
						if (!visibleRenderers.ContainsKey(child5.renderHash))
						{
							visibleRenderers.Add(child5.renderHash, child5);
						}
						if (!visibleTerrains.ContainsKey(child5.terrainHash))
						{
							visibleTerrains.Add(child5.terrainHash, child5);
						}
					}
				}
				else if (child5.renderHash != 0 && !visibleRenderers.ContainsKey(child5.renderHash) && (shadowCullingMask & (1 << (int)child5.layer)) != 0)
				{
					visibleRenderers.Add(child5.renderHash, child5);
				}
				else if (child5.terrainHash != 0 && !visibleTerrains.ContainsKey(child5.terrainHash) && (shadowCullingMask & (1 << (int)child5.layer)) != 0)
				{
					visibleTerrains.Add(child5.terrainHash, child5);
				}
			}
		}
	}

	private static bool _IsVisible(Bounds childBounds, List<Plane> frustumPlanes, int parentMask, List<List<Plane>> occluders)
	{
		int outMask;
		if (SECTR_Geometry.FrustumIntersectsBounds(childBounds, frustumPlanes, parentMask, out outMask))
		{
			int count = occluders.Count;
			for (int i = 0; i < count; i++)
			{
				if (SECTR_Geometry.FrustumContainsBounds(childBounds, occluders[i]))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private void _HideAllMembers()
	{
		int count = SECTR_Member.All.Count;
		for (int i = 0; i < count; i++)
		{
			SECTR_Member sECTR_Member = SECTR_Member.All[i];
			int count2 = sECTR_Member.Renderers.Count;
			for (int j = 0; j < count2; j++)
			{
				SECTR_Member.Child child = sECTR_Member.Renderers[j];
				child.renderCulled = true;
				if ((bool)child.renderer)
				{
					child.renderer.enabled = false;
				}
				hiddenRenderers[child.renderHash] = child;
			}
			int count3 = sECTR_Member.Lights.Count;
			for (int k = 0; k < count3; k++)
			{
				SECTR_Member.Child child2 = sECTR_Member.Lights[k];
				child2.lightCulled = true;
				if ((bool)child2.light)
				{
					child2.light.enabled = false;
				}
				hiddenLights[child2.lightHash] = child2;
			}
			int count4 = sECTR_Member.Terrains.Count;
			for (int l = 0; l < count4; l++)
			{
				SECTR_Member.Child child3 = sECTR_Member.Terrains[l];
				child3.terrainCulled = true;
				if ((bool)child3.terrain)
				{
					child3.terrain.drawHeightmap = false;
					child3.terrain.drawTreesAndFoliage = false;
				}
				hiddenTerrains[child3.terrainHash] = child3;
			}
		}
	}

	private void _ApplyCulling(bool visible)
	{
		Dictionary<int, SECTR_Member.Child>.Enumerator enumerator = visibleRenderers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SECTR_Member.Child value = enumerator.Current.Value;
			if ((bool)value.renderer)
			{
				value.renderer.enabled = visible;
			}
			value.renderCulled = !visible;
			if (visible)
			{
				hiddenRenderers.Remove(enumerator.Current.Key);
			}
			else
			{
				hiddenRenderers[enumerator.Current.Key] = value;
			}
		}
		if (visible)
		{
			renderersCulled = hiddenRenderers.Count;
		}
		Dictionary<int, SECTR_Member.Child>.Enumerator enumerator2 = visibleLights.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			SECTR_Member.Child value2 = enumerator2.Current.Value;
			if ((bool)value2.light)
			{
				value2.light.enabled = visible;
			}
			value2.lightCulled = !visible;
			if (visible)
			{
				hiddenLights.Remove(enumerator2.Current.Key);
			}
			else
			{
				hiddenLights[enumerator2.Current.Key] = value2;
			}
		}
		if (visible)
		{
			lightsCulled = hiddenLights.Count;
		}
		Dictionary<int, SECTR_Member.Child>.Enumerator enumerator3 = visibleTerrains.GetEnumerator();
		while (enumerator3.MoveNext())
		{
			SECTR_Member.Child value3 = enumerator3.Current.Value;
			if ((bool)value3.terrain)
			{
				value3.terrain.drawHeightmap = visible;
				value3.terrain.drawTreesAndFoliage = visible;
			}
			value3.terrainCulled = !visible;
			if (visible)
			{
				hiddenTerrains.Remove(enumerator3.Current.Key);
			}
			else
			{
				hiddenTerrains[enumerator3.Current.Key] = value3;
			}
		}
		if (visible)
		{
			terrainsCulled = hiddenTerrains.Count;
		}
		didCull = true;
	}

	private void _UndoCulling()
	{
		if (!didCull)
		{
			return;
		}
		Dictionary<int, SECTR_Member.Child>.Enumerator enumerator = hiddenRenderers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SECTR_Member.Child value = enumerator.Current.Value;
			if ((bool)value.renderer)
			{
				value.renderer.enabled = true;
			}
			value.renderCulled = false;
		}
		hiddenRenderers.Clear();
		Dictionary<int, SECTR_Member.Child>.Enumerator enumerator2 = hiddenLights.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			SECTR_Member.Child value2 = enumerator2.Current.Value;
			if ((bool)value2.light)
			{
				value2.light.enabled = true;
			}
			value2.lightCulled = false;
		}
		hiddenLights.Clear();
		Dictionary<int, SECTR_Member.Child>.Enumerator enumerator3 = hiddenTerrains.GetEnumerator();
		while (enumerator3.MoveNext())
		{
			SECTR_Member.Child value3 = enumerator3.Current.Value;
			Terrain terrain = value3.terrain;
			if ((bool)value3.terrain)
			{
				terrain.drawHeightmap = true;
				terrain.drawTreesAndFoliage = true;
			}
			value3.terrainCulled = false;
		}
		hiddenTerrains.Clear();
		didCull = false;
	}

	private void _BuildFrustumFromHull(Camera cullingCamera, bool forwardTraversal, List<ClipVertex> portalVertices, ref List<Plane> newFrustum)
	{
		int count = portalVertices.Count;
		if (count <= 2)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = portalVertices[i].vertex;
			Vector3 vector2 = portalVertices[(i + 1) % count].vertex;
			Vector3 vector3 = vector2 - vector;
			if (Vector3.SqrMagnitude(vector3) > 0.001f)
			{
				Vector3 vector4 = vector - cullingCamera.transform.position;
				Vector3 inNormal = ((!forwardTraversal) ? Vector3.Cross(vector4, vector3) : Vector3.Cross(vector3, vector4));
				inNormal.Normalize();
				newFrustum.Add(new Plane(inNormal, vector));
			}
		}
	}
}
