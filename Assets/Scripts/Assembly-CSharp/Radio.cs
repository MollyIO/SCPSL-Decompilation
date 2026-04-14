using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dissonance;
using Dissonance.Audio.Playback;
using Dissonance.Integrations.UNet_HLAPI;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

public class Radio : NetworkBehaviour
{
	[Serializable]
	public class VoiceInfo
	{
		public bool isAliveHuman;

		public bool isSCP;

		public bool IsDead()
		{
			return !isSCP && !isAliveHuman;
		}
	}

	[Serializable]
	public class RadioPreset
	{
		public string label;

		public string powerText;

		public float powerTime;

		public AnimationCurve nosie;

		public AnimationCurve volume;

		public float beepRange;
	}

	public static Radio localRadio;

	public AudioMixerGroup g_voice;

	public AudioMixerGroup g_radio;

	public AudioMixerGroup g_icom;

	public AudioMixerGroup g_079;

	public AudioClip[] beepStart;

	public AudioClip[] beepStop;

	public AudioSource beepSource;

	[Space]
	public AudioSource mySource;

	[Space]
	public VoiceInfo voiceInfo;

	public RadioPreset[] presets;

	[SyncVar(hook = "SetPreset")]
	public int curPreset;

	[SyncVar]
	public bool isTransmitting;

	private int myRadio = -1;

	private float timeToNextTransmition;

	private AudioSource noiseSource;

	private int lastPreset;

	private SpeakerIcon icon;

	private static float noiseIntensity;

	public static bool roundStarted;

	public static bool roundEnded;

	private GameObject host;

	public float icomNoise;

	private Inventory inv;

	public float noiseMultiplier;

	private CharacterClassManager ccm;

	private Scp939PlayerScript scp939;

	private Scp079PlayerScript scp079;

	private static DissonanceComms comms;

	private VoicePlayerState state;

	private VoicePlayback unityPlayback;

	private int radioUniq;

	private static int kCmdCmdSyncTransmitionStatus;

	private static int kCmdCmdUpdatePreset;

	public int NetworkcurPreset
	{
		get
		{
			return curPreset;
		}
		[param: In]
		set
		{
			Radio radio = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetPreset(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref radio.curPreset, 1u);
		}
	}

