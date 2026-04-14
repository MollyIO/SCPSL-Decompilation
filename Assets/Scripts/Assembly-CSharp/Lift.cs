using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MEC;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Lift : NetworkBehaviour
{
	[Serializable]
	public struct Elevator
	{
		public Transform target;

		public Animator door;

		public AudioSource musicSpeaker;

		private Vector3 pos;

		public void SetPosition()
		{
			pos = target.position;
		}

		public Vector3 GetPosition()
		{
			return pos;
		}
	}

	public enum Status
	{
		Up = 0,
		Down = 1,
		Moving = 2
	}

	public Elevator[] elevators;

	public string elevatorName;

	public float movingSpeed;

	public float maxDistance;

	public bool lockable;

	public bool operative = true;

	public Status status;

	private List<MeshRenderer> panels = new List<MeshRenderer>();

	public Material[] panelIcons;

	[SyncVar(hook = "SetLock")]
	private bool locked;

	[SyncVar(hook = "SetStatus")]
	public int statusID;

	public Text monitor;

	private static int kRpcRpcPlayMusic;

	public bool Networklocked
	{
		get
		{
			return locked;
		}
		[param: In]
		set
		{
			Lift lift = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetLock(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref lift.locked, 1u);
		}
	}

	public int NetworkstatusID
	{
		get
		{
			return statusID;
		}
		[param: In]
		set
		{
			Lift lift = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetStatus(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref lift.statusID, 2u);
		}
	}

	private void Start()
	{
		Elevator[] array = elevators;
		for (int i = 0; i < array.Length; i++)
		{
			Elevator elevator = array[i];
			MeshRenderer[] componentsInChildren = elevator.door.transform.parent.GetComponentsInChildren<MeshRenderer>();
			MeshRenderer[] array2 = componentsInChildren;
			foreach (MeshRenderer meshRenderer in array2)
			{
				if (meshRenderer.tag == "ElevatorButton")
				{
					panels.Add(meshRenderer);
				}
			}
		}
	}

	private void RefreshPanelIcons()
	{
		foreach (MeshRenderer panel in panels)
		{
			panel.sharedMaterial = panelIcons[statusID];
		}
	}

	private void Awake()
	{
		Elevator[] array = elevators;
		for (int i = 0; i < array.Length; i++)
		{
			Elevator elevator = array[i];
			elevator.target.tag = "LiftTarget";
		}
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < elevators.Length; i++)
		{
			bool value = statusID == i && status != Status.Moving;
			elevators[i].door.SetBool("isOpen", value);
		}
	}

	private void LateUpdate()
	{
		RefreshPanelIcons();
	}

	private void SetStatus(int i)
	{
		NetworkstatusID = i;
		status = (Status)i;
	}

	private void SetLock(bool b)
	{
		Networklocked = b;
		if (b && monitor != null)
		{
			monitor.text = "ELEVATOR SYSTEM <color=#e00>DISABLED</color>";
		}
	}

	public void Lock()
	{
		if (lockable)
		{
			SetLock(true);
			Timing.RunCoroutine(_LockdownUpdate(), Segment.Update);
		}
	}

	public bool UseLift()
	{
		if (!operative || AlphaWarheadController.host.timeToDetonation == 0f || locked)
		{
			return false;
		}
		Timing.RunCoroutine(_LiftAnimation(), Segment.Update);
		operative = false;
		return true;
	}

	private IEnumerator<float> _LiftAnimation()
	{
		Transform target = null;
		Elevator[] array = elevators;
		for (int i = 0; i < array.Length; i++)
		{
			Elevator elevator = array[i];
			if (!elevator.door.GetBool("isOpen"))
			{
				target = elevator.target;
			}
		}
		Status previousStatus = status;
		SetStatus(2);
		yield return Timing.WaitForSeconds(0.7f);
		CallRpcPlayMusic();
		yield return Timing.WaitForSeconds(2f);
		MovePlayers(target);
		yield return Timing.WaitForSeconds(movingSpeed - 2f);
		SetStatus((previousStatus != Status.Down) ? 1 : 0);
		yield return Timing.WaitForSeconds(2f);
		operative = true;
	}

	private IEnumerator<float> _LockdownUpdate()
	{
		while (status == Status.Moving || !operative)
		{
			yield return 0f;
		}
		if (status == Status.Down)
		{
			Timing.RunCoroutine(_LiftAnimation(), Segment.FixedUpdate);
		}
	}

	[ClientRpc(channel = 4)]
	private void RpcPlayMusic()
	{
		Elevator[] array = elevators;
		for (int i = 0; i < array.Length; i++)
		{
			Elevator elevator = array[i];
			try
			{
				elevator.musicSpeaker.Play();
			}
			catch
			{
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Elevator[] array = elevators;
		for (int i = 0; i < array.Length; i++)
		{
			Elevator elevator = array[i];
			Gizmos.DrawWireCube(elevator.target.transform.position, Vector3.one * maxDistance * 2f);
		}
	}

	private void MovePlayers(Transform target)
	{
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			GameObject which = null;
			if (InRange(gameObject.transform.position, out which) && !(which.transform == target))
			{
				PlyMovementSync component = gameObject.GetComponent<PlyMovementSync>();
				gameObject.transform.parent = which.transform;
				Vector3 localPosition = gameObject.transform.localPosition;
				gameObject.transform.parent = target.transform;
				gameObject.transform.localPosition = localPosition;
				gameObject.transform.parent = null;
				component.SetPosition(gameObject.transform.position);
				component.SetRotation(target.transform.rotation.eulerAngles.y - which.transform.rotation.eulerAngles.y);
				gameObject.transform.parent = null;
			}
		}
		Elevator[] array = elevators;
		for (int j = 0; j < array.Length; j++)
		{
			Elevator elevator = array[j];
			Collider[] array2 = Physics.OverlapBox(elevator.target.transform.position, Vector3.one * maxDistance * 2f);
			foreach (Collider collider in array2)
			{
				if (collider.GetComponent<Pickup>() != null || collider.GetComponent<Grenade>() != null)
				{
					GameObject which2 = null;
					if (InRange(collider.transform.position, out which2, 1.3f) && !(which2.transform == target))
					{
						collider.transform.parent = which2.transform;
						Vector3 localPosition2 = collider.transform.localPosition;
						Quaternion localRotation = collider.transform.localRotation;
						collider.transform.parent = target.transform;
						collider.transform.localPosition = localPosition2;
						collider.transform.localRotation = localRotation;
						collider.transform.parent = null;
					}
				}
			}
		}
	}

	public bool InRange(Vector3 pos, out GameObject which, float maxDistanceMultiplier = 1f)
	{
		Elevator[] array = elevators;
		for (int i = 0; i < array.Length; i++)
		{
			Elevator elevator = array[i];
			bool flag = !(Mathf.Abs(elevator.target.position.x - pos.x) > maxDistance * maxDistanceMultiplier);
			if (Mathf.Abs(elevator.target.position.y - pos.y) > maxDistance * maxDistanceMultiplier)
			{
				flag = false;
			}
			if (Mathf.Abs(elevator.target.position.z - pos.z) > maxDistance * maxDistanceMultiplier)
			{
				flag = false;
			}
			if (flag)
			{
				which = elevator.target.gameObject;
				return true;
			}
		}
		which = null;
		return false;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcPlayMusic(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayMusic called on server.");
		}
		else
		{
			((Lift)obj).RpcPlayMusic();
		}
	}

	public void CallRpcPlayMusic()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPlayMusic called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPlayMusic);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 4, "RpcPlayMusic");
	}

	static Lift()
	{
		kRpcRpcPlayMusic = 374858512;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Lift), kRpcRpcPlayMusic, InvokeRpcRpcPlayMusic);
		NetworkCRC.RegisterBehaviour("Lift", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(locked);
			writer.WritePackedUInt32((uint)statusID);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(locked);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)statusID);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			locked = reader.ReadBoolean();
			statusID = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetLock(reader.ReadBoolean());
		}
		if ((num & 2) != 0)
		{
			SetStatus((int)reader.ReadPackedUInt32());
		}
	}
}
