using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MEC;
using UnityEngine;
using UnityEngine.Networking;

public class MTFRespawn : NetworkBehaviour
{
	public GameObject ciTheme;

	private ChopperAutostart mtf_a;

	private CharacterClassManager _hostCcm;

	[Range(30f, 1000f)]
	public int minMtfTimeToRespawn = 200;

	[Range(40f, 1200f)]
	public int maxMtfTimeToRespawn = 400;

	public float CI_Time_Multiplier = 2f;

	public float CI_Percent = 20f;

	private float decontaminationCooldown;

	[Space(10f)]
	[Range(2f, 15f)]
	public int maxRespawnAmount = 15;

	public float timeToNextRespawn;

	public bool nextWaveIsCI;

	public List<GameObject> playersToNTF = new List<GameObject>();

	private bool loaded;

	private bool chopperStarted;

	[HideInInspector]
	public float respawnCooldown;

	private static int kRpcRpcPlayAnnouncement;

	private static int kRpcRpcPlayCustomAnnouncement;

	private static int kRpcRpcVan;

	private static int kRpcRpcAnnouncCI;

	private void Start()
	{
		minMtfTimeToRespawn = ConfigFile.ServerConfig.GetInt("minimum_MTF_time_to_spawn", 200);
		maxMtfTimeToRespawn = ConfigFile.ServerConfig.GetInt("maximum_MTF_time_to_spawn", 400);
		CI_Percent = ConfigFile.ServerConfig.GetInt("ci_respawn_percent", 35);
		if (NetworkServer.active && base.isServer && base.isLocalPlayer && !TutorialManager.status)
		{
			Timing.RunCoroutine(_Update(), Segment.FixedUpdate);
		}
	}

	public void SetDecontCooldown(float f)
	{
		decontaminationCooldown = f;
	}

	private void Update()
	{
		if (decontaminationCooldown >= 0f)
		{
			decontaminationCooldown -= Time.deltaTime;
		}
	}

	private IEnumerator<float> _Update()
	{
		_hostCcm = GetComponent<CharacterClassManager>();
		if (!NonFacilityCompatibility.currentSceneSettings.enableRespawning)
		{
			yield break;
		}
		while (this != null)
		{
			if (mtf_a == null)
			{
				mtf_a = Object.FindObjectOfType<ChopperAutostart>();
			}
			if (!_hostCcm.roundStarted)
			{
				yield return 0f;
				continue;
			}
			timeToNextRespawn -= 0.02f;
			if (respawnCooldown >= 0f)
			{
				respawnCooldown -= 0.02f;
			}
			if (timeToNextRespawn < ((!nextWaveIsCI) ? 18f : 13.5f) && !loaded)
			{
				loaded = true;
				GameObject[] players = PlayerManager.singleton.players;
				GameObject[] array = players;
				foreach (GameObject gameObject in array)
				{
					if (gameObject.GetComponent<CharacterClassManager>().curClass == 2)
					{
						chopperStarted = true;
						if (nextWaveIsCI && !AlphaWarheadController.host.detonated)
						{
							SummonVan();
						}
						else
						{
							SummonChopper(true);
						}
						break;
					}
				}
			}
			if (timeToNextRespawn < 0f)
			{
				float maxDelay = 0f;
				if (!nextWaveIsCI && PlayerManager.singleton.players.Where((GameObject item) => item.GetComponent<CharacterClassManager>().curClass == 2 && !item.GetComponent<ServerRoles>().OverwatchEnabled).ToArray().Length > 0)
				{
					bool warheadInProgress;
					bool cassieFree;
					do
					{
						warheadInProgress = AlphaWarheadController.host != null && AlphaWarheadController.host.inProgress && !AlphaWarheadController.host.detonated;
						cassieFree = NineTailedFoxAnnouncer.singleton.isFree && decontaminationCooldown <= 0f;
						yield return 0f;
						maxDelay += 0.02f;
					}
					while (!(maxDelay > 70f) && (!cassieFree || warheadInProgress));
				}
				loaded = false;
				if (GetComponent<CharacterClassManager>().roundStarted)
				{
					SummonChopper(false);
				}
				if (chopperStarted)
				{
					respawnCooldown = 35f;
					RespawnDeadPlayers();
				}
				nextWaveIsCI = (float)Random.Range(0, 100) <= CI_Percent;
				timeToNextRespawn = (float)Random.Range(minMtfTimeToRespawn, maxMtfTimeToRespawn) * ((!nextWaveIsCI) ? 1f : (1f / CI_Time_Multiplier));
				chopperStarted = false;
			}
			yield return 0f;
		}
	}

