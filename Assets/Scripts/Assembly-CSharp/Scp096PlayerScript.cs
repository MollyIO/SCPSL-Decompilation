using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MEC;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;

public class Scp096PlayerScript : NetworkBehaviour
{
	public enum RageState
	{
		NotEnraged = 0,
		Panic = 1,
		Enraged = 2,
		Cooldown = 3
	}

	[CompilerGenerated]
	private sealed class _003C_ExecuteServersideCode_Looking_003Ec__Iterator1 : IEnumerator, IDisposable, IEnumerator<float>
	{
		internal GameObject[] _003Cplys_003E__1;

		internal bool _003Cfound_003E__1;

		internal GameObject[] _0024locvar0;

		internal int _0024locvar1;

		internal GameObject _003Citem_003E__2;

		internal Scp096PlayerScript _0024this;

		internal float _0024current;

		internal bool _0024disposing;

		internal int _0024PC;

		float IEnumerator<float>.Current
		{
			[DebuggerHidden]
			get
			{
				return _0024current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _0024current;
			}
		}

		[DebuggerHidden]
		public _003C_ExecuteServersideCode_Looking_003Ec__Iterator1()
		{
		}

		public bool MoveNext()
		{
			uint num = (uint)_0024PC;
			_0024PC = -1;
			switch (num)
			{
			case 1u:
				_0024locvar1++;
				goto IL_0210;
			case 0u:
			case 2u:
				if (_0024this != null && _0024this.isServer)
				{
					if (instance != null && instance.iAm096)
					{
						_003Cplys_003E__1 = PlayerManager.singleton.players;
						_003Cfound_003E__1 = false;
						_0024locvar0 = _003Cplys_003E__1;
						_0024locvar1 = 0;
						goto IL_0210;
					}
					_0024current = 0f;
					if (!_0024disposing)
					{
						_0024PC = 2;
					}
					break;
				}
				_0024PC = -1;
				goto default;
			default:
				{
					return false;
				}
				IL_0210:
				if (_0024locvar1 < _0024locvar0.Length)
				{
					_003Citem_003E__2 = _0024locvar0[_0024locvar1];
					if (_003Citem_003E__2 != null && _003Citem_003E__2.GetComponent<CharacterClassManager>().IsHuman() && !_003Citem_003E__2.GetComponent<FlashEffect>().sync_blind)
					{
						Transform transform = _003Citem_003E__2.GetComponent<Scp096PlayerScript>().camera.transform;
						float num2 = _0024this.lookingTolerance.Evaluate(Vector3.Distance(transform.position, instance.camera.transform.position));
						RaycastHit hitInfo;
						if (((double)num2 < 0.75 || Vector3.Dot(transform.forward, (transform.position - instance.camera.transform.position).normalized) < 0f - num2) && Physics.Raycast(transform.transform.position, (instance.camera.transform.position - transform.position).normalized, out hitInfo, 20f, _0024this.layerMask) && hitInfo.collider.gameObject.layer == 24 && hitInfo.collider.GetComponentInParent<Scp096PlayerScript>() == instance)
						{
							_003Cfound_003E__1 = true;
						}
					}
					_0024current = 0f;
					if (!_0024disposing)
					{
						_0024PC = 1;
					}
					break;
				}
				if (_003Cfound_003E__1)
				{
					instance.IncreaseRage(0.02f * _0024this.ragemultiplier_looking * (float)_003Cplys_003E__1.Length);
				}
				goto case 0u;
			}
			return true;
		}

		[DebuggerHidden]
		public void Dispose()
		{
			_0024disposing = true;
			_0024PC = -1;
		}

