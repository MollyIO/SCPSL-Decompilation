using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

public class Locker : NetworkBehaviour
{
	[Serializable]
	public class LockerDrop
	{
		public int[] itemId;
	}

	[SyncVar]
	private Offset localPos;

	public List<Transform> spawnLocations;

	public LockerDrop[] drops;

	[SyncVar(hook = "SetOpen")]
	public bool isOpen;

	public Animator[] anims;

	private bool prevOpen;

	public Offset NetworklocalPos
	{
		get
		{
			return localPos;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref localPos, 1u);
		}
	}

	public bool NetworkisOpen
	{
		get
		{
			return isOpen;
		}
		[param: In]
		set
		{
			Locker locker = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetOpen(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref locker.isOpen, 2u);
		}
	}

	public void SetOpen(bool b)
	{
		NetworkisOpen = b;
	}

	private void Awake()
	{
		NetworklocalPos = new Offset
		{
			position = base.transform.localPosition,
			rotation = base.transform.localRotation.eulerAngles
		};
	}

	[ServerCallback]
	public void GetReady()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		NetworkisOpen = false;
		bool flag = false;
		while (!flag)
		{
			LockerDrop[] array = drops;
			foreach (LockerDrop lockerDrop in array)
			{
				if (spawnLocations.Count > 0)
				{
					int index = UnityEngine.Random.Range(0, spawnLocations.Count);
					int num = UnityEngine.Random.Range(0, lockerDrop.itemId.Length);
					if (num >= 0)
					{
						PlayerManager.localPlayer.GetComponent<Inventory>().SetPickup(lockerDrop.itemId[num], -4.6566467E+11f, spawnLocations[index].transform.position, spawnLocations[index].transform.rotation, 0, 0, 0);
						flag = true;
						spawnLocations.RemoveAt(index);
					}
				}
			}
		}
	}

	[ServerCallback]
	public void Open()
	{
		if (NetworkServer.active)
		{
			NetworkisOpen = true;
		}
	}

	public void Update()
	{
		if (prevOpen != isOpen)
		{
			prevOpen = isOpen;
			Animator[] array = anims;
			foreach (Animator animator in array)
			{
				animator.SetBool("isopen", isOpen);
			}
			GetComponent<AudioSource>().Play();
		}
		base.transform.localPosition = localPos.position;
		base.transform.localRotation = Quaternion.Euler(localPos.rotation);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			GeneratedNetworkCode._WriteOffset_None(writer, localPos);
			writer.Write(isOpen);
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
			GeneratedNetworkCode._WriteOffset_None(writer, localPos);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(isOpen);
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
			localPos = GeneratedNetworkCode._ReadOffset_None(reader);
			isOpen = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			localPos = GeneratedNetworkCode._ReadOffset_None(reader);
		}
		if ((num & 2) != 0)
		{
			SetOpen(reader.ReadBoolean());
		}
	}
}
