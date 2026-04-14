using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MEC;
using UnityEngine;
using UnityEngine.Networking;

public class AlphaWarheadController : NetworkBehaviour
{
	[Serializable]
	public class DetonationScenario
	{
		public AudioClip clip;

		public int tMinusTime;

		public float additionalTime;

		public float SumTime()
		{
			return (float)tMinusTime + additionalTime;
		}
	}

	public DetonationScenario[] scenarios_start;

	public DetonationScenario[] scenarios_resume;

	public AudioClip sound_canceled;

	internal BlastDoor[] blastDoors;

	public bool doorsClosed;

	public bool doorsOpen;

	public bool detonated;

	public int cooldown = 30;

	public int warheadKills;

	private static int _startScenario;

	private static int _resumeScenario;

	private float _shake;

	public static AudioSource alarmSource;

	public static AlphaWarheadController host;

	[SyncVar(hook = "SetTime")]
	public float timeToDetonation;

	[SyncVar(hook = "SetStartScenario")]
	public int sync_startScenario;

	[SyncVar(hook = "SetResumeScenario")]
	public int sync_resumeScenario = -1;

	[SyncVar(hook = "SetProgress")]
	public bool inProgress;

	private string file;

	private static int kRpcRpcShake;

	public float NetworktimeToDetonation
	{
		get
		{
			return timeToDetonation;
		}
		[param: In]
		set
		{
			AlphaWarheadController alphaWarheadController = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetTime(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref alphaWarheadController.timeToDetonation, 1u);
		}
	}

	public int Networksync_startScenario
	{
		get
		{
			return sync_startScenario;
		}
		[param: In]
		set
		{
			AlphaWarheadController alphaWarheadController = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetStartScenario(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref alphaWarheadController.sync_startScenario, 2u);
		}
	}

	public int Networksync_resumeScenario
	{
		get
		{
			return sync_resumeScenario;
		}
		[param: In]
		set
		{
			AlphaWarheadController alphaWarheadController = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetResumeScenario(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref alphaWarheadController.sync_resumeScenario, 4u);
		}
	}

	public bool NetworkinProgress
	{
		get
		{
			return inProgress;
		}
		[param: In]
		set
		{
			AlphaWarheadController alphaWarheadController = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetProgress(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref alphaWarheadController.inProgress, 8u);
		}
	}

	private void Start()
	{
		if (!base.isLocalPlayer || TutorialManager.status)
		{
			return;
		}
		Timing.RunCoroutine(_ReadCustomTranslations(), Segment.FixedUpdate);
		alarmSource = GameObject.Find("GameManager").GetComponent<AudioSource>();
		blastDoors = UnityEngine.Object.FindObjectsOfType<BlastDoor>();
		if (!base.isServer)
		{
			return;
		}
		int value = ConfigFile.ServerConfig.GetInt("warhead_tminus_start_duration", 90);
		value = Mathf.Clamp(value, 80, 120);
		float f = value / 10;
		value = Mathf.RoundToInt(f);
		value *= 10;
		Networksync_startScenario = 3;
		for (int i = 0; i < scenarios_start.Length; i++)
		{
			if (scenarios_start[i].tMinusTime == value)
			{
				Networksync_startScenario = i;
			}
		}
	}

	private void SetTime(float f)
	{
		NetworktimeToDetonation = f;
	}

	private void SetStartScenario(int i)
	{
		Networksync_startScenario = i;
	}

	private void SetResumeScenario(int i)
	{
		Networksync_resumeScenario = i;
	}

	private void SetProgress(bool b)
	{
		NetworkinProgress = b;
	}

	public void StartDetonation()
	{
		if (!Recontainer079.isLocked)
		{
			doorsOpen = false;
			ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Countdown started.", ServerLogs.ServerLogType.GameEvent);
			if ((_resumeScenario == -1 && scenarios_start[_startScenario].SumTime() == timeToDetonation) || (_resumeScenario != -1 && scenarios_resume[_resumeScenario].SumTime() == timeToDetonation))
			{
				SetProgress(true);
			}
		}
	}

	public void InstantPrepare()
	{
		NetworktimeToDetonation = ((_resumeScenario != -1) ? scenarios_resume[_resumeScenario].SumTime() : scenarios_start[_startScenario].SumTime());
	}