		[DebuggerHidden]
		public void Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003C_ExecuteServersideCode_RageHandler_003Ec__Iterator2 : IEnumerator, IDisposable, IEnumerator<float>
	{
		internal Scp096PlayerScript _0024this;

		internal float _0024current;

		internal bool _0024disposing;

		internal int _0024PC;

		float IEnumerator<float>.Current
		{
			[DebuggerHidden]
			get
			{
				return _0024current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _0024current;
			}
		}

		[DebuggerHidden]
		public _003C_ExecuteServersideCode_RageHandler_003Ec__Iterator2()
		{
		}

		public bool MoveNext()
		{
			uint num = (uint)_0024PC;
			_0024PC = -1;
			switch (num)
			{
			case 0u:
			case 1u:
				if (_0024this != null && _0024this.isServer)
				{
					_0024this.t = Time.realtimeSinceStartup;
					if (instance != null && instance.iAm096)
					{
						if (instance.enraged == RageState.Enraged)
						{
							instance.DeductRage();
						}
						if (instance.enraged == RageState.Cooldown)
						{
							instance.DeductCooldown();
						}
					}
					_0024current = 0f;
					if (!_0024disposing)
					{
						_0024PC = 1;
					}
					break;
				}
				_0024PC = -1;
				goto default;
			default:
				return false;
			}
			return true;
		}

		[DebuggerHidden]
		public void Dispose()
		{
			_0024disposing = true;
			_0024PC = -1;
		}

		[DebuggerHidden]
		public void Reset()
		{
			throw new NotSupportedException();
		}
	}

	public static Scp096PlayerScript instance;

	public GameObject camera;

	public bool sameClass;

	public bool iAm096;

	public LayerMask layerMask;

	private AnimationController animationController;

	private float cooldown;

	public SoundtrackManager.Track[] tracks;

	[Space]
	[SyncVar(hook = "SetRage")]
	public RageState enraged;

	public float rageProgress;

	[Space]
	public float ragemultiplier_looking;

	[Space]
	public float ragemultiplier_deduct = 0.08f;

	public float ragemultiplier_coodownduration = 20f;

	public AnimationCurve lookingTolerance;

	private float t;

	private CharacterClassManager ccm;

	private FirstPersonController fpc;

	private float normalSpeed;

	private static int kCmdCmdHurtPlayer;

	public RageState Networkenraged
	{
		get
		{
			return enraged;
		}
		[param: In]
		set
		{
			Scp096PlayerScript scp096PlayerScript = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetRage(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref scp096PlayerScript.enraged, 1u);
		}
	}

	private void SetRage(RageState b)
	{
		Networkenraged = b;
	}

	public void IncreaseRage(float amount)
	{
		if (enraged == RageState.NotEnraged)
		{
			rageProgress += amount;
			rageProgress = Mathf.Clamp01(rageProgress);
			if (rageProgress == 1f)
			{
				SetRage(RageState.Panic);
				Invoke("StartRage", 5f);
			}
		}
	}

	private void StartRage()
	{
		SetRage(RageState.Enraged);
	}

	private void Update()
	{
		ExecuteClientsideCode();
		Animator();
	}

	private IEnumerator<float> _UpdateAudios()
	{
		while (this != null)
		{
			for (int i = 0; i < tracks.Length; i++)
			{
				tracks[i].playing = i == (int)enraged && iAm096;
				tracks[i].Update(tracks.Length + 1);
				yield return 0f;
			}
			yield return 0f;
		}
	}

	private void Animator()
	{
		if (!base.isLocalPlayer && animationController.animator != null && iAm096)
		{
			animationController.animator.SetBool("Rage", enraged == RageState.Enraged || enraged == RageState.Panic);
		}
	}

	private void ExecuteClientsideCode()
	{
		if (base.isLocalPlayer && iAm096)
		{
			fpc.m_WalkSpeed = (fpc.m_RunSpeed = normalSpeed * ((enraged == RageState.Panic) ? 0f : ((enraged != RageState.Enraged) ? 1f : 2.8f)));
			if (enraged == RageState.Enraged && Input.GetKey(NewInput.GetKey("Shoot")))
			{
				Shoot();
			}
		}
	}

	public void DeductRage()
	{
		if (enraged == RageState.Enraged)
		{
			rageProgress -= Time.fixedDeltaTime * ragemultiplier_deduct;
			rageProgress = Mathf.Clamp01(rageProgress);
			if (rageProgress == 0f)
			{
				cooldown = ragemultiplier_coodownduration;
				SetRage(RageState.Cooldown);
			}
		}
	}

	public void DeductCooldown()
	{
		if (enraged == RageState.Cooldown)
		{
			cooldown -= 0.02f;
			cooldown = Mathf.Clamp(cooldown, 0f, ragemultiplier_coodownduration);
			if (cooldown == 0f)
			{
				SetRage(RageState.NotEnraged);
			}
		}
	}

