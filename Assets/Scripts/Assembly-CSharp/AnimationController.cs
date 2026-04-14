using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MEC;
using UnityEngine;
using UnityEngine.Networking;

public class AnimationController : NetworkBehaviour
{
	[Serializable]
	public class AnimAudioClip
	{
		public string clipName;

		public AudioClip audio;
	}

	public AnimAudioClip[] clips;

	public AudioSource walkSource;

	public AudioSource runSource;

	public AudioSource gunSource;

	public Animator animator;

	public Animator handAnimator;

	public Animator headAnimator;

	[SyncVar]
	public int curAnim;

	[SyncVar]
	public Vector2 speed;

	public bool cuffed;

	private FirstPersonController fpc;

	private Inventory inv;

	private PlyMovementSync pms;

	private Scp096PlayerScript scp096;

	private DistanceTo dt;

	private CharacterClassManager ccm;

	public static List<AnimationController> controllers;

	private float prevRotX;

	private int prevItem;

	private bool antiposed;

	private static int kCmdCmdSyncData;

	public int NetworkcurAnim
	{
		get
		{
			return curAnim;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref curAnim, 1u);
		}
	}

	public Vector2 Networkspeed
	{
		get
		{
			return speed;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref speed, 2u);
		}
	}

	private void Start()
	{
		ccm = GetComponent<CharacterClassManager>();
		dt = GetComponent<DistanceTo>();
		scp096 = GetComponent<Scp096PlayerScript>();
		pms = GetComponent<PlyMovementSync>();
		fpc = GetComponent<FirstPersonController>();
		inv = GetComponent<Inventory>();
		if (!base.isLocalPlayer)
		{
			controllers.Add(this);
			Invoke("RefreshItems", 6f);
		}
	}

	private void OnDestroy()
	{
		if (!base.isLocalPlayer)
		{
			controllers.Remove(this);
		}
	}

	private Quaternion GetCameraRotation()
	{
		if (pms == null)
		{
			return Quaternion.Euler(Vector3.zero);
		}
		float rotX = pms.rotX;
		rotX = ((!(rotX > 270f)) ? rotX : (rotX - 360f));
		rotX /= 3f;
		float num = (prevRotX = Mathf.Lerp(prevRotX, rotX, Time.deltaTime * 15f));
		return Quaternion.Euler(Vector3.right * num);
	}

	private void LateUpdate()
	{
		if (!base.isLocalPlayer && headAnimator != null)
		{
			headAnimator.transform.localRotation = GetCameraRotation();
		}
		if (!base.isLocalPlayer && handAnimator != null)
		{
			handAnimator.SetBool("Cuffed", cuffed);
		}
		cuffed = false;
	}

	public void PlaySound(int id, bool isGun)
	{
		if (!base.isLocalPlayer)
		{
			if (isGun)
			{
				gunSource.PlayOneShot(clips[id].audio);
			}
			else
			{
				runSource.PlayOneShot(clips[id].audio);
			}
		}
	}

	public void PlaySound(string label, bool isGun)
	{
		if (base.isLocalPlayer)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < clips.Length; i++)
		{
			if (clips[i].clipName == label)
			{
				num = i;
			}
		}
		if (isGun)
		{
			gunSource.PlayOneShot(clips[num].audio);
		}
		else
		{
			runSource.PlayOneShot(clips[num].audio);
		}
	}

	public void DoAnimation(string trigger)
	{
		if (!base.isLocalPlayer && handAnimator != null)
		{
			handAnimator.SetTrigger(trigger);
		}
	}

	private void FixedUpdate()
	{
		if (!base.isLocalPlayer)
		{
			if (prevItem != inv.curItem)
			{
				prevItem = inv.curItem;
				RefreshItems();
			}
			RecieveData();
		}
		else
		{
			TransmitData(fpc.animationID, fpc.plySpeed);
		}
	}

	private void RefreshItems()
	{
		HandPart[] componentsInChildren = GetComponentsInChildren<HandPart>(true);
		HandPart[] array = componentsInChildren;
		foreach (HandPart handPart in array)
		{
			handPart.Invoke("UpdateItem", 0.3f);
		}
	}

	public void SetState(int i)
	{
		NetworkcurAnim = i;
	}

	private IEnumerator<float> _StartAntiposing()
	{
		if (ccm.curClass >= 0 && ccm.klasy[ccm.curClass].team == Team.SCP)
		{
			yield return 0f;
			handAnimator.gameObject.SetActive(false);
			yield return 0f;
			handAnimator.gameObject.SetActive(true);
			yield return 0f;
			handAnimator.updateMode = AnimatorUpdateMode.AnimatePhysics;
			yield return 0f;
			handAnimator.updateMode = AnimatorUpdateMode.Normal;
			yield return 0f;
			handAnimator.gameObject.SetActive(false);
			yield return 0f;
			handAnimator.gameObject.SetActive(true);
		}
	}

	public void OnChangeClass()
	{
		antiposed = false;
	}

	public void RecieveData()
	{
		bool flag = false;
		if (TutorialManager.status)
		{
			flag = true;
		}
		else if ((flag = dt.IsInRange()) && !antiposed && handAnimator != null)
		{
			Timing.RunCoroutine(_StartAntiposing(), Segment.FixedUpdate);
			antiposed = true;
		}
		if (handAnimator != null)
		{
			handAnimator.enabled = flag;
		}
		if (!(this.animator != null))
		{
			return;
		}
		this.animator.enabled = flag || ccm.curClass == 3;
		if (!flag)
		{
			return;
		}
		CalculateAnimation();
		if (handAnimator == null)
		{
			Animator[] componentsInChildren = this.animator.GetComponentsInChildren<Animator>();
			Animator[] array = componentsInChildren;
			foreach (Animator animator in array)
			{
				if (animator != this.animator)
				{
					if (animator.transform.name.ToUpper().Contains("NECK"))
					{
						headAnimator = animator;
					}
					else
					{
						handAnimator = animator;
					}
				}
			}
		}
		else
		{
			handAnimator.SetInteger("CurItem", inv.curItem);
			handAnimator.SetInteger("Running", (speed.x != 0f) ? ((curAnim != 1) ? 1 : 2) : 0);
		}
	}

	private void CalculateAnimation()
	{
		animator.SetBool("Stafe", (curAnim != 2) & ((Mathf.Abs(speed.y) > 0f) & ((speed.x == 0f) | ((speed.x > 0f) & (curAnim == 0)))));
		animator.SetBool("Jump", curAnim == 2);
		float value = 0f;
		float value2 = 0f;
		if (curAnim != 2)
		{
			if (speed.x != 0f)
			{
				value = ((curAnim != 1) ? 1 : 2);
				if (speed.x < 0f)
				{
					value = -1f;
				}
			}
			if (speed.y != 0f)
			{
				value2 = ((speed.y > 0f) ? 1 : (-1));
			}
		}
		animator.SetFloat("Speed", value, 0.1f, Time.deltaTime);
		animator.SetFloat("Direction", value2, 0.1f, Time.deltaTime);
	}

	[ClientCallback]
	private void TransmitData(int state, Vector2 v2)
	{
		if (NetworkClient.active)
		{
			CallCmdSyncData(state, v2);
		}
	}

	[Command(channel = 3)]
	private void CmdSyncData(int state, Vector2 v2)
	{
		NetworkcurAnim = state;
		Networkspeed = v2;
		Color red = Color.red;
	}

	static AnimationController()
	{
		controllers = new List<AnimationController>();
		kCmdCmdSyncData = 1138927717;
		NetworkBehaviour.RegisterCommandDelegate(typeof(AnimationController), kCmdCmdSyncData, InvokeCmdCmdSyncData);
		NetworkCRC.RegisterBehaviour("AnimationController", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSyncData(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSyncData called on client.");
		}
		else
		{
			((AnimationController)obj).CmdSyncData((int)reader.ReadPackedUInt32(), reader.ReadVector2());
		}
	}

	public void CallCmdSyncData(int state, Vector2 v2)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSyncData called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSyncData(state, v2);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSyncData);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)state);
		networkWriter.Write(v2);
		SendCommandInternal(networkWriter, 3, "CmdSyncData");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)curAnim);
			writer.Write(speed);
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
			writer.WritePackedUInt32((uint)curAnim);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(speed);
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
			curAnim = (int)reader.ReadPackedUInt32();
			speed = reader.ReadVector2();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			curAnim = (int)reader.ReadPackedUInt32();
		}
		if ((num & 2) != 0)
		{
			speed = reader.ReadVector2();
		}
	}
}