	private IEnumerator<float> _ReadCustomTranslations()
	{
		DetonationScenario[] array = scenarios_resume;
		foreach (DetonationScenario asource in array)
		{
			string path = TranslationReader.path + "/Custom Audio/" + asource.clip.name + ".ogg";
			if (!File.Exists(path))
			{
				yield break;
			}
			if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
			{
				file = "file:///";
			}
			else
			{
				file = "file://";
			}
			using (WWW www = new WWW(file + path))
			{
				asource.clip = www.GetAudioClip(false);
				while (asource.clip.loadState != AudioDataLoadState.Loaded)
				{
					yield return Timing.WaitUntilDone(www);
				}
			}
			asource.clip.name = Path.GetFileName(path);
		}
		DetonationScenario[] array2 = scenarios_start;
		foreach (DetonationScenario asource2 in array2)
		{
			string path2 = TranslationReader.path + "/Custom Audio/" + asource2.clip.name + ".ogg";
			if (!File.Exists(path2))
			{
				break;
			}
			if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
			{
				file = "file:///";
			}
			else
			{
				file = "file://";
			}
			using (WWW www2 = new WWW(file + path2))
			{
				asource2.clip = www2.GetAudioClip(false);
				while (asource2.clip.loadState != AudioDataLoadState.Loaded)
				{
					yield return Timing.WaitUntilDone(www2);
				}
			}
			asource2.clip.name = Path.GetFileName(path2);
		}
	}

	public void CancelDetonation()
	{
		CancelDetonation(null);
	}

	public void CancelDetonation(GameObject disabler)
	{
		ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Detonation cancelled.", ServerLogs.ServerLogType.GameEvent);
		if (!inProgress || !(timeToDetonation > 10f))
		{
			return;
		}
		if (timeToDetonation <= 15f && disabler != null)
		{
			GetComponent<PlayerStats>().CallTargetAchieve(disabler.GetComponent<NetworkIdentity>().connectionToClient, "thatwasclose");
		}
		for (int i = 0; i < scenarios_resume.Length; i++)
		{
			if (scenarios_resume[i].SumTime() > timeToDetonation && scenarios_resume[i].SumTime() < scenarios_start[_startScenario].SumTime())
			{
				Networksync_resumeScenario = i;
			}
		}
		SetTime(((_resumeScenario >= 0) ? scenarios_resume[_resumeScenario].SumTime() : scenarios_start[_startScenario].SumTime()) + (float)cooldown);
		SetProgress(false);
		Door[] array = UnityEngine.Object.FindObjectsOfType<Door>();
		foreach (Door door in array)
		{
			door.warheadlock = false;
			door.UpdateLock();
		}
	}

