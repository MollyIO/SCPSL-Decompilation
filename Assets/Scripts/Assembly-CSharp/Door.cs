using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Assets._Scripts.RemoteAdmin;
using MEC;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Door : NetworkBehaviour, IComparable
{
	public AudioSource soundsource;

	public AudioClip sound_checkpointWarning;

	public AudioClip sound_denied;

	public MovingStatus moving;

	public GameObject destroyedPrefab;

	public Vector3 localPos;

	public Quaternion localRot;

	internal DoorRemoteAdminButton RemoteAdminButton;

	private SECTR_Portal _portal;

	public Animator[] parts;

	public AudioClip[] sound_open;

	public AudioClip[] sound_close;

	private Rigidbody[] _destoryedRb;

	public int doorType;

	public int status = -1;

	public float curCooldown;

	public float cooldown;

	public bool dontOpenOnWarhead;

	public bool blockAfterDetonation;

	public bool lockdown;

	public bool warheadlock;

	public bool commandlock;

	public bool decontlock;

	public bool GrenadesResistant;

	private bool _buffedStatus;

	private bool _wasLocked;

	private bool _prevDestroyed;

	private bool _deniedInProgress;

	public float scp079Lockdown;

	private bool isLockedBy079;

	public string DoorName;

	public string permissionLevel;

	[HideInInspector]
	public List<GameObject> buttons = new List<GameObject>();

	[SyncVar(hook = "DestroyDoor")]
	public bool destroyed;

	[SyncVar(hook = "SetState")]
	public bool isOpen;

	[SyncVar(hook = "SetLock")]
	public bool locked;

	private static int kRpcRpcDoSound;

	public bool Networkdestroyed
	{
		get
		{
			return destroyed;
		}
		[param: In]
		set
		{
			Door door = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				DestroyDoor(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref door.destroyed, 1u);
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
			Door door = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetState(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref door.isOpen, 2u);
		}
	}

	public bool Networklocked
	{
		get
		{
			return locked;
		}
		[param: In]
		set
		{
			Door door = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetLock(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref door.locked, 4u);
		}
	}

	private void Start()
	{
		scp079Lockdown = -3f;
		Timing.RunCoroutine(_Start(), Segment.FixedUpdate);
	}

	private void Update079Lock()
	{
		bool flag = false;
		NetworkIdentity component = GetComponent<NetworkIdentity>();
		string text = base.transform.parent.name + "/" + base.transform.name;
		foreach (Scp079PlayerScript instance in Scp079PlayerScript.instances)
		{
			foreach (string lockedDoor in instance.lockedDoors)
			{
				if (lockedDoor == text)
				{
					flag = true;
				}
			}
		}
		if (flag != isLockedBy079)
		{
			isLockedBy079 = flag;
			UpdateLock();
		}
	}

	public void LockBy079()
	{
		isLockedBy079 = true;
		UpdateLock();
	}

	private void LateUpdate()
	{
		if (isLockedBy079)
		{
			Update079Lock();
		}
		if (_prevDestroyed != destroyed)
		{
			GameObject gameObject = GameObject.Find("Host");
			if (gameObject != null && RandomSeedSync.generated)
			{
				StartCoroutine(RefreshDestroyAnimation());
			}
		}
		if (curCooldown >= 0f)
		{
			curCooldown -= Time.deltaTime;
		}
		if (NetworkServer.active && scp079Lockdown >= -3f)
		{
			scp079Lockdown -= Time.deltaTime;
			UpdateLock();
		}
		if (!_deniedInProgress && (!locked || permissionLevel == "UNACCESSIBLE"))
		{
			if (curCooldown >= 0f && status != 3)
			{
				if (sound_checkpointWarning == null)
				{
					if (_portal != null)
					{
						_portal.Flags = (SECTR_Portal.PortalFlags)0;
					}
					SetActiveStatus(2);
				}
			}
			else
			{
				if (_portal != null)
				{
					_portal.Flags = ((!(isOpen | destroyed)) ? SECTR_Portal.PortalFlags.Closed : ((SECTR_Portal.PortalFlags)0));
				}
				SetActiveStatus(isOpen ? 1 : 0);
			}
		}
		if (locked && permissionLevel != "UNACCESSIBLE")
		{
			if (_portal != null)
			{
				_portal.Flags = ((!(isOpen | destroyed | moving.moving)) ? SECTR_Portal.PortalFlags.Closed : ((SECTR_Portal.PortalFlags)0));
			}
			if (!_wasLocked)
			{
				_wasLocked = true;
				SetActiveStatus(4);
			}
		}
		else if (_wasLocked)
		{
			_wasLocked = false;
			if (doorType == 3)
			{
				SetState(false);
				CallRpcDoSound();
			}
		}
	}

	public int CompareTo(object obj)
	{
		return string.CompareOrdinal(DoorName, ((Door)obj).DoorName);
	}

	private void SetLock(bool l)
	{
		Networklocked = l;
		if (RemoteAdminButton != null)
		{
			RemoteAdminButton.UpdateColor();
		}
	}

	public void UpdateLock()
	{
		Networklocked = permissionLevel != "UNACCESSIBLE" && (commandlock | lockdown | warheadlock | decontlock | (scp079Lockdown > 0f) | isLockedBy079);
	}

	public void SetPortal(SECTR_Portal p)
	{
		_portal = p;
	}

	public void SetLocalPos()
	{
		localPos = base.transform.localPosition;
		localRot = base.transform.localRotation;
	}

	private IEnumerator<float> _UpdatePosition()
	{
		Animator[] array = parts;
		foreach (Animator animator in array)
		{
			animator.SetBool("isOpen", isOpen);
		}
		if (sound_checkpointWarning == null || !isOpen)
		{
			yield break;
		}
		_deniedInProgress = true;
		moving.moving = true;
		if (!locked)
		{
			SetActiveStatus(2);
		}
		float t = 0f;
		while (t < 5f)
		{
			t += 0.1f;
			yield return Timing.WaitForSeconds(0.1f);
			if (curCooldown < 0f && !locked)
			{
				SetActiveStatus(1);
			}
		}
		if (locked)
		{
			moving.moving = false;
			_deniedInProgress = false;
			yield break;
		}
		soundsource.PlayOneShot(sound_checkpointWarning);
		SetActiveStatus(5);
		yield return Timing.WaitForSeconds(2f);
		SetActiveStatus(0);
		moving.moving = false;
		_deniedInProgress = false;
		SetState(false);
		soundsource.PlayOneShot(sound_close[UnityEngine.Random.Range(0, sound_close.Length)]);
	}

	public void SetState(bool open)
	{
		NetworkisOpen = open;
		ForceCooldown(cooldown);
		if (RemoteAdminButton != null)
		{
			RemoteAdminButton.UpdateColor();
		}
	}

	public void SetStateWithSound(bool open)
	{
		if (isOpen != open)
		{
			CallRpcDoSound();
		}
		NetworkisOpen = open;
		ForceCooldown(cooldown);
	}

	public void DestroyDoor(bool b)
	{
		if (b && destroyedPrefab != null)
		{
			Networkdestroyed = true;
		}
		else
		{
			Networkdestroyed = false;
		}
		if (RemoteAdminButton != null)
		{
			RemoteAdminButton.UpdateColor();
		}
	}

	private IEnumerator RefreshDestroyAnimation()
	{
		Animator[] array = parts;
		foreach (Animator animator in array)
		{
			if (animator.gameObject.activeSelf)
			{
				animator.gameObject.SetActive(false);
				GameObject gameObject = UnityEngine.Object.Instantiate(destroyedPrefab, animator.transform);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
				gameObject.transform.localScale = Vector3.one;
				gameObject.transform.parent = null;
				int num = 0;
				_destoryedRb = gameObject.GetComponentsInChildren<Rigidbody>();
				Vector3 vector = ((!(_portal == null)) ? _portal.GetRandomSectorPos() : Vector3.one);
				Rigidbody[] destoryedRb = _destoryedRb;
				foreach (Rigidbody rigidbody in destoryedRb)
				{
					rigidbody.GetComponent<Collider>().isTrigger = true;
					rigidbody.transform.parent = null;
					Vector3 vector2 = vector - base.transform.position;
					vector2.y = 0f;
					vector2 = vector2.normalized;
					rigidbody.velocity = ((num != 1 && num != 2) ? vector2 : (-vector2)) * UnityEngine.Random.Range(7, 9);
					num++;
				}
			}
		}
		yield return new WaitForSeconds(0.15f);
		Rigidbody[] destoryedRb2 = _destoryedRb;
		foreach (Rigidbody rigidbody2 in destoryedRb2)
		{
			rigidbody2.GetComponent<Collider>().isTrigger = false;
		}
		yield return new WaitForSeconds(5f);
		Rigidbody[] destoryedRb3 = _destoryedRb;
		foreach (Rigidbody rigidbody3 in destoryedRb3)
		{
			rigidbody3.isKinematic = true;
			rigidbody3.GetComponent<Collider>().enabled = false;
		}
	}

	private IEnumerator<float> _Start()
	{
		Component[] componentsInChildren = GetComponentsInChildren(typeof(Renderer));
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Renderer renderer = (Renderer)componentsInChildren[i];
			if (renderer.tag == "DoorButton")
			{
				buttons.Add(renderer.gameObject);
			}
		}
		SetActiveStatus(0);
		float time = 0f;
		while (time < 10f)
		{
			time += 0.02f;
			if (_buffedStatus != isOpen)
			{
				_buffedStatus = isOpen;
				ForceCooldown(cooldown);
				break;
			}
			yield return 0f;
		}
	}

	public void UpdatePos()
	{
		if (!(localPos == Vector3.zero))
		{
			base.transform.localPosition = localPos;
			base.transform.localRotation = localRot;
		}
	}

	public void SetZero()
	{
		localPos = Vector3.zero;
	}

	public bool ChangeState(bool force = false)
	{
		if (!(curCooldown < 0f) || moving.moving || _deniedInProgress || (locked && !force))
		{
			return false;
		}
		if (Recontainer079.isLocked && GetComponent<Scp079Interactable>().currentZonesAndRooms[0].currentZone == "HeavyRooms")
		{
			return false;
		}
		moving.moving = true;
		SetState(!isOpen);
		CallRpcDoSound();
		return true;
	}

	public bool ChangeState079()
	{
		if (!(curCooldown < 0f) || moving.moving || _deniedInProgress || (permissionLevel != "UNACCESSIBLE" && (commandlock | lockdown | warheadlock | decontlock)))
		{
			return false;
		}
		moving.moving = true;
		SetState(!isOpen);
		CallRpcDoSound();
		return true;
	}

	public void OpenDecontamination()
	{
		if (!(permissionLevel == "UNACCESSIBLE"))
		{
			decontlock = true;
			if (!isOpen)
			{
				CallRpcDoSound();
			}
			moving.moving = true;
			SetState(true);
			UpdateLock();
		}
	}

	public void CloseDecontamination()
	{
		if (!(permissionLevel == "UNACCESSIBLE") && !(base.transform.position.y < -100f) && !(base.transform.position.y > 100f))
		{
			decontlock = true;
			if (isOpen)
			{
				CallRpcDoSound();
			}
			moving.moving = true;
			SetState(false);
			UpdateLock();
		}
	}

	public void OpenWarhead(bool force, bool lockDoor)
	{
		if (permissionLevel == "UNACCESSIBLE" || (dontOpenOnWarhead && !force))
		{
			return;
		}
		if (lockDoor)
		{
			warheadlock = true;
		}
		if ((!locked || force) && (force || !(permissionLevel == "CONT_LVL_3")))
		{
			if (!isOpen)
			{
				CallRpcDoSound();
			}
			moving.moving = true;
			SetState(true);
			UpdateLock();
		}
	}

	[ClientRpc(channel = 14)]
	public void RpcDoSound()
	{
		soundsource.PlayOneShot((!isOpen) ? sound_close[UnityEngine.Random.Range(0, sound_close.Length)] : sound_open[UnityEngine.Random.Range(0, sound_open.Length)]);
	}

	public void SetActiveStatus(int s)
	{
		if (status == s)
		{
			return;
		}
		status = s;
		foreach (GameObject button in buttons)
		{
			MeshRenderer component = button.GetComponent<MeshRenderer>();
			Text componentInChildren = button.GetComponentInChildren<Text>();
			Image componentInChildren2 = button.GetComponentInChildren<Image>();
			if (component != null)
			{
				component.material = ButtonStages.types[doorType].stages[s].mat;
			}
			if (componentInChildren != null)
			{
				componentInChildren.text = ButtonStages.types[doorType].stages[s].info;
			}
			if (componentInChildren2 != null)
			{
				componentInChildren2.color = ((!(ButtonStages.types[doorType].stages[s].texture == null)) ? Color.white : Color.clear);
				componentInChildren2.sprite = ButtonStages.types[doorType].stages[s].texture;
			}
		}
	}

	public IEnumerator<float> _Denied()
	{
		if (curCooldown < 0f && !moving.moving && !_deniedInProgress)
		{
			_deniedInProgress = true;
			soundsource.PlayOneShot(sound_denied);
			if (!locked)
			{
				SetActiveStatus(3);
			}
			yield return Timing.WaitForSeconds(1f);
			_deniedInProgress = false;
		}
	}

	public void ForceCooldown(float cd)
	{
		curCooldown = cd;
		Timing.RunCoroutine(_UpdatePosition(), Segment.Update);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcDoSound(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDoSound called on server.");
		}
		else
		{
			((Door)obj).RpcDoSound();
		}
	}

	public void CallRpcDoSound()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcDoSound called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcDoSound);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 14, "RpcDoSound");
	}

	static Door()
	{
		kRpcRpcDoSound = 630763456;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Door), kRpcRpcDoSound, InvokeRpcRpcDoSound);
		NetworkCRC.RegisterBehaviour("Door", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(destroyed);
			writer.Write(isOpen);
			writer.Write(locked);
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
			writer.Write(destroyed);
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
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(locked);
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
			destroyed = reader.ReadBoolean();
			isOpen = reader.ReadBoolean();
			locked = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			DestroyDoor(reader.ReadBoolean());
		}
		if ((num & 2) != 0)
		{
			SetState(reader.ReadBoolean());
		}
		if ((num & 4) != 0)
		{
			SetLock(reader.ReadBoolean());
		}
	}
}
