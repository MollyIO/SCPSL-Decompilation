using System.Linq;
using GameConsole;
using MEC;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerInteract : NetworkBehaviour
{
	public GameObject playerCamera;

	public LayerMask mask;

	public float raycastMaxDistance;

	private CharacterClassManager _ccm;

	private ServerRoles _sr;

	private Inventory _inv;

	private string uiToggleKey = "numlock";

	private bool enableUiToggle;

	private static int kCmdCmdUse914;

	private static int kCmdCmdUseGenerator;

	private static int kCmdCmdChange914knob;

	private static int kRpcRpcUse914;

	private static int kCmdCmdUseWorkStation_Place;

	private static int kCmdCmdUseWorkStation_Take;

	private static int kCmdCmdUsePanel;

	private static int kRpcRpcLeverSound;

	private static int kCmdCmdUseElevator;

	private static int kCmdCmdSwitchAWButton;

	private static int kCmdCmdDetonateWarhead;

	private static int kCmdCmdOpenDoor;

	private static int kRpcRpcDenied;

	private static int kCmdCmdContain106;

	private static int kRpcRpcContain106;

	private void Start()
	{
		_ccm = GetComponent<CharacterClassManager>();
		_sr = GetComponent<ServerRoles>();
		_inv = GetComponent<Inventory>();
		if (base.isLocalPlayer)
		{
			enableUiToggle = ConfigFile.ServerConfig.GetBool("enable_ui_toggle");
			uiToggleKey = ConfigFile.ServerConfig.GetString("ui_toggle_key", "numlock");
		}
	}

	private void Update()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		if (enableUiToggle && Input.GetKeyDown(uiToggleKey))
		{
			Console.singleton.AddLog("UI toggled. Press " + uiToggleKey + " to toggle it.", Color.yellow);
			GameObject gameObject = GameObject.Find("Player Crosshair Canvas");
			if (gameObject != null)
			{
				gameObject.GetComponent<Canvas>().enabled = !gameObject.GetComponent<Canvas>().enabled;
			}
			GameObject gameObject2 = GameObject.Find("Player Canvas");
			if (gameObject2 != null)
			{
				gameObject2.GetComponent<Canvas>().enabled = !gameObject2.GetComponent<Canvas>().enabled;
			}
		}
		RaycastHit hitInfo;
		if (!Input.GetKeyDown(NewInput.GetKey("Interact")) || GetComponent<CharacterClassManager>().curClass == 2 || !Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hitInfo, raycastMaxDistance, mask))
		{
			return;
		}
		if (hitInfo.transform.GetComponentInParent<Door>() != null)
		{
			CallCmdOpenDoor(hitInfo.transform.GetComponentInParent<Door>().gameObject);
		}
		else if (hitInfo.transform.CompareTag("AW_Button"))
		{
			if (_inv.curItem != 0 && _inv.availableItems[Mathf.Clamp(_inv.curItem, 0, _inv.availableItems.Length - 1)].permissions.Any((string item) => item == "CONT_LVL_3"))
			{
				CallCmdSwitchAWButton();
				return;
			}
			GameObject.Find("Keycard Denied Text").GetComponent<Text>().enabled = true;
			Invoke("DisableDeniedText", 1f);
		}
		else if (hitInfo.transform.CompareTag("AW_Detonation"))
		{
			if (AlphaWarheadOutsitePanel.nukeside.enabled && !AlphaWarheadController.host.inProgress)
			{
				CallCmdDetonateWarhead();
			}
		}
		else if (hitInfo.transform.CompareTag("AW_Panel"))
		{
			CallCmdUsePanel(hitInfo.transform.name);
		}
		else if (hitInfo.transform.CompareTag("914_use"))
		{
			CallCmdUse914();
		}
		else if (hitInfo.transform.CompareTag("914_knob"))
		{
			CallCmdChange914knob();
		}
		else if (hitInfo.transform.CompareTag("ElevatorButton"))
		{
			Lift[] array = Object.FindObjectsOfType<Lift>();
			foreach (Lift lift in array)
			{
				Lift.Elevator[] elevators = lift.elevators;
				for (int num2 = 0; num2 < elevators.Length; num2++)
				{
					Lift.Elevator elevator = elevators[num2];
					if (ChckDis(elevator.door.transform.position))
					{
						CallCmdUseElevator(lift.transform.gameObject);
					}
				}
			}
		}
		else if (hitInfo.collider.name.StartsWith("EPS_"))
		{
			CallCmdUseGenerator(hitInfo.collider.name, hitInfo.collider.GetComponentInParent<Generator079>().gameObject);
		}
		else if (hitInfo.transform.CompareTag("FemurBreaker"))
		{
			CallCmdContain106();
		}
		else if (hitInfo.collider.CompareTag("WS"))
		{
			hitInfo.collider.GetComponentInParent<WorkStation>().UseButton(hitInfo.collider.GetComponent<Button>());
		}
	}

	[Command(channel = 4)]
	private void CmdUse914()
	{
		if (!Scp914.singleton.working && ChckDis(GameObject.FindGameObjectWithTag("914_use").transform.position))
		{
			CallRpcUse914();
		}
	}

	[Command(channel = 4)]
	private void CmdUseGenerator(string command, GameObject go)
	{
		if (!(go == null) && !(go.GetComponent<Generator079>() == null))
		{
			if (ChckDis(go.transform.position))
			{
				go.GetComponent<Generator079>().Interact(base.gameObject, command);
			}
			else
			{
				Debug.Log("Command aborted");
			}
		}
	}

	[Command(channel = 4)]
	private void CmdChange914knob()
	{
		if (!Scp914.singleton.working && ChckDis(GameObject.FindGameObjectWithTag("914_use").transform.position))
		{
			Scp914.singleton.ChangeKnobStatus();
		}
	}

	[ClientRpc(channel = 4)]
	private void RpcUse914()
	{
		Scp914.singleton.StartRefining();
	}

	[Command(channel = 4)]
	public void CmdUseWorkStation_Place(GameObject station)
	{
		if (ChckDis(station.transform.position))
		{
			station.GetComponent<WorkStation>().ConnectTablet(base.gameObject);
		}
	}

	[Command(channel = 4)]
	public void CmdUseWorkStation_Take(GameObject station)
	{
		if (ChckDis(station.transform.position))
		{
			station.GetComponent<WorkStation>().UnconnectTablet(base.gameObject);
		}
	}

	[Command(channel = 4)]
	private void CmdUsePanel(string n)
	{
		AlphaWarheadNukesitePanel nukeside = AlphaWarheadOutsitePanel.nukeside;
		if (ChckDis(nukeside.transform.position))
		{
			if (n.Contains("cancel"))
			{
				AlphaWarheadController.host.CancelDetonation(base.gameObject);
				ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Player " + GetComponent<NicknameSync>().myNick + " (" + GetComponent<CharacterClassManager>().SteamId + ") cancelled the Alpha Warhead detonation.", ServerLogs.ServerLogType.GameEvent);
			}
			else if (n.Contains("lever") && nukeside.AllowChangeLevelState())
			{
				nukeside.Networkenabled = !nukeside.enabled;
				CallRpcLeverSound();
				ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Player " + GetComponent<NicknameSync>().myNick + " (" + GetComponent<CharacterClassManager>().SteamId + ") set the Alpha Warhead status to " + nukeside.enabled + ".", ServerLogs.ServerLogType.GameEvent);
			}
		}
	}

	[ClientRpc(channel = 4)]
	private void RpcLeverSound()
	{
		AlphaWarheadOutsitePanel.nukeside.lever.GetComponent<AudioSource>().Play();
	}

	[Command(channel = 4)]
	private void CmdUseElevator(GameObject elevator)
	{
		Lift.Elevator[] elevators = elevator.GetComponent<Lift>().elevators;
		for (int i = 0; i < elevators.Length; i++)
		{
			Lift.Elevator elevator2 = elevators[i];
			if (ChckDis(elevator2.door.transform.position))
			{
				elevator.GetComponent<Lift>().UseLift();
			}
		}
	}

	[Command(channel = 4)]
	private void CmdSwitchAWButton()
	{
		GameObject gameObject = GameObject.Find("OutsitePanelScript");
		if (ChckDis(gameObject.transform.position) && _inv.availableItems[_inv.curItem].permissions.Any((string item) => item == "CONT_LVL_3"))
		{
			gameObject.GetComponentInParent<AlphaWarheadOutsitePanel>().SetKeycardState(true);
		}
	}

	[Command(channel = 4)]
	private void CmdDetonateWarhead()
	{
		GameObject gameObject = GameObject.Find("OutsitePanelScript");
		if (ChckDis(gameObject.transform.position) && AlphaWarheadOutsitePanel.nukeside.enabled && gameObject.GetComponent<AlphaWarheadOutsitePanel>().keycardEntered)
		{
			AlphaWarheadController.host.StartDetonation();
			ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Player " + GetComponent<NicknameSync>().myNick + " (" + GetComponent<CharacterClassManager>().SteamId + ") started the Alpha Warhead detonation.", ServerLogs.ServerLogType.GameEvent);
		}
	}

	[Command(channel = 14)]
	private void CmdOpenDoor(GameObject doorId)
	{
		Door door = doorId.GetComponent<Door>();
		if (!((door.buttons.Count != 0) ? door.buttons.Any((GameObject item) => ChckDis(item.transform.position)) : ChckDis(doorId.transform.position)))
		{
			return;
		}
		Scp096PlayerScript component = GetComponent<Scp096PlayerScript>();
		if (door.destroyedPrefab != null && (!door.isOpen || door.curCooldown > 0f) && component.iAm096 && component.enraged == Scp096PlayerScript.RageState.Enraged)
		{
			if (!door.locked || _sr.BypassMode)
			{
				door.DestroyDoor(true);
			}
			return;
		}
		if (_sr.BypassMode)
		{
			door.ChangeState(true);
			return;
		}
		if (door.permissionLevel.ToUpper() == "CHCKPOINT_ACC" && GetComponent<CharacterClassManager>().klasy[GetComponent<CharacterClassManager>().curClass].team == Team.SCP)
		{
			door.ChangeState();
			return;
		}
		try
		{
			if (string.IsNullOrEmpty(door.permissionLevel))
			{
				if (!door.locked)
				{
					door.ChangeState();
				}
			}
			else if (_inv.availableItems[_inv.curItem].permissions.Any((string item) => item == door.permissionLevel))
			{
				if (!door.locked)
				{
					door.ChangeState();
				}
				else
				{
					CallRpcDenied(doorId);
				}
			}
			else
			{
				CallRpcDenied(doorId);
			}
		}
		catch
		{
			CallRpcDenied(doorId);
		}
	}

	[ClientRpc(channel = 14)]
	private void RpcDenied(GameObject door)
	{
		Timing.RunCoroutine(door.GetComponent<Door>()._Denied(), Segment.Update);
	}

	private bool ChckDis(Vector3 pos, float distanceMultiplier = 1f)
	{
		if (TutorialManager.status)
		{
			return true;
		}
		return Vector3.Distance(GetComponent<PlyMovementSync>().position, pos) < raycastMaxDistance * 1.5f;
	}

	[Command(channel = 4)]
	private void CmdContain106()
	{
		if (!Object.FindObjectOfType<LureSubjectContainer>().allowContain || (_ccm.klasy[_ccm.curClass].team == Team.SCP && _ccm.curClass != 3) || !ChckDis(GameObject.FindGameObjectWithTag("FemurBreaker").transform.position) || Object.FindObjectOfType<OneOhSixContainer>().used || _ccm.klasy[_ccm.curClass].team == Team.RIP)
		{
			return;
		}
		bool flag = false;
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			if (gameObject.GetComponent<CharacterClassManager>().GodMode && gameObject.GetComponent<CharacterClassManager>().curClass == 3)
			{
				flag = true;
			}
		}
		if (flag)
		{
			return;
		}
		GameObject[] players2 = PlayerManager.singleton.players;
		foreach (GameObject gameObject2 in players2)
		{
			if (gameObject2.GetComponent<CharacterClassManager>().curClass == 3)
			{
				gameObject2.GetComponent<Scp106PlayerScript>().Contain(_ccm);
			}
		}
		CallRpcContain106(base.gameObject);
		Object.FindObjectOfType<OneOhSixContainer>().SetState(true);
	}

	[ClientRpc(channel = 4)]
	private void RpcContain106(GameObject executor)
	{
		Object.Instantiate(GetComponent<Scp106PlayerScript>().screamsPrefab);
		if (executor != base.gameObject)
		{
			return;
		}
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			if (gameObject.GetComponent<CharacterClassManager>().curClass == 3)
			{
				AchievementManager.Achieve("securecontainprotect");
			}
		}
	}

	private void DisableDeniedText()
	{
		GameObject.Find("Keycard Denied Text").GetComponent<Text>().enabled = false;
		HintManager.singleton.AddHint(1);
	}

	private void DisableAlphaText()
	{
		GameObject.Find("Alpha Denied Text").GetComponent<Text>().enabled = false;
		HintManager.singleton.AddHint(2);
	}

	private void DisableLockText()
	{
		GameObject.Find("Lock Denied Text").GetComponent<Text>().enabled = false;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdUse914(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUse914 called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdUse914();
		}
	}

	protected static void InvokeCmdCmdUseGenerator(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUseGenerator called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdUseGenerator(reader.ReadString(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdChange914knob(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChange914knob called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdChange914knob();
		}
	}

	protected static void InvokeCmdCmdUseWorkStation_Place(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUseWorkStation_Place called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdUseWorkStation_Place(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdUseWorkStation_Take(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUseWorkStation_Take called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdUseWorkStation_Take(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdUsePanel(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUsePanel called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdUsePanel(reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdUseElevator(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUseElevator called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdUseElevator(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdSwitchAWButton(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSwitchAWButton called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdSwitchAWButton();
		}
	}

	protected static void InvokeCmdCmdDetonateWarhead(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDetonateWarhead called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdDetonateWarhead();
		}
	}

	protected static void InvokeCmdCmdOpenDoor(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOpenDoor called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdOpenDoor(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdContain106(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdContain106 called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdContain106();
		}
	}

	public void CallCmdUse914()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUse914 called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUse914();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUse914);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 4, "CmdUse914");
	}

	public void CallCmdUseGenerator(string command, GameObject go)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUseGenerator called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUseGenerator(command, go);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUseGenerator);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(command);
		networkWriter.Write(go);
		SendCommandInternal(networkWriter, 4, "CmdUseGenerator");
	}

	public void CallCmdChange914knob()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdChange914knob called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdChange914knob();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdChange914knob);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 4, "CmdChange914knob");
	}

	public void CallCmdUseWorkStation_Place(GameObject station)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUseWorkStation_Place called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUseWorkStation_Place(station);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUseWorkStation_Place);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(station);
		SendCommandInternal(networkWriter, 4, "CmdUseWorkStation_Place");
	}

	public void CallCmdUseWorkStation_Take(GameObject station)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUseWorkStation_Take called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUseWorkStation_Take(station);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUseWorkStation_Take);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(station);
		SendCommandInternal(networkWriter, 4, "CmdUseWorkStation_Take");
	}

	public void CallCmdUsePanel(string n)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUsePanel called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUsePanel(n);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUsePanel);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(n);
		SendCommandInternal(networkWriter, 4, "CmdUsePanel");
	}

	public void CallCmdUseElevator(GameObject elevator)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUseElevator called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUseElevator(elevator);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUseElevator);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(elevator);
		SendCommandInternal(networkWriter, 4, "CmdUseElevator");
	}

	public void CallCmdSwitchAWButton()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSwitchAWButton called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSwitchAWButton();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSwitchAWButton);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 4, "CmdSwitchAWButton");
	}

	public void CallCmdDetonateWarhead()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdDetonateWarhead called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdDetonateWarhead();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdDetonateWarhead);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 4, "CmdDetonateWarhead");
	}

	public void CallCmdOpenDoor(GameObject doorId)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdOpenDoor called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdOpenDoor(doorId);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdOpenDoor);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(doorId);
		SendCommandInternal(networkWriter, 14, "CmdOpenDoor");
	}

	public void CallCmdContain106()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdContain106 called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdContain106();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdContain106);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 4, "CmdContain106");
	}

	protected static void InvokeRpcRpcUse914(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUse914 called on server.");
		}
		else
		{
			((PlayerInteract)obj).RpcUse914();
		}
	}

	protected static void InvokeRpcRpcLeverSound(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcLeverSound called on server.");
		}
		else
		{
			((PlayerInteract)obj).RpcLeverSound();
		}
	}

	protected static void InvokeRpcRpcDenied(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDenied called on server.");
		}
		else
		{
			((PlayerInteract)obj).RpcDenied(reader.ReadGameObject());
		}
	}

	protected static void InvokeRpcRpcContain106(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcContain106 called on server.");
		}
		else
		{
			((PlayerInteract)obj).RpcContain106(reader.ReadGameObject());
		}
	}

	public void CallRpcUse914()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcUse914 called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcUse914);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 4, "RpcUse914");
	}

	public void CallRpcLeverSound()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcLeverSound called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcLeverSound);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 4, "RpcLeverSound");
	}

	public void CallRpcDenied(GameObject door)
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
		networkWriter.Write(door);
		SendRPCInternal(networkWriter, 14, "RpcDenied");
	}

	public void CallRpcContain106(GameObject executor)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcContain106 called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcContain106);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(executor);
		SendRPCInternal(networkWriter, 4, "RpcContain106");
	}

	static PlayerInteract()
	{
		kCmdCmdUse914 = -1419322708;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdUse914, InvokeCmdCmdUse914);
		kCmdCmdUseGenerator = -2072146621;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdUseGenerator, InvokeCmdCmdUseGenerator);
		kCmdCmdChange914knob = -845424245;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdChange914knob, InvokeCmdCmdChange914knob);
		kCmdCmdUseWorkStation_Place = 1646281979;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdUseWorkStation_Place, InvokeCmdCmdUseWorkStation_Place);
		kCmdCmdUseWorkStation_Take = -1055163885;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdUseWorkStation_Take, InvokeCmdCmdUseWorkStation_Take);
		kCmdCmdUsePanel = 1853207668;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdUsePanel, InvokeCmdCmdUsePanel);
		kCmdCmdUseElevator = 339400830;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdUseElevator, InvokeCmdCmdUseElevator);
		kCmdCmdSwitchAWButton = -710673229;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdSwitchAWButton, InvokeCmdCmdSwitchAWButton);
		kCmdCmdDetonateWarhead = -151679759;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdDetonateWarhead, InvokeCmdCmdDetonateWarhead);
		kCmdCmdOpenDoor = 1645579471;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdOpenDoor, InvokeCmdCmdOpenDoor);
		kCmdCmdContain106 = 1084648090;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdContain106, InvokeCmdCmdContain106);
		kRpcRpcUse914 = -637254142;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerInteract), kRpcRpcUse914, InvokeRpcRpcUse914);
		kRpcRpcLeverSound = -829118990;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerInteract), kRpcRpcLeverSound, InvokeRpcRpcLeverSound);
		kRpcRpcDenied = -1136563096;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerInteract), kRpcRpcDenied, InvokeRpcRpcDenied);
		kRpcRpcContain106 = -1051575568;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerInteract), kRpcRpcContain106, InvokeRpcRpcContain106);
		NetworkCRC.RegisterBehaviour("PlayerInteract", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