	private void RespawnDeadPlayers()
	{
		int num = 0;
		List<GameObject> list2;
		if (ConfigFile.ServerConfig.GetBool("priority_mtf_respawn", true))
		{
			List<GameObject> list = (from item in PlayerManager.singleton.players
				where item.GetComponent<CharacterClassManager>().curClass == 2 && !item.GetComponent<ServerRoles>().OverwatchEnabled
				orderby item.GetComponent<CharacterClassManager>().DeathTime
				select item).ToList();
			while (list.Count > maxRespawnAmount)
			{
				list.RemoveAt(list.Count - 1);
			}
			if (ConfigFile.ServerConfig.GetBool("use_crypto_rng"))
			{
				Misc.ShuffleListSecure(list);
			}
			else
			{
				Misc.ShuffleList(list);
			}
			list2 = list;
		}
		else
		{
			List<GameObject> list3 = PlayerManager.singleton.players.Where((GameObject item) => item.GetComponent<CharacterClassManager>().curClass == 2 && !item.GetComponent<ServerRoles>().OverwatchEnabled).ToList();
			if (ConfigFile.ServerConfig.GetBool("use_crypto_rng"))
			{
				Misc.ShuffleListSecure(list3);
			}
			else
			{
				Misc.ShuffleList(list3);
			}
			while (list3.Count > maxRespawnAmount)
			{
				list3.RemoveAt(list3.Count - 1);
			}
			list2 = list3;
		}
		playersToNTF.Clear();
		if (nextWaveIsCI && AlphaWarheadController.host.detonated)
		{
			nextWaveIsCI = false;
		}
		foreach (GameObject item in list2)
		{
			if (!(item == null))
			{
				num++;
				if (nextWaveIsCI)
				{
					GetComponent<CharacterClassManager>().SetPlayersClass(8, item);
					ServerLogs.AddLog(ServerLogs.Modules.ClassChange, item.GetComponent<NicknameSync>().myNick + " (" + item.GetComponent<CharacterClassManager>().SteamId + ") respawned as Chaos Insurgency agent.", ServerLogs.ServerLogType.GameEvent);
				}
				else
				{
					playersToNTF.Add(item);
				}
			}
		}
		if (num > 0)
		{
			ServerLogs.AddLog(ServerLogs.Modules.ClassChange, ((!nextWaveIsCI) ? "MTF" : "Chaos Insurgency") + " respawned!", ServerLogs.ServerLogType.GameEvent);
			if (nextWaveIsCI)
			{
				Invoke("CmdDelayCIAnnounc", 1f);
			}
		}
		SummonNTF();
	}

	[ServerCallback]
	public void SummonNTF()
	{
		if (!NetworkServer.active || playersToNTF.Count <= 0)
		{
			return;
		}
		char letter;
		int number;
		SetUnit(playersToNTF.ToArray(), out letter, out number);
		CallRpcPlayAnnouncement(letter, number, PlayerManager.singleton.players.Where((GameObject item) => item.GetComponent<CharacterClassManager>().IsScpButNotZombie()).ToArray().Length);
		for (int num = 0; num < playersToNTF.Count; num++)
		{
			if (num == 0)
			{
				GetComponent<CharacterClassManager>().SetPlayersClass(12, playersToNTF[num]);
				ServerLogs.AddLog(ServerLogs.Modules.ClassChange, playersToNTF[num].GetComponent<NicknameSync>().myNick + " (" + playersToNTF[num].GetComponent<CharacterClassManager>().SteamId + ") respawned as MTF Commander.", ServerLogs.ServerLogType.GameEvent);
			}
			else if (num <= 3)
			{
				GetComponent<CharacterClassManager>().SetPlayersClass(11, playersToNTF[num]);
				ServerLogs.AddLog(ServerLogs.Modules.ClassChange, playersToNTF[num].GetComponent<NicknameSync>().myNick + " (" + playersToNTF[num].GetComponent<CharacterClassManager>().SteamId + ") respawned as MTF Lieutenant.", ServerLogs.ServerLogType.GameEvent);
			}
			else
			{
				GetComponent<CharacterClassManager>().SetPlayersClass(13, playersToNTF[num]);
				ServerLogs.AddLog(ServerLogs.Modules.ClassChange, playersToNTF[num].GetComponent<NicknameSync>().myNick + " (" + playersToNTF[num].GetComponent<CharacterClassManager>().SteamId + ") respawned as MTF Guard.", ServerLogs.ServerLogType.GameEvent);
			}
		}
		playersToNTF.Clear();
	}

	[ClientRpc]
	private void RpcPlayAnnouncement(char natoLetter, int natoNumber, int scpsLeft)
	{
		NineTailedFoxAnnouncer.singleton.AnnounceNtfEntrance(scpsLeft, natoNumber, natoLetter);
	}

