using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

public class Generator079 : NetworkBehaviour
{
	private Animator anim;

	public Animator tabletAnim;

	[SyncVar]
	public bool isDoorOpen;

	[SyncVar]
	public bool isDoorUnlocked;

	[SyncVar]
	public bool isTabletConnected;

	[SyncVar(hook = "SetTime")]
	public float remainingPowerup;

	public float startDuration = 90f;

	[SyncVar(hook = "SetOffset")]
	private Offset position;

	private float doorAnimationCooldown;

	private float tabletAnimCooldown;

	private float deniedCooldown;

	private float localTime;

	private bool prevConn;

	private AudioSource asource;

	public MeshRenderer keycardRenderer;

	public MeshRenderer wmtRenderer;

	public MeshRenderer epsenRenderer;

	public MeshRenderer epsdisRenderer;

	public MeshRenderer cancel1Rend;

	public MeshRenderer cancel2Rend;

	public Material matLocked;

	public Material matUnlocked;

	public Material matDenied;

	public Material matLedBlack;

	public Material matLetGreen;

	public Material matLetBlue;

	public Material cancel1MatDis;

	public Material cancel2MatDis;

	public Material cancel1MatEn;

	public Material cancel2MatEn;

	public AudioClip clipOpen;

	public AudioClip clipClose;

	public AudioClip beepSound;

	public AudioClip unlockSound;

	public AudioClip clipConnect;

	public AudioClip clipDisconnect;

	public AudioClip clipCounter;

	public Transform tabletEjectionPoint;

	public TextMeshProUGUI countdownText;

	public TextMeshProUGUI warningText;

	public Transform localArrow;

	public Transform totalArrow;

	public float localVoltage;

	[SyncVar(hook = "SetTotal")]
	public int totalVoltage;

	public string curRoom;

	public static List<Generator079> generators;

	public static Generator079 mainGenerator;

	private string prevMin;

	private bool prevReady;

	private bool prevFinish;

	private bool prevUnlocked;

	private static int kRpcRpcDenied;

	private static int kRpcRpcOvercharge;

	private static int kRpcRpcNotify;

	private static int kRpcRpcDoSound;

