using System;
using System.Linq;
using Dissonance.Integrations.UNet_HLAPI;
using RemoteAdmin;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerStats : NetworkBehaviour
{
	[Serializable]
	public struct HitInfo
	{
		public float amount;

		public int tool;

		public int time;

		public string attacker;

		public int plyID;

		public HitInfo(float amnt, string attackerName, DamageTypes.DamageType weapon, int attackerID)
		{
			amount = amnt;
			tool = DamageTypes.ToIndex(weapon);
			attacker = attackerName;
			plyID = attackerID;
			time = ServerTime.time;
		}

		public GameObject GetPlayerObject()
		{
			GameObject[] players = PlayerManager.singleton.players;
			foreach (GameObject gameObject in players)
			{
				if (gameObject.GetComponent<QueryProcessor>().PlayerId == plyID)
				{
					return gameObject;
				}
			}
			return null;
		}

		public DamageTypes.DamageType GetDamageType()
		{
			return DamageTypes.FromIndex(tool);
		}

		public string GetDamageName()
		{
			return DamageTypes.FromIndex(tool).name;
		}
	}

	public HitInfo lastHitInfo = new HitInfo(0f, "NONE", DamageTypes.None, 0);

	public Transform[] grenadePoints;

	public CharacterClassManager ccm;

	private UserMainInterface _ui;

	private static Lift[] _lifts;

	public int maxHP;

	public bool used914;

	private bool _pocketCleanup;

	private bool _allowSPDmg;

	private int _health;

	private bool _hpDirty;

	private float killstreak_time;

	private int killstreak;

	private static int kTargetRpcTargetSyncHp;

	private static int kCmdCmdSelfDeduct;

	private static int kCmdCmdTesla;

	private static int kTargetRpcTargetAchieve;

	private static int kRpcRpcAnnounceScpKill;

	private static int kTargetRpcTargetStats;

	private static int kTargetRpcTargetOofEffect;

	private static int kRpcRpcRoundrestart;

	public int health
	{
		get
		{
			return _health;
		}
		set
		{
			_health = value;
			_hpDirty = true;
		}
	}

	public void MakeHpDirty()
	{
		_hpDirty = true;
	}

	private void Start()
	{
		_pocketCleanup = ConfigFile.ServerConfig.GetBool("SCP106_CLEANUP");
		_allowSPDmg = ConfigFile.ServerConfig.GetBool("spawn_protect_allow_dmg", true);
		ccm = GetComponent<CharacterClassManager>();
		_ui = UserMainInterface.singleton;
		if (_lifts.Length == 0)
		{
			_lifts = UnityEngine.Object.FindObjectsOfType<Lift>();
		}
	}

	private void Update()
	{
		if (base.isLocalPlayer && ccm.curClass != 2)
		{
			_ui.SetHP((health >= 0) ? health : 0, maxHP);
		}
		if (base.isLocalPlayer)
		{
			_ui.hpOBJ.SetActive(ccm.curClass != 2);
		}
		if (!_hpDirty)
		{
			return;
		}
		_hpDirty = false;
		if (NetworkServer.active)
		{
			CallTargetSyncHp(base.connectionToClient, _health);
		}
		foreach (CharacterClassManager item in PlayerManager.singleton.players.Select((GameObject s) => s.GetComponent<CharacterClassManager>()))
		{
			if (item.curClass == 2 && item.IsVerified)
			{
				CallTargetSyncHp(item.connectionToClient, _health);
			}
		}
	}

	[TargetRpc(channel = 2)]
	public void TargetSyncHp(NetworkConnection conn, int hp)
	{
		_health = hp;
	}

	public float GetHealthPercent()
	{
		if (ccm.curClass < 0)
		{
			return 0f;
		}
		return Mathf.Clamp01(1f - (float)health / (float)ccm.klasy[ccm.curClass].maxHP);
	}

	[Command(channel = 2)]
	public void CmdSelfDeduct(HitInfo info)
	{
		HurtPlayer(info, base.gameObject);
	}

	public bool Explode(bool inElevator)
	{
		bool flag = health > 0 && (inElevator || base.transform.position.y < 900f);
		switch (ccm.curClass)
		{
		case 7:
			flag = true;
			break;
		case 3:
		{
			Scp106PlayerScript component = GetComponent<Scp106PlayerScript>();
			if ((object)component != null)
			{
				component.DeletePortal();
			}
			bool? flag2 = (((object)component != null) ? new bool?(component.goingViaThePortal) : ((bool?)null));
			if (flag2.HasValue && flag2.Value)
			{
				flag = true;
			}
			break;
		}
		}
		return flag && HurtPlayer(new HitInfo(-1f, "WORLD", DamageTypes.Nuke, 0), base.gameObject);
	}

	[Command(channel = 2)]
	public void CmdTesla()
	{
		HurtPlayer(new HitInfo(UnityEngine.Random.Range(100, 200), GetComponent<HlapiPlayer>().PlayerId, DamageTypes.Tesla, 0), base.gameObject);
	}

	public void SetHPAmount(int hp)
	{
		health = hp;
	}

	public bool HealHPAmount(int hp)
	{
		int num = Mathf.Clamp(hp, 0, maxHP - health);
		health = ((health + num <= health) ? health : (health + num));
		return num > 0;
	}

	public bool HurtPlayer(HitInfo info, GameObject go)
	{
		bool result = false;
		if (info.amount < 0f)
		{
			int? obj;
			if ((object)go == null)
			{
				obj = null;
			}
			else
			{
				PlayerStats component = go.GetComponent<PlayerStats>();
				obj = (((object)component != null) ? new int?(component.health) : ((int?)null));
			}
			int? num = obj + 1;
			info.amount = Mathf.Abs((!num.HasValue) ? 999999f : ((float)num.Value));
		}
		if (info.amount > 2.1474836E+09f)
		{
			info.amount = 2.1474836E+09f;
		}
		if (go != null)
		{
			PlayerStats component2 = go.GetComponent<PlayerStats>();
			CharacterClassManager component3 = go.GetComponent<CharacterClassManager>();
			if (component3.GodMode)
			{
				return false;
			}
			if (ccm.curClass > -1 && component3.curClass > -1 && ccm.klasy[ccm.curClass].team == Team.SCP && ccm.klasy[component3.curClass].team == Team.SCP && ccm != component3)
			{
				return false;
			}
			if (component3.SpawnProtected && !_allowSPDmg)
			{
				return false;
			}
			if (base.isLocalPlayer && info.plyID != go.GetComponent<QueryProcessor>().PlayerId)
			{
				RoundSummary.Damages += ((!((float)component2.health < info.amount)) ? ((int)info.amount) : component2.health);
			}
			component2.health -= Mathf.CeilToInt(info.amount);
			if (Mathf.CeilToInt(component2.health) < 0)
			{
				component2.health = 0;
			}
			component2.lastHitInfo = info;
			if (component2.health < 1 && component3.curClass != 2)
			{
				foreach (Scp079PlayerScript instance in Scp079PlayerScript.instances)
				{
					Scp079Interactable.ZoneAndRoom otherRoom = go.GetComponent<Scp079PlayerScript>().GetOtherRoom();
					Scp079Interactable.InteractableType[] filter = new Scp079Interactable.InteractableType[5]
					{
						Scp079Interactable.InteractableType.Door,
						Scp079Interactable.InteractableType.Light,
						Scp079Interactable.InteractableType.Lockdown,
						Scp079Interactable.InteractableType.Tesla,
						Scp079Interactable.InteractableType.ElevatorUse
					};
					bool flag = false;
					foreach (Scp079Interaction item in instance.ReturnRecentHistory(12f, filter))
					{
						foreach (Scp079Interactable.ZoneAndRoom currentZonesAndRoom in item.interactable.currentZonesAndRooms)
						{
							if (currentZonesAndRoom.currentZone == otherRoom.currentZone && currentZonesAndRoom.currentRoom == otherRoom.currentRoom)
							{
								flag = true;
							}
						}
					}
					if (flag)
					{
						instance.CallRpcGainExp(ExpGainType.KillAssist, component3.curClass);
					}
				}
				if (RoundSummary.RoundInProgress() && RoundSummary.roundTime < 60)
				{
					CallTargetAchieve(component3.connectionToClient, "wowreally");
				}
				if (base.isLocalPlayer && info.plyID != go.GetComponent<QueryProcessor>().PlayerId)
				{
					RoundSummary.Kills++;
				}
				result = true;
				if (component3.curClass == 9 && go.GetComponent<Scp096PlayerScript>().enraged == Scp096PlayerScript.RageState.Panic)
				{
					CallTargetAchieve(component3.connectionToClient, "unvoluntaryragequit");
				}
				else if (info.GetDamageType() == DamageTypes.Pocket)
				{
					CallTargetAchieve(component3.connectionToClient, "newb");
				}
				else if (info.GetDamageType() == DamageTypes.Scp173)
				{
					CallTargetAchieve(component3.connectionToClient, "firsttime");
				}
				else if (info.GetDamageType() == DamageTypes.Grenade && info.plyID == go.GetComponent<QueryProcessor>().PlayerId)
				{
					CallTargetAchieve(component3.connectionToClient, "iwanttobearocket");
				}
				else if (info.GetDamageType().isWeapon)
				{
					if (component3.curClass == 6 && component3.GetComponent<Inventory>().curItem >= 0 && component3.GetComponent<Inventory>().curItem <= 11 && GetComponent<CharacterClassManager>().curClass == 1)
					{
						CallTargetAchieve(base.connectionToClient, "betrayal");
					}
					if (Time.realtimeSinceStartup - killstreak_time > 30f || killstreak == 0)
					{
						killstreak = 0;
						killstreak_time = Time.realtimeSinceStartup;
					}
					if (GetComponent<WeaponManager>().GetShootPermission(component3, true))
					{
						killstreak++;
					}
					if (killstreak > 5)
					{
						CallTargetAchieve(base.connectionToClient, "pewpew");
					}
					if (ccm.curClass > -1 && (ccm.klasy[ccm.curClass].team == Team.MTF || ccm.klasy[ccm.curClass].team == Team.RSC) && component3.curClass == 1)
					{
						CallTargetStats(base.connectionToClient, "dboys_killed", "justresources", 50);
					}
					if (ccm.curClass > -1 && ccm.klasy[ccm.curClass].team == Team.RSC && component3.curClass > -1 && ccm.klasy[component3.curClass].team == Team.SCP)
					{
						CallTargetAchieve(base.connectionToClient, "timetodoitmyself");
					}
				}
				ServerLogs.AddLog(ServerLogs.Modules.ClassChange, "Player " + go.GetComponent<NicknameSync>().myNick + " (" + go.GetComponent<CharacterClassManager>().SteamId + ") killed by " + info.attacker + " using " + info.GetDamageName() + ".", ServerLogs.ServerLogType.KillLog);
				if (!_pocketCleanup || info.GetDamageType() != DamageTypes.Pocket)
				{
					go.GetComponent<Inventory>().ServerDropAll();
					if (component3.curClass >= 0 && info.GetDamageType() != DamageTypes.RagdollLess)
					{
						GetComponent<RagdollManager>().SpawnRagdoll(go.transform.position, go.transform.rotation, component3.curClass, info, component3.klasy[component3.curClass].team != Team.SCP, go.GetComponent<HlapiPlayer>().PlayerId, go.GetComponent<NicknameSync>().myNick, go.GetComponent<QueryProcessor>().PlayerId);
					}
				}
				else
				{
					go.GetComponent<Inventory>().Clear();
				}
				component3.NetworkdeathPosition = go.transform.position;
				if (component3.curClass > -1 && component3.curClass != 10 && component3.klasy[component3.curClass].team == Team.SCP)
				{
					GameObject gameObject = null;
					GameObject[] players = PlayerManager.singleton.players;
					foreach (GameObject gameObject2 in players)
					{
						if (gameObject2.GetComponent<QueryProcessor>().PlayerId == info.plyID)
						{
							gameObject = gameObject2;
						}
					}
					if (gameObject != null)
					{
						CallRpcAnnounceScpKill(component3.klasy[component3.curClass].fullName, gameObject);
					}
					else
					{
						string text = string.Empty;
						if (component3.klasy[component3.curClass].fullName.Contains("-"))
						{
							string text2 = component3.klasy[component3.curClass].fullName.Split('-')[1];
							foreach (char c in text2)
							{
								text = text + c + " ";
							}
						}
						MTFRespawn component4 = PlayerManager.localPlayer.GetComponent<MTFRespawn>();
						DamageTypes.DamageType damageType = info.GetDamageType();
						if (component3.curClass != 7)
						{
							if (damageType == DamageTypes.Tesla)
							{
								component4.CallRpcPlayCustomAnnouncement("SCP " + text + " SUCCESSFULLY TERMINATED BY AUTOMATIC SECURITY SYSTEM", false);
							}
							else if (damageType == DamageTypes.Nuke)
							{
								component4.CallRpcPlayCustomAnnouncement("SCP " + text + " TERMINATED BY ALPHA WARHEAD", false);
							}
							else if (damageType == DamageTypes.Decont)
							{
								component4.CallRpcPlayCustomAnnouncement("SCP " + text + " LOST IN DECONTAMINATION SEQUENCE", false);
							}
							else
							{
								CallRpcAnnounceScpKill(component3.klasy[component3.curClass].fullName, null);
							}
						}
					}
				}
				component2.SetHPAmount(100);
				component3.SetClassID(2);
				if (TutorialManager.status)
				{
					PlayerManager.localPlayer.GetComponent<TutorialManager>().KillNPC();
				}
			}
			else
			{
				Vector3 pos = Vector3.zero;
				float num2 = 40f;
				if (info.GetDamageType().isWeapon)
				{
					GameObject playerOfID = GetPlayerOfID(info.plyID);
					if (playerOfID != null)
					{
						pos = go.transform.InverseTransformPoint(playerOfID.transform.position).normalized;
						num2 = 100f;
					}
				}
				if (component3.curClass > -1 && (component3.curClass == 16 || component3.curClass == 17))
				{
					component3.GetComponent<Scp939PlayerScript>().NetworkspeedMultiplier = 1.25f;
				}
				CallTargetOofEffect(go.GetComponent<NetworkIdentity>().connectionToClient, pos, Mathf.Clamp01(info.amount / num2));
			}
		}
		return result;
	}

	[TargetRpc]
	public void TargetAchieve(NetworkConnection conn, string key)
	{
		AchievementManager.Achieve(key);
	}

	[ClientRpc]
	private void RpcAnnounceScpKill(string scpnum, GameObject exec)
	{
		NineTailedFoxAnnouncer.singleton.AnnounceScpKill(scpnum, (!(exec == null)) ? exec.GetComponent<CharacterClassManager>() : null);
	}

	[TargetRpc]
	public void TargetStats(NetworkConnection conn, string key, string targetAchievement, int maxValue)
	{
		AchievementManager.StatsProgress(key, targetAchievement, maxValue);
	}

	private GameObject GetPlayerOfID(int id)
	{
		return PlayerManager.singleton.players.FirstOrDefault((GameObject ply) => ply.GetComponent<QueryProcessor>().PlayerId == id);
	}

	[TargetRpc]
	private void TargetOofEffect(NetworkConnection conn, Vector3 pos, float overall)
	{
		OOF_Controller.singleton.AddBlood(pos, overall);
	}

	[ClientRpc(channel = 7)]
	private void RpcRoundrestart()
	{
		if (!base.isServer)
		{
			CustomNetworkManager customNetworkManager = UnityEngine.Object.FindObjectOfType<CustomNetworkManager>();
			customNetworkManager.reconnect = true;
			Invoke("ChangeLevel", 0.5f);
		}
	}

	public void Roundrestart()
	{
		CallRpcRoundrestart();
		Invoke("ChangeLevel", 2.5f);
	}

	private void ChangeLevel()
	{
		if (base.isServer)
		{
			GC.Collect();
			NetworkManager.singleton.ServerChangeScene(NetworkManager.singleton.onlineScene);
		}
		else
		{
			NetworkManager.singleton.StopClient();
		}
	}

	public string HealthToString()
	{
		double num = (double)health / (double)maxHP * 100.0;
		return health + "/" + maxHP + "(" + num + "%)";
	}

	static PlayerStats()
	{
		_lifts = new Lift[0];
		kCmdCmdSelfDeduct = -2147454163;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerStats), kCmdCmdSelfDeduct, InvokeCmdCmdSelfDeduct);
		kCmdCmdTesla = -1109720487;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerStats), kCmdCmdTesla, InvokeCmdCmdTesla);
		kRpcRpcAnnounceScpKill = 530564897;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), kRpcRpcAnnounceScpKill, InvokeRpcRpcAnnounceScpKill);
		kRpcRpcRoundrestart = 907411477;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), kRpcRpcRoundrestart, InvokeRpcRpcRoundrestart);
		kTargetRpcTargetSyncHp = -945916362;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), kTargetRpcTargetSyncHp, InvokeRpcTargetSyncHp);
		kTargetRpcTargetAchieve = 1310991230;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), kTargetRpcTargetAchieve, InvokeRpcTargetAchieve);
		kTargetRpcTargetStats = 662062348;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), kTargetRpcTargetStats, InvokeRpcTargetStats);
		kTargetRpcTargetOofEffect = -1463723612;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), kTargetRpcTargetOofEffect, InvokeRpcTargetOofEffect);
		NetworkCRC.RegisterBehaviour("PlayerStats", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSelfDeduct(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSelfDeduct called on client.");
		}
		else
		{
			((PlayerStats)obj).CmdSelfDeduct(GeneratedNetworkCode._ReadHitInfo_PlayerStats(reader));
		}
	}

	protected static void InvokeCmdCmdTesla(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTesla called on client.");
		}
		else
		{
			((PlayerStats)obj).CmdTesla();
		}
	}

	public void CallCmdSelfDeduct(HitInfo info)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSelfDeduct called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSelfDeduct(info);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSelfDeduct);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteHitInfo_PlayerStats(networkWriter, info);
		SendCommandInternal(networkWriter, 2, "CmdSelfDeduct");
	}

	public void CallCmdTesla()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdTesla called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdTesla();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdTesla);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdTesla");
	}

	protected static void InvokeRpcRpcAnnounceScpKill(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAnnounceScpKill called on server.");
		}
		else
		{
			((PlayerStats)obj).RpcAnnounceScpKill(reader.ReadString(), reader.ReadGameObject());
		}
	}

	protected static void InvokeRpcRpcRoundrestart(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRoundrestart called on server.");
		}
		else
		{
			((PlayerStats)obj).RpcRoundrestart();
		}
	}

	protected static void InvokeRpcTargetSyncHp(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSyncHp called on server.");
		}
		else
		{
			((PlayerStats)obj).TargetSyncHp(ClientScene.readyConnection, (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcTargetAchieve(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetAchieve called on server.");
		}
		else
		{
			((PlayerStats)obj).TargetAchieve(ClientScene.readyConnection, reader.ReadString());
		}
	}

	protected static void InvokeRpcTargetStats(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetStats called on server.");
		}
		else
		{
			((PlayerStats)obj).TargetStats(ClientScene.readyConnection, reader.ReadString(), reader.ReadString(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcTargetOofEffect(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetOofEffect called on server.");
		}
		else
		{
			((PlayerStats)obj).TargetOofEffect(ClientScene.readyConnection, reader.ReadVector3(), reader.ReadSingle());
		}
	}

	public void CallRpcAnnounceScpKill(string scpnum, GameObject exec)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcAnnounceScpKill called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcAnnounceScpKill);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(scpnum);
		networkWriter.Write(exec);
		SendRPCInternal(networkWriter, 0, "RpcAnnounceScpKill");
	}

	public void CallRpcRoundrestart()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRoundrestart called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRoundrestart);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 7, "RpcRoundrestart");
	}

	public void CallTargetSyncHp(NetworkConnection conn, int hp)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetSyncHp called on client.");
			return;
		}
       if (conn.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetSyncHp called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetSyncHp);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)hp);
		SendTargetRPCInternal(conn, networkWriter, 2, "TargetSyncHp");
	}

	public void CallTargetAchieve(NetworkConnection conn, string key)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetAchieve called on client.");
			return;
		}
       if (conn.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetAchieve called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetAchieve);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(key);
		SendTargetRPCInternal(conn, networkWriter, 0, "TargetAchieve");
	}

	public void CallTargetStats(NetworkConnection conn, string key, string targetAchievement, int maxValue)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetStats called on client.");
			return;
		}
       if (conn.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetStats called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetStats);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(key);
		networkWriter.Write(targetAchievement);
		networkWriter.WritePackedUInt32((uint)maxValue);
		SendTargetRPCInternal(conn, networkWriter, 0, "TargetStats");
	}

	public void CallTargetOofEffect(NetworkConnection conn, Vector3 pos, float overall)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetOofEffect called on client.");
			return;
		}
       if (conn.connectionId == 0 && !NetworkServer.localClientActive)
		{
			Debug.LogError("TargetRPC Function TargetOofEffect called on connection to server");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetOofEffect);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(pos);
		networkWriter.Write(overall);
		SendTargetRPCInternal(conn, networkWriter, 0, "TargetOofEffect");
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
