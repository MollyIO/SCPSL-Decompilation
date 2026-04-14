using Dissonance.Integrations.UNet_HLAPI;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.ImageEffects;

public class Scp173PlayerScript : NetworkBehaviour
{
	[Header("Player Properties")]
	public bool iAm173;

	public bool sameClass;

	[Header("Raycasting")]
	public GameObject cam;

	public float range;

	public LayerMask layerMask;

	public LayerMask teleportMask;

	public LayerMask hurtLayer;

	[Header("Blinking")]
	public float minBlinkTime;

	public float maxBlinkTime;

	public float blinkDuration_notsee;

	public float blinkDuration_see;

	private float remainingTime;

	private VignetteAndChromaticAberration blinkCtrl;

	private FirstPersonController fpc;

	private PlyMovementSync pms;

	private CharacterClassManager public_ccm;

	private PlayerStats ps;

	public GameObject weaponCameras;

	public GameObject hitbox;

	public AudioClip[] necksnaps;

	[Header("Boosts")]
	public AnimationCurve boost_teleportDistance;

	public AnimationCurve boost_speed;

	private bool allowMove = true;

	private static float blinkTimeRemaining;

	private static bool localplayerIs173;

	public static bool isBlinking;

	private FlashEffect flash;

	private LocalCurrentRoomEffects lcre;

	private float antiBlindTime;

	private static int kCmdCmdHurtPlayer;

	private static int kRpcRpcBlinkTime;

	private static int kRpcRpcSyncAudio;

	private void Start()
	{
		lcre = GetComponent<LocalCurrentRoomEffects>();
		flash = GetComponent<FlashEffect>();
		ps = GetComponent<PlayerStats>();
		public_ccm = GetComponent<CharacterClassManager>();
		if (base.isLocalPlayer)
		{
			blinkCtrl = GetComponentInChildren<VignetteAndChromaticAberration>();
			fpc = GetComponent<FirstPersonController>();
			pms = GetComponent<PlyMovementSync>();
			isBlinking = false;
		}
	}

	public void Init(int classID, Class c)
	{
		sameClass = c.team == Team.SCP;
		if (base.isLocalPlayer)
		{
			fpc.lookingAtMe = false;
		}
		iAm173 = classID == 0;
		if (base.isLocalPlayer)
		{
			localplayerIs173 = iAm173;
		}
		hitbox.SetActive(!base.isLocalPlayer && localplayerIs173);
	}