	public bool NetworkisDoorOpen
	{
		get
		{
			return isDoorOpen;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref isDoorOpen, 1u);
		}
	}

	public bool NetworkisDoorUnlocked
	{
		get
		{
			return isDoorUnlocked;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref isDoorUnlocked, 2u);
		}
	}

	public bool NetworkisTabletConnected
	{
		get
		{
			return isTabletConnected;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref isTabletConnected, 4u);
		}
	}

	public float NetworkremainingPowerup
	{
		get
		{
			return remainingPowerup;
		}
		[param: In]
		set
		{
			Generator079 generator = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetTime(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref generator.remainingPowerup, 8u);
		}
	}

	public Offset Networkposition
	{
		get
		{
			return position;
		}
		[param: In]
		set
		{
			Generator079 generator = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetOffset(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref generator.position, 16u);
		}
	}

	public int NetworktotalVoltage
	{
		get
		{
			return totalVoltage;
		}
		[param: In]
		set
		{
			Generator079 generator = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetTotal(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref generator.totalVoltage, 32u);
		}
	}

	public void SetTotal(int total)
	{
		NetworktotalVoltage = total;
	}

	public void SetOffset(Offset o)
	{
		Networkposition = o;
	}

	private void SetTime(float time)
	{
		NetworkremainingPowerup = time;
		if (Mathf.Abs(time - localTime) > 1f)
		{
			localTime = time;
		}
	}

	private void Awake()
	{
		if (!base.name.Contains("("))
		{
			mainGenerator = this;
		}
		asource = GetComponent<AudioSource>();
		anim = GetComponent<Animator>();
		generators.Clear();
	}

	private void Start()
	{
		if (NetworkServer.active)
		{
			float num = (NetworkremainingPowerup = startDuration);
			localTime = num;
		}
		generators.Add(this);
	}

	private void Update()
	{
		if (tabletAnimCooldown >= -1f)
		{
			tabletAnimCooldown -= Time.deltaTime;
		}
		if (base.transform.position != position.position || base.transform.rotation != Quaternion.Euler(position.rotation) || string.IsNullOrEmpty(curRoom))
		{
			base.transform.position = position.position;
			base.transform.rotation = Quaternion.Euler(position.rotation);
			RaycastHit hitInfo;
			if (Physics.Raycast(new Ray(base.transform.position - base.transform.forward, Vector3.down), out hitInfo, 50f, Interface079.singleton.roomDetectionMask))
			{
				Transform parent = hitInfo.transform;
				while (parent != null && !parent.transform.name.ToUpper().Contains("ROOT"))
				{
					parent = parent.transform.parent;
				}
				if (parent != null)
				{
					curRoom = parent.transform.name;
				}
			}
		}
		anim.SetBool("isOpen", isDoorOpen);
		float[] array = new float[6] { -57f, -38f, -22f, 0f, 22f, 38f };
		localArrow.transform.localRotation = Quaternion.Lerp(localArrow.transform.localRotation, Quaternion.Euler(0f, Mathf.Lerp(-40f, 40f, localVoltage), 0f), Time.deltaTime * 2f);
		totalArrow.transform.localRotation = Quaternion.Lerp(totalArrow.transform.localRotation, Quaternion.Euler(0f, array[Mathf.Clamp(mainGenerator.totalVoltage, 0, 5)], 0f), Time.deltaTime * 2f);
		if (doorAnimationCooldown >= 0f)
		{
			doorAnimationCooldown -= Time.deltaTime;
		}
		if (deniedCooldown >= 0f)
		{
			deniedCooldown -= Time.deltaTime;
			if (deniedCooldown < 0f)
			{
				keycardRenderer.material = ((!isDoorUnlocked) ? matLocked : matUnlocked);
			}
		}
	}

	private void LateUpdate()
	{
		if (Mathf.Abs(localTime - remainingPowerup) > 1.3f || remainingPowerup == 0f)
		{
			localTime = remainingPowerup;
		}
		if (prevConn && tabletAnimCooldown <= 0f && localTime > 0f)
		{
			if (NetworkServer.active && remainingPowerup > 0f)
			{
				NetworkremainingPowerup = remainingPowerup - Time.deltaTime;
				if (remainingPowerup < 0f)
				{
					NetworkremainingPowerup = 0f;
				}
				localTime = remainingPowerup;
			}
			localTime -= Time.deltaTime;
			if (localTime < 0f)
			{
				localTime = 0f;
			}
			float num = localTime;
			int num2 = 0;
			while (num >= 60f)
			{
				num -= 60f;
				num2++;
			}
			string[] array = (Mathf.Round(num * 100f) / 100f).ToString("00.00").Split('.');
			if (array.Length >= 2)
			{
				if (array[0] != prevMin)
				{
					prevMin = array[0];
					if (tabletAnimCooldown < -0.5f)
					{
						asource.PlayOneShot(clipCounter);
					}
				}
				countdownText.text = num2.ToString("00") + ":" + array[0] + ":" + array[1];
				warningText.enabled = true;
			}
		}
		else
		{
			countdownText.text = ((!(localTime > 0f)) ? "ENGAGED" : string.Empty);
			warningText.enabled = false;
			if (NetworkServer.active && prevConn && localTime <= 0f && isTabletConnected)
			{
				int num3 = 0;
				foreach (Generator079 generator in generators)
				{
					if (generator.localTime <= 0f)
					{
						num3++;
					}
				}
				NetworkremainingPowerup = 0f;
				localTime = 0f;
				EjectTablet();
				CallRpcNotify(num3);
				if (num3 < 5)
				{
					PlayerManager.localPlayer.GetComponent<MTFRespawn>().CallRpcPlayCustomAnnouncement("SCP079RECON" + num3, false);
				}
				else
				{
					Recontainer079.BeginContainment();
				}
				mainGenerator.NetworktotalVoltage = num3;
			}
			if (!prevConn && NetworkServer.active && tabletAnimCooldown < 0f && remainingPowerup < startDuration - 1f && remainingPowerup > 0f)
			{
				NetworkremainingPowerup = remainingPowerup + Time.deltaTime;
			}
		}
		localVoltage = 1f - Mathf.InverseLerp(0f, startDuration, localTime);
		CheckTabletConnectionStatus();
		CheckFinish();
		Unlock();
	}

	public void Interact(GameObject person, string command)
	{
		if (command.StartsWith("EPS_DOOR"))
		{
			OpenClose(person);
		}
		else
		{
			if (command.StartsWith("EPS_TABLET"))
			{
				if (isTabletConnected || !isDoorOpen || !(localTime > 0f))
				{
					return;
				}
				Inventory component = person.GetComponent<Inventory>();
				{
					foreach (Inventory.SyncItemInfo item in component.items)
					{
						if (item.id == 19)
						{
							component.items.Remove(item);
							NetworkisTabletConnected = true;
							break;
						}
					}
					return;
				}
			}
			if (command.StartsWith("EPS_CANCEL"))
			{
				EjectTablet();
			}
			else
			{
				Debug.LogError("Unknown command: " + command);
			}
		}
	}

	public void EjectTablet()
	{
		if (isTabletConnected)
		{
			NetworkisTabletConnected = false;
			PlayerManager.localPlayer.GetComponent<Inventory>().SetPickup(19, 0f, tabletEjectionPoint.position, tabletEjectionPoint.rotation, 0, 0, 0);
		}
	}

	private void CheckTabletConnectionStatus()
	{
		if (prevConn != isTabletConnected)
		{
			prevConn = isTabletConnected;
			tabletAnimCooldown = 1f;
			tabletAnim.SetBool("b", prevConn);
			asource.PlayOneShot((!prevConn) ? clipDisconnect : clipConnect);
		}
		bool flag = prevConn && tabletAnimCooldown <= 0f;
		if (prevReady != flag)
		{
			prevReady = flag;
			wmtRenderer.material = ((!flag) ? matLedBlack : matLetBlue);
			Material material = ((!(localTime > 0f) || !prevConn || !(tabletAnimCooldown <= 0f)) ? cancel1MatDis : cancel1MatEn);
			cancel1Rend.material = material;
			material = ((!(localTime > 0f) || !prevConn || !(tabletAnimCooldown <= 0f)) ? cancel2MatDis : cancel2MatEn);
			cancel2Rend.material = material;
		}
	}

	private void CheckFinish()
	{
		if (!prevFinish && localTime <= 0f)
		{
			prevFinish = true;
			epsenRenderer.material = matLetGreen;
			epsdisRenderer.material = matLedBlack;
			asource.PlayOneShot(unlockSound);
		}
	}

	private void OpenClose(GameObject person)
	{
		Inventory component = person.GetComponent<Inventory>();
		if (component == null || doorAnimationCooldown > 0f || deniedCooldown > 0f)
		{
			return;
		}
		if (!isDoorUnlocked)
		{
			bool flag = person.GetComponent<ServerRoles>().BypassMode;
			if (component.curItem > 0)
			{
				string[] permissions = component.availableItems[component.curItem].permissions;
				foreach (string text in permissions)
				{
					if (text == "ARMORY_LVL_3")
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				NetworkisDoorUnlocked = true;
				doorAnimationCooldown = 0.5f;
			}
			else
			{
				CallRpcDenied();
			}
		}
		else
		{
			doorAnimationCooldown = 1.5f;
			NetworkisDoorOpen = !isDoorOpen;
			CallRpcDoSound(isDoorOpen);
		}
	}

	[ClientRpc]
	private void RpcDenied()
	{
		deniedCooldown = 0.5f;
		if (keycardRenderer.material != matUnlocked)
		{
			keycardRenderer.material = matDenied;
		}
		asource.PlayOneShot(beepSound);
	}

	[ClientRpc]
	public void RpcOvercharge()
	{
		FlickerableLight[] array = Object.FindObjectsOfType<FlickerableLight>();
		foreach (FlickerableLight flickerableLight in array)
		{
			Scp079Interactable component = flickerableLight.GetComponent<Scp079Interactable>();
			if (component == null || component.currentZonesAndRooms[0].currentZone == "HeavyRooms")
			{
				flickerableLight.EnableFlickering(10f);
			}
		}
	}

	private void Unlock()
	{
		if (prevUnlocked != isDoorUnlocked)
		{
			prevUnlocked = true;
			asource.PlayOneShot(unlockSound);
			keycardRenderer.material = matUnlocked;
		}
	}

	[ClientRpc]
	private void RpcNotify(int curr)
	{
		if (Interface079.lply != null && Interface079.lply.iAm079)
		{
			Interface079.singleton.AddBigNotification("ENGAGED GENERATORS:\n" + curr + "/5");
		}
	}

	[ClientRpc]
	private void RpcDoSound(bool isOpen)
	{
		asource.PlayOneShot((!isOpen) ? clipClose : clipOpen);
	}

	static Generator079()
	{
		generators = new List<Generator079>();
		kRpcRpcDenied = -443288176;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Generator079), kRpcRpcDenied, InvokeRpcRpcDenied);
		kRpcRpcOvercharge = -228857859;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Generator079), kRpcRpcOvercharge, InvokeRpcRpcOvercharge);
		kRpcRpcNotify = -147582658;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Generator079), kRpcRpcNotify, InvokeRpcRpcNotify);
		kRpcRpcDoSound = -595480593;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Generator079), kRpcRpcDoSound, InvokeRpcRpcDoSound);
		NetworkCRC.RegisterBehaviour("Generator079", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcDenied(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDenied called on server.");
		}
		else
		{
			((Generator079)obj).RpcDenied();
		}
	}

	protected static void InvokeRpcRpcOvercharge(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOvercharge called on server.");
		}
		else
		{
			((Generator079)obj).RpcOvercharge();
		}
	}

	protected static void InvokeRpcRpcNotify(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcNotify called on server.");
		}
		else
		{
			((Generator079)obj).RpcNotify((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcDoSound(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDoSound called on server.");
		}
		else
		{
			((Generator079)obj).RpcDoSound(reader.ReadBoolean());
		}
	}

	public void CallRpcDenied()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcDenied called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcDenied);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcDenied");
	}

	public void CallRpcOvercharge()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcOvercharge called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcOvercharge);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcOvercharge");
	}

	public void CallRpcNotify(int curr)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcNotify called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcNotify);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)curr);
		SendRPCInternal(networkWriter, 0, "RpcNotify");
	}

	public void CallRpcDoSound(bool isOpen)
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
		networkWriter.Write(isOpen);
		SendRPCInternal(networkWriter, 0, "RpcDoSound");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(isDoorOpen);
			writer.Write(isDoorUnlocked);
			writer.Write(isTabletConnected);
			writer.Write(remainingPowerup);
			GeneratedNetworkCode._WriteOffset_None(writer, position);
			writer.WritePackedUInt32((uint)totalVoltage);
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
			writer.Write(isDoorOpen);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(isDoorUnlocked);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(isTabletConnected);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(remainingPowerup);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			GeneratedNetworkCode._WriteOffset_None(writer, position);
		}
		if ((base.syncVarDirtyBits & 0x20) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)totalVoltage);
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
			isDoorOpen = reader.ReadBoolean();
			isDoorUnlocked = reader.ReadBoolean();
			isTabletConnected = reader.ReadBoolean();
			remainingPowerup = reader.ReadSingle();
			position = GeneratedNetworkCode._ReadOffset_None(reader);
			totalVoltage = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			isDoorOpen = reader.ReadBoolean();
		}
		if ((num & 2) != 0)
		{
			isDoorUnlocked = reader.ReadBoolean();
		}
		if ((num & 4) != 0)
		{
			isTabletConnected = reader.ReadBoolean();
		}
		if ((num & 8) != 0)
		{
			SetTime(reader.ReadSingle());
		}
		if ((num & 0x10) != 0)
		{
			SetOffset(GeneratedNetworkCode._ReadOffset_None(reader));
		}
		if ((num & 0x20) != 0)
		{
			SetTotal((int)reader.ReadPackedUInt32());
		}
	}
}