	[ClientRpc]
	public void RpcPlayCustomAnnouncement(string words, bool makeHold)
	{
		NineTailedFoxAnnouncer.singleton.AddPhraseToQueue(words, false, makeHold);
	}

	[ServerCallback]
	private void SetUnit(GameObject[] ply, out char letter, out int number)
	{
		if (!NetworkServer.active)
		{
           letter = '\0';
			number = 0;
			return;
		}
		int unit = GetComponent<NineTailedFoxUnits>().NewName(out number, out letter);
		foreach (GameObject gameObject in ply)
		{
			gameObject.GetComponent<CharacterClassManager>().SetUnit(unit);
		}
	}

	[ServerCallback]
	private void SummonChopper(bool state)
	{
		if (NetworkServer.active && NonFacilityCompatibility.currentSceneSettings.enableStandardGamplayItems)
		{
			mtf_a.SetState(state);
		}
	}

	[ServerCallback]
	private void SummonVan()
	{
		if (NetworkServer.active && NonFacilityCompatibility.currentSceneSettings.enableStandardGamplayItems)
		{
			CallRpcVan();
		}
	}

	[ClientRpc(channel = 2)]
	private void RpcVan()
	{
		GameObject.Find("CIVanArrive").GetComponent<Animator>().SetTrigger("Arrive");
	}

	private void CmdDelayCIAnnounc()
	{
		PlayAnnoncCI();
	}

	[ServerCallback]
	private void PlayAnnoncCI()
	{
		if (NetworkServer.active)
		{
			CallRpcAnnouncCI();
		}
	}

	[ClientRpc(channel = 2)]
	private void RpcAnnouncCI()
	{
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
			if (component.isLocalPlayer)
			{
				Team team = component.klasy[component.curClass].team;
				if (team == Team.CDP || team == Team.CHI || component.GetComponent<ServerRoles>().OverwatchEnabled)
				{
					Object.Instantiate(ciTheme);
				}
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcPlayAnnouncement(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayAnnouncement called on server.");
		}
		else
		{
			((MTFRespawn)obj).RpcPlayAnnouncement((char)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcPlayCustomAnnouncement(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayCustomAnnouncement called on server.");
		}
		else
		{
			((MTFRespawn)obj).RpcPlayCustomAnnouncement(reader.ReadString(), reader.ReadBoolean());
		}
	}

	protected static void InvokeRpcRpcVan(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcVan called on server.");
		}
		else
		{
			((MTFRespawn)obj).RpcVan();
		}
	}

	protected static void InvokeRpcRpcAnnouncCI(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAnnouncCI called on server.");
		}
		else
		{
			((MTFRespawn)obj).RpcAnnouncCI();
		}
	}

	public void CallRpcPlayAnnouncement(char natoLetter, int natoNumber, int scpsLeft)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPlayAnnouncement called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPlayAnnouncement);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32(natoLetter);
		networkWriter.WritePackedUInt32((uint)natoNumber);
		networkWriter.WritePackedUInt32((uint)scpsLeft);
		SendRPCInternal(networkWriter, 0, "RpcPlayAnnouncement");
	}

	public void CallRpcPlayCustomAnnouncement(string words, bool makeHold)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPlayCustomAnnouncement called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPlayCustomAnnouncement);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(words);
		networkWriter.Write(makeHold);
		SendRPCInternal(networkWriter, 0, "RpcPlayCustomAnnouncement");
	}

	public void CallRpcVan()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcVan called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcVan);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 2, "RpcVan");
	}

	public void CallRpcAnnouncCI()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcAnnouncCI called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcAnnouncCI);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 2, "RpcAnnouncCI");
	}

	static MTFRespawn()
	{
		kRpcRpcPlayAnnouncement = 418834810;
		NetworkBehaviour.RegisterRpcDelegate(typeof(MTFRespawn), kRpcRpcPlayAnnouncement, InvokeRpcRpcPlayAnnouncement);
		kRpcRpcPlayCustomAnnouncement = -1255213749;
		NetworkBehaviour.RegisterRpcDelegate(typeof(MTFRespawn), kRpcRpcPlayCustomAnnouncement, InvokeRpcRpcPlayCustomAnnouncement);
		kRpcRpcVan = -871850524;
		NetworkBehaviour.RegisterRpcDelegate(typeof(MTFRespawn), kRpcRpcVan, InvokeRpcRpcVan);
		kRpcRpcAnnouncCI = -699664669;
		NetworkBehaviour.RegisterRpcDelegate(typeof(MTFRespawn), kRpcRpcAnnouncCI, InvokeRpcRpcAnnouncCI);
		NetworkCRC.RegisterBehaviour("MTFRespawn", 0);
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