	private void FixedUpdate()
	{
		DoBlinkingSequence();
		if (!iAm173 || (!base.isLocalPlayer && !NetworkServer.active))
		{
			return;
		}
		allowMove = true;
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			Scp173PlayerScript component = gameObject.GetComponent<Scp173PlayerScript>();
			if (!component.sameClass && component.LookFor173(base.gameObject, true) && LookFor173(component.gameObject, false))
			{
				allowMove = false;
				break;
			}
		}
		if (base.isLocalPlayer)
		{
			CheckForInput();
			fpc.lookingAtMe = !allowMove;
			float num = boost_speed.Evaluate(ps.GetHealthPercent());
			fpc.m_WalkSpeed = num;
			fpc.m_RunSpeed = num;
		}
	}

	public bool LookFor173(GameObject scp, bool angleCheck)
	{
		if (!scp || public_ccm.curClass == 2 || flash.sync_blind || lcre.syncFlicker)
		{
			return false;
		}
		RaycastHit hitInfo;
		if ((!angleCheck || Vector3.Dot(cam.transform.forward, (cam.transform.position - scp.transform.position).normalized) < -0.65f) && Physics.Raycast(cam.transform.position, (scp.transform.position - cam.transform.position).normalized, out hitInfo, range, layerMask) && hitInfo.transform.name == scp.name)
		{
			return true;
		}
		return false;
	}

	public bool CanMove()
	{
		return allowMove || blinkTimeRemaining > 0f;
	}

	private void DoBlinkingSequence()
	{
		if (!base.isServer || !base.isLocalPlayer)
		{
			return;
		}
		remainingTime -= Time.fixedDeltaTime;
		blinkTimeRemaining -= Time.fixedDeltaTime;
		if (remainingTime < 0f)
		{
			blinkTimeRemaining = blinkDuration_see + 0.5f;
			remainingTime = Random.Range(minBlinkTime, maxBlinkTime);
			Scp173PlayerScript[] array = Object.FindObjectsOfType<Scp173PlayerScript>();
			foreach (Scp173PlayerScript scp173PlayerScript in array)
			{
				scp173PlayerScript.CallRpcBlinkTime();
			}
		}
	}

	public void Boost()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		pms.ClientSetRotation(base.transform.rotation.eulerAngles.y);
		if (fpc.lookingAtMe)
		{
			bool flag = false;
			RaycastHit hitInfo;
			if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hitInfo, 100f, teleportMask) && hitInfo.transform.GetComponent<CharacterClassManager>() != null && Input.GetAxisRaw("Vertical") > 0f && Input.GetAxisRaw("Horizontal") == 0f)
			{
				flag = true;
			}
			float num = boost_teleportDistance.Evaluate(ps.GetHealthPercent());
			Vector3 position = base.transform.position;
			if (flag)
			{
				Vector3 vector = hitInfo.transform.position - base.transform.position;
				vector = vector.normalized * Mathf.Clamp(vector.magnitude - 1f, 0f, num);
				base.transform.position += vector;
			}
			else
			{
				RaycastHit hitInfo2;
				Physics.Raycast(cam.transform.position, cam.transform.forward, out hitInfo2, 100f, teleportMask);
				float b = Vector3.Distance(base.transform.position, new Vector3(hitInfo2.point.x, base.transform.position.y, hitInfo2.point.z));
				num = Mathf.Min(num, b);
				for (int i = 0; i < 1000; i++)
				{
					if (!(Vector3.Distance(position, base.transform.position) < num))
					{
						break;
					}
					Vector3 position2 = base.transform.position;
					Forward();
					if (position2 == base.transform.position)
					{
						break;
					}
				}
			}
		}
		if (Input.GetKey(NewInput.GetKey("Shoot")))
		{
			Shoot();
		}
	}

	private void Forward()
	{
		fpc.blinkAddition = 0.8f;
		fpc.MotorPlayer();
		fpc.blinkAddition = 0f;
	}

	public void Blink()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		isBlinking = true;
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			if (gameObject.GetComponent<Scp173PlayerScript>().iAm173)
			{
				bool flag = LookFor173(gameObject, true);
				if (flag)
				{
					blinkCtrl.intensity = 1f;
					weaponCameras.SetActive(false);
				}
				Invoke("UnBlink", (!flag) ? blinkDuration_notsee : blinkDuration_see);
			}
		}
	}

	private void UnBlink()
	{
		blinkCtrl.intensity = 0.036f;
		isBlinking = false;
		weaponCameras.SetActive(true);
	}

	private void Update()
	{
		if (base.isLocalPlayer && !sameClass)
		{
			AntiBlind();
		}
	}

	private void AntiBlind()
	{
		if (blinkCtrl.intensity > 0.5f)
		{
			antiBlindTime += Time.deltaTime;
			if (antiBlindTime > 2f)
			{
				UnBlink();
			}
		}
		else
		{
			antiBlindTime = 0f;
		}
	}

	private void CheckForInput()
	{
		if (Input.GetKeyDown(NewInput.GetKey("Shoot")) && allowMove)
		{
			Shoot();
		}
	}

	private void Shoot()
	{
		RaycastHit hitInfo;
		if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hitInfo, 1.5f, hurtLayer))
		{
			CharacterClassManager component = hitInfo.transform.GetComponent<CharacterClassManager>();
			if (component != null && component.klasy[component.curClass].team != Team.SCP)
			{
				HurtPlayer(hitInfo.transform.gameObject, GetComponent<HlapiPlayer>().PlayerId);
			}
		}
	}

	private void HurtPlayer(GameObject go, string plyID)
	{
		Hitmarker.Hit();
		CallCmdHurtPlayer(go);
	}

	[Command(channel = 2)]
	private void CmdHurtPlayer(GameObject target)
	{
		CharacterClassManager component = target.GetComponent<CharacterClassManager>();
		if (GetComponent<CharacterClassManager>().curClass == 0 && CanMove() && Vector3.Distance(GetComponent<PlyMovementSync>().position, target.transform.position) < 3f + boost_teleportDistance.Evaluate(ps.GetHealthPercent()) && component.klasy[component.curClass].team != Team.SCP)
		{
			CallRpcSyncAudio();
			GetComponent<CharacterClassManager>().CallRpcPlaceBlood(target.transform.position, 0, 2.2f);
			GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(999990f, GetComponent<NicknameSync>().myNick + " (" + public_ccm.SteamId + ")", DamageTypes.Scp173, GetComponent<QueryProcessor>().PlayerId), target);
		}
	}

	[ClientRpc(channel = 0)]
	private void RpcBlinkTime()
	{
		if (iAm173)
		{
			Boost();
		}
		if (!sameClass)
		{
			Blink();
		}
	}

	[ClientRpc(channel = 1)]
	private void RpcSyncAudio()
	{
		GetComponent<AnimationController>().gunSource.PlayOneShot(necksnaps[Random.Range(0, necksnaps.Length)]);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdHurtPlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHurtPlayer called on client.");
		}
		else
		{
			((Scp173PlayerScript)obj).CmdHurtPlayer(reader.ReadGameObject());
		}
	}

	public void CallCmdHurtPlayer(GameObject target)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdHurtPlayer called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdHurtPlayer(target);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdHurtPlayer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(target);
		SendCommandInternal(networkWriter, 2, "CmdHurtPlayer");
	}

	protected static void InvokeRpcRpcBlinkTime(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcBlinkTime called on server.");
		}
		else
		{
			((Scp173PlayerScript)obj).RpcBlinkTime();
		}
	}

	protected static void InvokeRpcRpcSyncAudio(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcSyncAudio called on server.");
		}
		else
		{
			((Scp173PlayerScript)obj).RpcSyncAudio();
		}
	}

	public void CallRpcBlinkTime()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcBlinkTime called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcBlinkTime);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcBlinkTime");
	}

	public void CallRpcSyncAudio()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcSyncAudio called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSyncAudio);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 1, "RpcSyncAudio");
	}

	static Scp173PlayerScript()
	{
		kCmdCmdHurtPlayer = 1571551081;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Scp173PlayerScript), kCmdCmdHurtPlayer, InvokeCmdCmdHurtPlayer);
		kRpcRpcBlinkTime = -1078791558;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Scp173PlayerScript), kRpcRpcBlinkTime, InvokeRpcRpcBlinkTime);
		kRpcRpcSyncAudio = -769227732;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Scp173PlayerScript), kRpcRpcSyncAudio, InvokeRpcRpcSyncAudio);
		NetworkCRC.RegisterBehaviour("Scp173PlayerScript", 0);
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