	public bool NetworkisTransmitting
	{
		get
		{
			return isTransmitting;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref isTransmitting, 2u);
		}
	}

	public void ResetPreset()
	{
		NetworkcurPreset = 1;
		CallCmdUpdatePreset(1);
		if (myRadio >= 0)
		{
			return;
		}
		for (int i = 0; i < inv.items.Count; i++)
		{
			if (inv.items[i].id == 12)
			{
				radioUniq = inv.items[i].uniq;
			}
		}
	}

	private void Start()
	{
		if (comms == null)
		{
			comms = UnityEngine.Object.FindObjectOfType<DissonanceComms>();
		}
		ccm = GetComponent<CharacterClassManager>();
		noiseSource = GameObject.Find("RadioNoiseSound").GetComponent<AudioSource>();
		inv = GetComponent<Inventory>();
		scp939 = GetComponent<Scp939PlayerScript>();
		scp079 = GetComponent<Scp079PlayerScript>();
		if (base.isLocalPlayer)
		{
			roundEnded = false;
			localRadio = this;
		}
		if (NetworkServer.active)
		{
			InvokeRepeating("UseBattery", 1f, 1f);
		}
		icon = GetComponentInChildren<SpeakerIcon>();
	}

	public void UpdateClass()
	{
		bool isSCP = false;
		bool isAliveHuman = false;
		if (ccm.curClass >= 0 && ccm.curClass != 2)
		{
			if (ccm.klasy[ccm.curClass].team == Team.SCP)
			{
				isSCP = true;
			}
			else
			{
				isAliveHuman = true;
			}
		}
		voiceInfo.isAliveHuman = isAliveHuman;
		voiceInfo.isSCP = isSCP;
	}

	private void Update()
	{
		if (unityPlayback == null && !string.IsNullOrEmpty(GetComponent<HlapiPlayer>().PlayerId))
		{
			state = comms.FindPlayer(GetComponent<HlapiPlayer>().PlayerId);
			if (((state != null) ? state.Playback : null) != null)
			{
				unityPlayback = (VoicePlayback)state.Playback;
			}
		}
		UpdateClass();
		if (host == null)
		{
			host = GameObject.Find("Host");
		}
		if (inv.GetItemIndex() != -1 && inv.items[inv.GetItemIndex()].id == 12)
		{
			radioUniq = inv.items[inv.GetItemIndex()].uniq;
		}
		myRadio = -1;
		for (int i = 0; i < inv.items.Count; i++)
		{
			if (inv.items[i].uniq == radioUniq)
			{
				myRadio = i;
			}
		}
		if (!base.isLocalPlayer)
		{
			return;
		}
		if (noiseSource.volume > 0f != noiseIntensity > 0f)
		{
			PlayBeepSound(noiseIntensity > 0f);
		}
		noiseSource.volume = noiseIntensity * noiseMultiplier;
		noiseIntensity = 0f;
		GetInput();
		if (myRadio != -1)
		{
			RadioDisplay.battery = Mathf.Clamp(Mathf.CeilToInt(inv.items[myRadio].durability), 0, 100).ToString();
			RadioDisplay.power = presets[curPreset].powerText;
			RadioDisplay.label = presets[curPreset].label;
		}
		using (IEnumerator<Inventory.SyncItemInfo> enumerator = inv.items.GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current.id != 12)
			{
			}
		}
	}

	private void UseBattery()
	{
		if (CheckRadio() && inv.items[myRadio].id == 12)
		{
			float num = inv.items[myRadio].durability - 1.67f * (1f / presets[curPreset].powerTime) * (float)((!isTransmitting) ? 1 : 3);
			if (num > -1f && num < 101f)
			{
				inv.items.ModifyDuration(myRadio, num);
			}
		}
	}

	private void GetInput()
	{
		if (timeToNextTransmition > 0f)
		{
			timeToNextTransmition -= Time.deltaTime;
		}
		bool flag = Input.GetKey(NewInput.GetKey("Voice Chat")) && CheckRadio();
		if (flag != isTransmitting && timeToNextTransmition <= 0f)
		{
			NetworkisTransmitting = flag;
			PlayBeepSound(isTransmitting);
			timeToNextTransmition = 0.5f;
			CallCmdSyncTransmitionStatus(flag, base.transform.position);
		}
		if (inv.curItem != 12 || !(Inventory.inventoryCooldown <= 0f) || Cursor.visible)
		{
			return;
		}
		if (Input.GetKeyDown(NewInput.GetKey("Shoot")) && curPreset != 0)
		{
			NetworkcurPreset = curPreset + 1;
			if (curPreset >= presets.Length)
			{
				NetworkcurPreset = 1;
			}
			lastPreset = curPreset;
			CallCmdUpdatePreset(curPreset);
		}
		if (Input.GetKeyDown(NewInput.GetKey("Zoom")))
		{
			lastPreset = Mathf.Clamp(lastPreset, 1, presets.Length - 1);
			NetworkcurPreset = ((curPreset == 0) ? lastPreset : 0);
			CallCmdUpdatePreset(curPreset);
		}
	}

	public void SetRelationship()
	{
		if (base.isLocalPlayer || unityPlayback == null)
		{
			return;
		}
		mySource = unityPlayback.AudioSource;
		icon.id = 0;
		bool flag = false;
		bool flag2 = false;
		mySource.outputAudioMixerGroup = g_voice;
		mySource.volume = 0f;
		mySource.spatialBlend = 1f;
		if (!roundStarted || roundEnded || (voiceInfo.IsDead() && localRadio.voiceInfo.IsDead()))
		{
			mySource.volume = 1f;
			mySource.spatialBlend = 0f;
			return;
		}
		if (ccm.curClass == 7)
		{
			if (!string.IsNullOrEmpty(scp079.Speaker))
			{
				mySource.volume = 1f;
				mySource.spatialBlend = 1f;
				mySource.outputAudioMixerGroup = g_079;
			}
			else if (localRadio.voiceInfo.isSCP)
			{
				mySource.volume = 1f;
				mySource.spatialBlend = 0f;
			}
			return;
		}
		if (voiceInfo.isAliveHuman || (scp939.iAm939 && scp939.usingHumanChat))
		{
			flag2 = true;
		}
		if (voiceInfo.isSCP && localRadio.voiceInfo.isSCP)
		{
			flag2 = true;
			flag = true;
		}
		if (flag2)
		{
			icon.id = 1;
			mySource.volume = 1f;
		}
		if (!flag && host != null && base.gameObject == host.GetComponent<Intercom>().speaker)
		{
			icon.id = 2;
			mySource.outputAudioMixerGroup = g_icom;
			flag = true;
			if (icomNoise > noiseIntensity)
			{
				noiseIntensity = icomNoise;
			}
		}
		else if (isTransmitting && localRadio.CheckRadio() && !flag)
		{
			mySource.outputAudioMixerGroup = g_radio;
			flag = true;
			float num = 0f;
			int lowerPresetID = GetLowerPresetID();
			float time = Vector3.Distance(localRadio.transform.position, base.transform.position);
			mySource.volume = presets[lowerPresetID].volume.Evaluate(time);
			num = presets[lowerPresetID].nosie.Evaluate(time);
			if (num > noiseIntensity && !base.isLocalPlayer)
			{
				noiseIntensity = num;
			}
		}
		if (isTransmitting)
		{
			icon.id = 2;
		}
		if (flag)
		{
			mySource.spatialBlend = 0f;
		}
	}

	public int GetLowerPresetID()
	{
		return (curPreset >= localRadio.curPreset) ? localRadio.curPreset : curPreset;
	}

	public bool CheckRadio()
	{
		return myRadio != -1 && inv.items[myRadio].durability > 0f && voiceInfo.isAliveHuman && curPreset > 0;
	}

	[Command(channel = 6)]
	private void CmdSyncTransmitionStatus(bool b, Vector3 myPos)
	{
		NetworkisTransmitting = b;
	}

	public void PlayBeepSound(bool b)
	{
		beepSource.PlayOneShot((!b) ? beepStop[UnityEngine.Random.Range(0, beepStop.Length)] : beepStart[UnityEngine.Random.Range(0, beepStart.Length)]);
	}

	private float Distance(Vector3 a, Vector3 b)
	{
		return Vector3.Distance(new Vector3(a.x, a.y / 4f, a.z), new Vector3(b.x, b.y / 4f, b.z));
	}

	public bool ShouldBeVisible(GameObject localplayer)
	{
		return !isTransmitting || presets[GetLowerPresetID()].beepRange > Distance(base.transform.position, localplayer.transform.position);
	}

	private void SetPreset(int preset)
	{
		NetworkcurPreset = preset;
	}

	[Command(channel = 6)]
	public void CmdUpdatePreset(int preset)
	{
		SetPreset(preset);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSyncTransmitionStatus(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSyncTransmitionStatus called on client.");
		}
		else
		{
			((Radio)obj).CmdSyncTransmitionStatus(reader.ReadBoolean(), reader.ReadVector3());
		}
	}

	protected static void InvokeCmdCmdUpdatePreset(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUpdatePreset called on client.");
		}
		else
		{
			((Radio)obj).CmdUpdatePreset((int)reader.ReadPackedUInt32());
		}
	}

	public void CallCmdSyncTransmitionStatus(bool b, Vector3 myPos)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSyncTransmitionStatus called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSyncTransmitionStatus(b, myPos);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSyncTransmitionStatus);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(b);
		networkWriter.Write(myPos);
		SendCommandInternal(networkWriter, 6, "CmdSyncTransmitionStatus");
	}

	public void CallCmdUpdatePreset(int preset)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUpdatePreset called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUpdatePreset(preset);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUpdatePreset);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)preset);
		SendCommandInternal(networkWriter, 6, "CmdUpdatePreset");
	}

	static Radio()
	{
		kCmdCmdSyncTransmitionStatus = 860412084;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Radio), kCmdCmdSyncTransmitionStatus, InvokeCmdCmdSyncTransmitionStatus);
		kCmdCmdUpdatePreset = -1209260349;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Radio), kCmdCmdUpdatePreset, InvokeCmdCmdUpdatePreset);
		NetworkCRC.RegisterBehaviour("Radio", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)curPreset);
			writer.Write(isTransmitting);
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
			writer.WritePackedUInt32((uint)curPreset);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(isTransmitting);
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
			curPreset = (int)reader.ReadPackedUInt32();
			isTransmitting = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetPreset((int)reader.ReadPackedUInt32());
		}
		if ((num & 2) != 0)
		{
			isTransmitting = reader.ReadBoolean();
		}
	}
}