	internal void Detonate()
	{
		ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Warhead detonated.", ServerLogs.ServerLogType.GameEvent);
		detonated = true;
		CallRpcShake();
		GameObject[] array = GameObject.FindGameObjectsWithTag("LiftTarget");
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			GameObject[] array2 = array;
			foreach (GameObject gameObject2 in array2)
			{
				if (gameObject.GetComponent<PlayerStats>().Explode(Vector3.Distance(gameObject2.transform.position, gameObject.transform.position) < 3.5f))
				{
					warheadKills++;
				}
			}
		}
		Door[] array3 = UnityEngine.Object.FindObjectsOfType<Door>();
		foreach (Door door in array3)
		{
			if (door.blockAfterDetonation)
			{
				door.OpenWarhead(true, true);
			}
		}
	}

	[ClientRpc]
	private void RpcShake()
	{
		ExplosionCameraShake.singleton.Shake(1f);
		if (PlayerManager.localPlayer.transform.position.y > 900f)
		{
			AchievementManager.Achieve("tminus");
		}
	}

	private void FixedUpdate()
	{
		if (base.name == "Host")
		{
			host = this;
			_startScenario = sync_startScenario;
			_resumeScenario = sync_resumeScenario;
		}
		if (!(host == null) && base.isLocalPlayer)
		{
			UpdateSourceState();
			if (base.isServer)
			{
				ServerCountdown();
			}
		}
	}

	private void UpdateSourceState()
	{
		if (TutorialManager.status)
		{
			return;
		}
		if (host.inProgress)
		{
			if (host.timeToDetonation != 0f)
			{
				if (!alarmSource.isPlaying)
				{
					alarmSource.volume = 1f;
					alarmSource.clip = ((_resumeScenario >= 0) ? scenarios_resume[_resumeScenario].clip : scenarios_start[_startScenario].clip);
					alarmSource.Play();
					return;
				}
				float num = RealDetonationTime();
				float num2 = num - host.timeToDetonation;
				if (Mathf.Abs(alarmSource.time - num2) > 0.5f)
				{
					alarmSource.time = Mathf.Clamp(num2, 0f, num);
				}
			}
			if (host.timeToDetonation < 5f && host.timeToDetonation != 0f)
			{
				_shake += Time.fixedDeltaTime / 20f;
				_shake = Mathf.Clamp(_shake, 0f, 0.5f);
				if (Vector3.Distance(base.transform.position, AlphaWarheadOutsitePanel.nukeside.transform.position) < 100f)
				{
					ExplosionCameraShake.singleton.Shake(_shake);
				}
			}
		}
		else if (alarmSource.isPlaying && alarmSource.clip != null)
		{
			alarmSource.Stop();
			alarmSource.clip = null;
			alarmSource.PlayOneShot(sound_canceled);
		}
	}

	public float RealDetonationTime()
	{
		return (_resumeScenario < 0) ? scenarios_start[_startScenario].SumTime() : scenarios_resume[_resumeScenario].SumTime();
	}

	[ServerCallback]
	private void ServerCountdown()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		float num = RealDetonationTime();
		float num2 = timeToDetonation;
		if (timeToDetonation != 0f)
		{
			if (inProgress)
			{
				num2 -= Time.fixedDeltaTime;
				if (num2 < 2f && !doorsClosed)
				{
					doorsClosed = true;
					BlastDoor[] array = blastDoors;
					foreach (BlastDoor blastDoor in array)
					{
						blastDoor.SetClosed(true);
					}
				}
				if (ConfigFile.ServerConfig.GetBool("open_doors_on_countdown", true) && !doorsOpen && num2 < num - ((_resumeScenario < 0) ? scenarios_start[_startScenario].additionalTime : scenarios_resume[_resumeScenario].additionalTime))
				{
					doorsOpen = true;
					bool flag = ConfigFile.ServerConfig.GetBool("lock_gates_on_countdown", true);
					bool flag2 = ConfigFile.ServerConfig.GetBool("isolate_zones_on_countdown");
					Door[] array2 = UnityEngine.Object.FindObjectsOfType<Door>();
					foreach (Door door in array2)
					{
						if (flag2 && door.DoorName.Contains("CHECKPOINT"))
						{
							door.warheadlock = true;
							door.UpdateLock();
							door.SetStateWithSound(false);
						}
						else
						{
							door.OpenWarhead(false, flag || !door.DoorName.Contains("GATE"));
						}
					}
				}
				if (num2 <= 0f)
				{
					Detonate();
				}
				num2 = Mathf.Clamp(num2, 0f, num);
			}
			else
			{
				if (num2 > num)
				{
					num2 -= Time.fixedDeltaTime;
				}
				num2 = Mathf.Clamp(num2, num, (float)cooldown + num);
			}
		}
		if (num2 != timeToDetonation)
		{
			SetTime(num2);
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcShake(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShake called on server.");
		}
		else
		{
			((AlphaWarheadController)obj).RpcShake();
		}
	}

	public void CallRpcShake()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcShake called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcShake);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcShake");
	}

	static AlphaWarheadController()
	{
		kRpcRpcShake = -737840022;
		NetworkBehaviour.RegisterRpcDelegate(typeof(AlphaWarheadController), kRpcRpcShake, InvokeRpcRpcShake);
		NetworkCRC.RegisterBehaviour("AlphaWarheadController", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(timeToDetonation);
			writer.WritePackedUInt32((uint)sync_startScenario);
			writer.WritePackedUInt32((uint)sync_resumeScenario);
			writer.Write(inProgress);
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
			writer.Write(timeToDetonation);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)sync_startScenario);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)sync_resumeScenario);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(inProgress);
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
			timeToDetonation = reader.ReadSingle();
			sync_startScenario = (int)reader.ReadPackedUInt32();
			sync_resumeScenario = (int)reader.ReadPackedUInt32();
			inProgress = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetTime(reader.ReadSingle());
		}
		if ((num & 2) != 0)
		{
			SetStartScenario((int)reader.ReadPackedUInt32());
		}
		if ((num & 4) != 0)
		{
			SetResumeScenario((int)reader.ReadPackedUInt32());
		}
		if ((num & 8) != 0)
		{
			SetProgress(reader.ReadBoolean());
		}
	}
}
