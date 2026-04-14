using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Handcuffs : NetworkBehaviour
{
	public TextMeshProUGUI distanceText;

	public LayerMask mask;

	private Transform plyCam;

	private CharacterClassManager ccm;

	private Inventory inv;

	private Image uncuffProgress;

	public float maxDistance;

	private float progress;

	private float lostCooldown;

	private float serverCooldown;

	[SyncVar(hook = "SetTarget")]
	public GameObject cuffTarget;

	private NetworkInstanceId ___cuffTargetNetId;

	private static int kCmdCmdTarget;

	private static int kCmdCmdResetTarget;

	public GameObject NetworkcuffTarget
	{
		get
		{
			return cuffTarget;
		}
		[param: In]
		set
		{
			Handcuffs handcuffs = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetTarget(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVarGameObject(value, ref handcuffs.cuffTarget, 1u, ref ___cuffTargetNetId);
		}
	}

	private void Start()
	{
		uncuffProgress = GameObject.Find("UncuffProgress").GetComponent<Image>();
		inv = GetComponent<Inventory>();
		plyCam = GetComponent<Scp049PlayerScript>().plyCam.transform;
		ccm = GetComponent<CharacterClassManager>();
	}

	private void Update()
	{
		if (serverCooldown > 0f)
		{
			serverCooldown -= Time.deltaTime;
			if (serverCooldown < 0f)
			{
				serverCooldown = 0f;
			}
		}
		if (base.isLocalPlayer)
		{
			CheckForInput();
			UpdateText();
		}
		if (cuffTarget != null)
		{
			cuffTarget.GetComponent<AnimationController>().cuffed = true;
		}
	}

	private void CheckForInput()
	{
		if (cuffTarget != null)
		{
			bool flag = false;
			foreach (Inventory.SyncItemInfo item in inv.items)
			{
				if (item.id == 27)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				CallCmdTarget(null);
			}
		}
		if (Inventory.inventoryCooldown > 0f)
		{
			return;
		}
		if (inv.curItem == 27)
		{
			if (Input.GetKeyDown(NewInput.GetKey("Shoot")) && cuffTarget == null)
			{
				CuffPlayer();
			}
			else if (Input.GetKeyDown(NewInput.GetKey("Zoom")) && cuffTarget != null)
			{
				CallCmdTarget(null);
			}
		}
		if (ccm.curClass >= 0 && ccm.klasy[ccm.curClass].team != Team.SCP && Input.GetKey(NewInput.GetKey("Interact")))
		{
			RaycastHit hitInfo;
			if (Physics.Raycast(plyCam.position, plyCam.forward, out hitInfo, maxDistance, GetComponent<PlayerInteract>().mask))
			{
				Handcuffs componentInParent = hitInfo.collider.GetComponentInParent<Handcuffs>();
				if (componentInParent != null && componentInParent.GetComponent<AnimationController>().handAnimator != null && componentInParent.GetComponent<AnimationController>().handAnimator.GetBool("Cuffed"))
				{
					progress += Time.deltaTime;
					if (progress >= 1.5f)
					{
						progress = 0f;
						GameObject[] players = PlayerManager.singleton.players;
						foreach (GameObject gameObject in players)
						{
							if (gameObject.GetComponent<Handcuffs>().cuffTarget == componentInParent.gameObject)
							{
								CallCmdResetTarget(gameObject);
							}
						}
					}
				}
				else
				{
					progress = 0f;
				}
			}
			else
			{
				progress = 0f;
			}
		}
		else
		{
			progress = 0f;
		}
		if (ccm.curClass != 3)
		{
			uncuffProgress.fillAmount = Mathf.Clamp01(progress / 1.5f);
		}
	}

	private void CuffPlayer()
	{
		Ray ray = new Ray(plyCam.position, plyCam.forward);
		RaycastHit hitInfo;
		if (!Physics.Raycast(ray, out hitInfo, maxDistance, mask))
		{
			return;
		}
		CharacterClassManager componentInParent = hitInfo.collider.GetComponentInParent<CharacterClassManager>();
		if (componentInParent == null)
		{
			return;
		}
		Class obj = ccm.klasy[componentInParent.curClass];
		if (CanCuffClient(ccm.klasy[ccm.curClass], obj) && componentInParent.GetComponent<AnimationController>().curAnim == 0 && !(componentInParent.GetComponent<AnimationController>().speed != Vector2.zero))
		{
			if (ccm.klasy[ccm.curClass].team == Team.CDP && obj.team == Team.MTF)
			{
				AchievementManager.Achieve("tableshaveturned");
			}
			Debug.Log("Trying to cuff " + componentInParent.GetComponent<NicknameSync>().myNick);
			CallCmdTarget(componentInParent.gameObject);
		}
	}

	public bool CanCuffClient(Class source, Class target)
	{
		if (source.team == Team.RIP || source.team == Team.SCP)
		{
			return false;
		}
		if (target.team == Team.SCP || target.team == Team.TUT || target.team == Team.RIP)
		{
			return false;
		}
		if (source.team == target.team)
		{
			return source.fullName == "Nine-Tailed Fox Commander" && target.fullName != "Nine-Tailed Fox Commander";
		}
		if (source.team == Team.CDP && target.team == Team.CHI)
		{
			return false;
		}
		if (source.team == Team.RSC && target.team == Team.MTF)
		{
			return false;
		}
		return true;
	}

	public bool CanCuffServer(Class source, Class target)
	{
		if (source.team == Team.RIP || source.team == Team.SCP)
		{
			return false;
		}
		if (target.team == Team.SCP || target.team == Team.TUT || target.team == Team.RIP)
		{
			return false;
		}
		if (source.team == target.team)
		{
			return source.fullName == "Nine-Tailed Fox Commander" && target.fullName != "Nine-Tailed Fox Commander" && ConfigFile.ServerConfig.GetBool("commander_can_cuff_mtf", true);
		}
		if (source.team == Team.CDP && target.team == Team.CHI)
		{
			return false;
		}
		if (source.team == Team.CHI && target.team == Team.CDP && !ConfigFile.ServerConfig.GetBool("ci_can_cuff_class_d", true))
		{
			return false;
		}
		if (source.team == Team.RSC && target.team == Team.MTF)
		{
			return false;
		}
		if (source.team == Team.MTF && target.team == Team.RSC && !ConfigFile.ServerConfig.GetBool("mtf_can_cuff_researchers", true))
		{
			return false;
		}
		return true;
	}

	[Command(channel = 2)]
	public void CmdTarget(GameObject t)
	{
		if (t == null)
		{
			SetTarget(null);
		}
		else if (!(serverCooldown > 0f) && !GetComponent<Inventory>().items.All((Inventory.SyncItemInfo item) => item.id != 27) && !(Vector3.Distance(base.transform.position, t.transform.position) >= 3f) && inv.curItem == 27 && t.GetComponent<AnimationController>().curAnim == 0)
		{
			Class target = ccm.klasy[t.GetComponent<CharacterClassManager>().curClass];
			if (CanCuffServer(ccm.klasy[ccm.curClass], target))
			{
				serverCooldown = 2.5f;
				SetTarget(t);
				t.GetComponent<Inventory>().ServerDropAll();
			}
		}
	}

	[Command(channel = 2)]
	public void CmdResetTarget(GameObject t)
	{
		if (t != base.gameObject)
		{
			t.GetComponent<Handcuffs>().SetTarget(null);
		}
	}

	private void SetTarget(GameObject t)
	{
		NetworkcuffTarget = t;
	}

	private void UpdateText()
	{
		if (cuffTarget != null)
		{
			float num = Vector3.Distance(base.transform.position, cuffTarget.transform.position);
			if (num > 200f)
			{
				num = 200f;
				lostCooldown += Time.deltaTime;
				if (lostCooldown > 1f)
				{
					CallCmdTarget(null);
				}
			}
			else
			{
				lostCooldown = 0f;
			}
			distanceText.text = (num * 1.5f).ToString("0 m");
		}
		else
		{
			distanceText.text = "NONE";
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdTarget(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTarget called on client.");
		}
		else
		{
			((Handcuffs)obj).CmdTarget(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdResetTarget(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdResetTarget called on client.");
		}
		else
		{
			((Handcuffs)obj).CmdResetTarget(reader.ReadGameObject());
		}
	}

	public void CallCmdTarget(GameObject t)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdTarget called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdTarget(t);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdTarget);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(t);
		SendCommandInternal(networkWriter, 2, "CmdTarget");
	}

	public void CallCmdResetTarget(GameObject t)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdResetTarget called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdResetTarget(t);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdResetTarget);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(t);
		SendCommandInternal(networkWriter, 2, "CmdResetTarget");
	}

	static Handcuffs()
	{
		kCmdCmdTarget = 624996931;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Handcuffs), kCmdCmdTarget, InvokeCmdCmdTarget);
		kCmdCmdResetTarget = -1476369842;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Handcuffs), kCmdCmdResetTarget, InvokeCmdCmdResetTarget);
		NetworkCRC.RegisterBehaviour("Handcuffs", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(cuffTarget);
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
			writer.Write(cuffTarget);
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
			___cuffTargetNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetTarget(reader.ReadGameObject());
		}
	}

	public override void PreStartClient()
	{
		if (!___cuffTargetNetId.IsEmpty())
		{
			NetworkcuffTarget = ClientScene.FindLocalObject(___cuffTargetNetId);
		}
	}
}