	[ServerCallback]
	[DebuggerHidden]
	private IEnumerator<float> _ExecuteServersideCode_Looking()
	{
		if (!NetworkServer.active)
		{
			return null;
		}
		_003C_ExecuteServersideCode_Looking_003Ec__Iterator1 _003C_ExecuteServersideCode_Looking_003Ec__Iterator2 = new _003C_ExecuteServersideCode_Looking_003Ec__Iterator1();
		_003C_ExecuteServersideCode_Looking_003Ec__Iterator2._0024this = this;
		return _003C_ExecuteServersideCode_Looking_003Ec__Iterator2;
	}

	[DebuggerHidden]
	[ServerCallback]
	private IEnumerator<float> _ExecuteServersideCode_RageHandler()
	{
		if (!NetworkServer.active)
		{
			return null;
		}
		_003C_ExecuteServersideCode_RageHandler_003Ec__Iterator2 _003C_ExecuteServersideCode_RageHandler_003Ec__Iterator3 = new _003C_ExecuteServersideCode_RageHandler_003Ec__Iterator2();
		_003C_ExecuteServersideCode_RageHandler_003Ec__Iterator3._0024this = this;
		return _003C_ExecuteServersideCode_RageHandler_003Ec__Iterator3;
	}

	private void Shoot()
	{
		RaycastHit hitInfo;
		if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hitInfo, 1.5f))
		{
			CharacterClassManager component = hitInfo.transform.GetComponent<CharacterClassManager>();
			if (component != null && component.klasy[component.curClass].team != Team.SCP)
			{
				Hitmarker.Hit();
				CallCmdHurtPlayer(hitInfo.transform.gameObject);
			}
		}
	}

	[Command(channel = 2)]
	private void CmdHurtPlayer(GameObject target)
	{
		CharacterClassManager component = target.GetComponent<CharacterClassManager>();
		if (ccm.curClass == 9 && Vector3.Distance(GetComponent<PlyMovementSync>().position, target.transform.position) < 3f && enraged == RageState.Enraged && component.klasy[component.curClass].team != Team.SCP)
		{
			GetComponent<CharacterClassManager>().CallRpcPlaceBlood(target.transform.position, 0, 3.1f);
			GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(99999f, GetComponent<NicknameSync>().myNick + " (" + GetComponent<CharacterClassManager>().SteamId + ")", DamageTypes.Scp096, GetComponent<QueryProcessor>().PlayerId), target);
		}
	}

	public void Init(int classID, Class c)
	{
		sameClass = c.team == Team.SCP;
		iAm096 = classID == 9;
		if (iAm096)
		{
			instance = this;
		}
	}

	private void Start()
	{
		animationController = GetComponent<AnimationController>();
		fpc = GetComponent<FirstPersonController>();
		ccm = GetComponent<CharacterClassManager>();
		normalSpeed = ccm.klasy[9].runSpeed;
		Timing.RunCoroutine(_UpdateAudios(), Segment.FixedUpdate);
		if (base.isLocalPlayer && base.isServer)
		{
			Timing.RunCoroutine(_ExecuteServersideCode_Looking(), Segment.FixedUpdate);
			Timing.RunCoroutine(_ExecuteServersideCode_RageHandler(), Segment.FixedUpdate);
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdHurtPlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdHurtPlayer called on client.");
		}
		else
		{
			((Scp096PlayerScript)obj).CmdHurtPlayer(reader.ReadGameObject());
		}
	}

	public void CallCmdHurtPlayer(GameObject target)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("Command function CmdHurtPlayer called on server.");
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

	static Scp096PlayerScript()
	{
		kCmdCmdHurtPlayer = 787420137;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Scp096PlayerScript), kCmdCmdHurtPlayer, InvokeCmdCmdHurtPlayer);
		NetworkCRC.RegisterBehaviour("Scp096PlayerScript", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write((int)enraged);
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
			writer.Write((int)enraged);
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
			enraged = (RageState)reader.ReadInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetRage((RageState)reader.ReadInt32());
		}
	}
}
