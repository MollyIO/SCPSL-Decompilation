using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MEC;
using RemoteAdmin;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class Scp106PlayerScript : NetworkBehaviour
{
	[CompilerGenerated]
	private sealed class _003C_ContainAnimation_003Ec__Iterator0 : IEnumerator, IDisposable, IEnumerator<float>
	{
		internal CharacterClassManager ccm;

		internal Scp106PlayerScript _0024this;

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
		public _003C_ContainAnimation_003Ec__Iterator0()
		{
		}

		public bool MoveNext()
		{
			uint num = (uint)_0024PC;
			_0024PC = -1;
			switch (num)
			{
			case 0u:
				_0024this.CallRpcContainAnimation();
				_0024current = Timing.WaitForSeconds(18f);
				if (!_0024disposing)
				{
					_0024PC = 1;
				}
				break;
			case 1u:
				_0024this.goingViaThePortal = true;
				_0024current = Timing.WaitForSeconds(3.5f);
				if (!_0024disposing)
				{
					_0024PC = 2;
				}
				break;
			case 2u:
				_0024this.Kill(ccm);
				_0024this.goingViaThePortal = false;
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

	[CompilerGenerated]
	private sealed class _003C_DoTeleportAnimation_003Ec__Iterator4 : IEnumerator, IDisposable, IEnumerator<float>
	{
		internal PlyMovementSync _003Cpms_003E__1;

		internal Scp106PlayerScript _0024this;

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
		public _003C_DoTeleportAnimation_003Ec__Iterator4()
		{
		}

		public bool MoveNext()
		{
			uint num = (uint)_0024PC;
			_0024PC = -1;
			switch (num)
			{
			case 0u:
				if (_0024this.portalPrefab != null && !_0024this.goingViaThePortal)
				{
					_0024this.CallRpcTeleportAnimation();
					_0024this.goingViaThePortal = true;
					_003Cpms_003E__1 = _0024this.GetComponent<PlyMovementSync>();
					_0024current = Timing.WaitForSeconds(3.5f);
					if (!_0024disposing)
					{
						_0024PC = 1;
					}
					break;
				}
				goto IL_0169;
			case 1u:
				_003Cpms_003E__1.SetPosition(_0024this.portalPrefab.transform.position + Vector3.up * 1.5f);
				_0024current = Timing.WaitForSeconds(3.5f);
				if (!_0024disposing)
				{
					_0024PC = 2;
				}
				break;
			case 2u:
				if (AlphaWarheadController.host.detonated && _0024this.transform.position.y < 800f)
				{
					_0024this.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(9000f, "WORLD", DamageTypes.Nuke, 0), _0024this.gameObject);
				}
				_003Cpms_003E__1.SetAllowInput(true);
				_0024this.goingViaThePortal = false;
				goto IL_0169;
			default:
				{
					return false;
				}
				IL_0169:
				_0024PC = -1;
				goto default;
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

	[Header("Player Properties")]
	public Transform plyCam;

	public bool iAm106;

	public bool sameClass;

	[SyncVar]
	private float ultimatePoints;

	public float teleportSpeed;

	public GameObject screamsPrefab;

	[SyncVar(hook = "SetPortalPosition")]
	[Header("Portal")]
	public Vector3 portalPosition;

	public GameObject portalPrefab;

	private Vector3 previousPortalPosition;

	private Offset modelOffset;

	private CharacterClassManager ccm;

	private FirstPersonController fpc;

	private GameObject popup106;

	private TextMeshProUGUI highlightedAbilityText;

	private Text pointsText;

	private string highlightedString;

	public int highlightID;

	private Image cooldownImg;

	private static BlastDoor blastDoor;

	private float attackCooldown;

	public bool goingViaThePortal;

	private bool isCollidingDoorOpen;

	private Door doorCurrentlyIn;

	private bool isHighlightingPoints;

	public LayerMask teleportPlacementMask;

	private static int kRpcRpcContainAnimation;

	private static int kRpcRpcTeleportAnimation;

	private static int kCmdCmdMakePortal;

	private static int kCmdCmdUsePortal;

	private static int kCmdCmdMovePlayer;

	public float NetworkultimatePoints
	{
		get
		{
			return ultimatePoints;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref ultimatePoints, 1u);
		}
	}

	public Vector3 NetworkportalPosition
	{
		get
		{
			return portalPosition;
		}
		[param: In]
		set
		{
			Scp106PlayerScript scp106PlayerScript = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetPortalPosition(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref scp106PlayerScript.portalPosition, 2u);
		}
	}

	private void Start()
	{
		if (blastDoor == null)
		{
			blastDoor = UnityEngine.Object.FindObjectOfType<BlastDoor>();
		}
		cooldownImg = GameObject.Find("Cooldown106").GetComponent<Image>();
		ccm = GetComponent<CharacterClassManager>();
		fpc = GetComponent<FirstPersonController>();
		InvokeRepeating("ExitDoor", 1f, 2f);
		if (base.isLocalPlayer && NetworkServer.active)
		{
			InvokeRepeating("HumanPocketLoss", 1f, 1f);
		}
		modelOffset = ccm.klasy[3].model_offset;
		if (base.isLocalPlayer)
		{
			pointsText = UnityEngine.Object.FindObjectOfType<ScpInterfaces>().Scp106_ability_points;
			pointsText.text = TranslationReader.Get("Legancy_Interfaces", 11);
		}
	}

	private void Update()
	{
		CheckForInventoryInput();
		CheckForShootInput();
		AnimateHighlightedText();
		UpdatePointText();
		DoorCollisionCheck();
	}

	[Server]
	private void HumanPocketLoss()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Scp106PlayerScript::HumanPocketLoss()' called on client");
			return;
		}
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			if (gameObject.transform.position.y < -1500f && gameObject.GetComponent<CharacterClassManager>().IsHuman())
			{
				gameObject.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(1f, "WORLD", DamageTypes.Pocket, GetComponent<QueryProcessor>().PlayerId), gameObject);
			}
		}
	}

	private void CheckForShootInput()
	{
		if (base.isLocalPlayer && iAm106)
		{
			cooldownImg.fillAmount = Mathf.Clamp01((!(attackCooldown <= 0f)) ? (1f - attackCooldown * 2f) : 0f);
			if (attackCooldown > 0f)
			{
				attackCooldown -= Time.deltaTime;
			}
			if (Input.GetKeyDown(NewInput.GetKey("Shoot")) && attackCooldown <= 0f && Inventory.inventoryCooldown <= 0f)
			{
				attackCooldown = 0.5f;
				Shoot();
			}
		}
	}

	private void Shoot()
	{
		RaycastHit hitInfo;
		if (Physics.Raycast(plyCam.transform.position, plyCam.transform.forward, out hitInfo, 1.5f))
		{
			CharacterClassManager component = hitInfo.transform.GetComponent<CharacterClassManager>();
			if (!(component == null) && component.klasy[component.curClass].team != Team.SCP)
			{
				CallCmdMovePlayer(hitInfo.transform.gameObject, ServerTime.time);
				Hitmarker.Hit(1.5f);
			}
		}
	}

	private void UpdatePointText()
	{
		if (base.isServer)
		{
			NetworkultimatePoints = ultimatePoints + Time.deltaTime * 6.66f * teleportSpeed;
			NetworkultimatePoints = Mathf.Clamp(ultimatePoints, 0f, 100f);
		}
	}

	private bool BuyAbility(int cost)
	{
		if ((float)cost <= ultimatePoints)
		{
			if (base.isServer)
			{
				NetworkultimatePoints = ultimatePoints - (float)cost;
			}
			return true;
		}
		return false;
	}

	private void AnimateHighlightedText()
	{
		if (highlightedAbilityText == null)
		{
			highlightedAbilityText = UnityEngine.Object.FindObjectOfType<ScpInterfaces>().Scp106_ability_highlight;
			return;
		}
		highlightedString = string.Empty;
		switch (highlightID)
		{
		case 1:
			highlightedString = TranslationReader.Get("Legancy_Interfaces", 12);
			break;
		case 2:
			highlightedString = TranslationReader.Get("Legancy_Interfaces", 13);
			break;
		}
		if (highlightedString != highlightedAbilityText.text)
		{
			if (highlightedAbilityText.canvasRenderer.GetAlpha() > 0f)
			{
				highlightedAbilityText.canvasRenderer.SetAlpha(highlightedAbilityText.canvasRenderer.GetAlpha() - Time.deltaTime * 4f);
			}
			else
			{
				highlightedAbilityText.text = highlightedString;
			}
		}
		else if (highlightedAbilityText.canvasRenderer.GetAlpha() < 1f && highlightedString != string.Empty)
		{
			highlightedAbilityText.canvasRenderer.SetAlpha(highlightedAbilityText.canvasRenderer.GetAlpha() + Time.deltaTime * 4f);
		}
	}

	private void CheckForInventoryInput()
	{
		if (base.isLocalPlayer)
		{
			if (popup106 == null)
			{
				popup106 = UnityEngine.Object.FindObjectOfType<ScpInterfaces>().Scp106_eq;
				return;
			}
			bool flag = iAm106 & Input.GetKey(NewInput.GetKey("Inventory"));
			CursorManager.singleton.scp106 = flag;
			popup106.SetActive(flag);
			fpc.m_MouseLook.scp106_eq = flag;
		}
	}

	public void Init(int classID, Class c)
	{
		iAm106 = classID == 3;
		sameClass = c.team == Team.SCP;
	}

	public void SetDoors()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		Door[] array = UnityEngine.Object.FindObjectsOfType<Door>();
		Door[] array2 = array;
		foreach (Door door in array2)
		{
			if (!(door.permissionLevel != "UNACCESSIBLE") || door.locked)
			{
				continue;
			}
			Collider[] componentsInChildren = door.GetComponentsInChildren<Collider>();
			foreach (Collider collider in componentsInChildren)
			{
				if (collider.tag != "DoorButton")
				{
					try
					{
						collider.isTrigger = iAm106;
					}
					catch
					{
					}
				}
			}
		}
	}

	[Server]
	public void Contain(CharacterClassManager ccm)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Scp106PlayerScript::Contain(CharacterClassManager)' called on client");
			return;
		}
		NetworkultimatePoints = 0f;
		Timing.RunCoroutine(_ContainAnimation(ccm), Segment.Update);
	}

	public void DeletePortal()
	{
		if (portalPosition.y < 900f)
		{
			portalPrefab = null;
			NetworkportalPosition = Vector3.zero;
		}
	}

	public void UseTeleport()
	{
		if (GetComponent<FallDamage>().isGrounded)
		{
			if (portalPrefab != null && BuyAbility(100) && portalPosition != Vector3.zero)
			{
				CallCmdUsePortal();
			}
			else
			{
				Timing.RunCoroutine(_HighlightPointsText(), Segment.FixedUpdate);
			}
		}
	}

	private void SetPortalPosition(Vector3 pos)
	{
		NetworkportalPosition = pos;
		Timing.RunCoroutine(_DoPortalSetupAnimation(), Segment.Update);
	}

	public void CreatePortalInCurrentPosition()
	{
		if (!GetComponent<FallDamage>().isGrounded)
		{
			return;
		}
		if (BuyAbility(100))
		{
			if (base.isLocalPlayer)
			{
				CallCmdMakePortal();
			}
		}
		else
		{
			Timing.RunCoroutine(_HighlightPointsText(), Segment.FixedUpdate);
		}
	}

	[Server]
	[DebuggerHidden]
	private IEnumerator<float> _ContainAnimation(CharacterClassManager ccm)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Collections.Generic.IEnumerator`1<System.Single> Scp106PlayerScript::_ContainAnimation(CharacterClassManager)' called on client");
			return null;
		}
		_003C_ContainAnimation_003Ec__Iterator0 _003C_ContainAnimation_003Ec__Iterator1 = new _003C_ContainAnimation_003Ec__Iterator0();
		_003C_ContainAnimation_003Ec__Iterator1.ccm = ccm;
		_003C_ContainAnimation_003Ec__Iterator1._0024this = this;
		return _003C_ContainAnimation_003Ec__Iterator1;
	}

	private IEnumerator<float> _ClientContainAnimation()
	{
		for (int i = 0; i < 900; i++)
		{
			yield return 0f;
		}
		if (base.isLocalPlayer)
		{
			goingViaThePortal = true;
			VignetteAndChromaticAberration vaca = GetComponentInChildren<VignetteAndChromaticAberration>();
			Recoil recoil = GetComponentInChildren<Recoil>();
			fpc.noclip = true;
			for (float i2 = 1f; i2 <= 175f; i2 += 1f)
			{
				recoil.positionOffset = -1.6f * (vaca.intensity = i2 / 175f);
				yield return 0f;
			}
			yield return Timing.WaitForSeconds(2f);
			fpc.noclip = false;
			goingViaThePortal = false;
			yield return Timing.WaitForSeconds(5f);
			vaca.intensity = 0.036f;
			recoil.positionOffset = 0f;
		}
		else
		{
			GetComponent<AnimationController>().animator.SetTrigger("Teleporting");
		}
	}

	[ClientRpc]
	private void RpcContainAnimation()
	{
		Timing.RunCoroutine(_ClientContainAnimation(), Segment.FixedUpdate);
	}

	private void LateUpdate()
	{
		Animator animator = GetComponent<AnimationController>().animator;
		if (animator != null && iAm106 && !base.isLocalPlayer)
		{
			AnimationFloatValue component = ccm.myModel.GetComponent<AnimationFloatValue>();
			Offset offset = modelOffset;
			offset.position -= component.v3_value * component.f_value;
			animator.transform.localPosition = offset.position;
			animator.transform.localRotation = Quaternion.Euler(offset.rotation);
		}
	}

	[Server]
	public void Kill(CharacterClassManager ccm)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Void Scp106PlayerScript::Kill(CharacterClassManager)' called on client");
		}
		else
		{
			GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(999799f, string.Empty, DamageTypes.RagdollLess, ccm.GetComponent<QueryProcessor>().PlayerId), base.gameObject);
		}
	}

	private IEnumerator<float> _HighlightPointsText()
	{
		if (!isHighlightingPoints)
		{
			isHighlightingPoints = true;
			while ((double)pointsText.color.g > 0.05)
			{
				pointsText.color = Color.Lerp(pointsText.color, Color.red, 0.19999999f);
				yield return 0f;
			}
			while ((double)pointsText.color.g < 0.95)
			{
				pointsText.color = Color.Lerp(pointsText.color, Color.white, 0.19999999f);
				yield return 0f;
			}
			isHighlightingPoints = false;
		}
	}

	private IEnumerator<float> _DoPortalSetupAnimation()
	{
		while (portalPrefab == null)
		{
			portalPrefab = GameObject.Find("SCP106_PORTAL");
			yield return 0f;
		}
		Animator portalAnim = portalPrefab.GetComponent<Animator>();
		portalAnim.SetBool("activated", false);
		yield return Timing.WaitForSeconds(1f);
		portalPrefab.transform.position = portalPosition;
		portalAnim.SetBool("activated", true);
	}

	[DebuggerHidden]
	[Server]
	private IEnumerator<float> _DoTeleportAnimation()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogWarning("[Server] function 'System.Collections.Generic.IEnumerator`1<System.Single> Scp106PlayerScript::_DoTeleportAnimation()' called on client");
			return null;
		}
		_003C_DoTeleportAnimation_003Ec__Iterator4 _003C_DoTeleportAnimation_003Ec__Iterator5 = new _003C_DoTeleportAnimation_003Ec__Iterator4();
		_003C_DoTeleportAnimation_003Ec__Iterator5._0024this = this;
		return _003C_DoTeleportAnimation_003Ec__Iterator5;
	}

	[ClientRpc]
	public void RpcTeleportAnimation()
	{
		Timing.RunCoroutine(_ClientTeleportAnimation(), Segment.FixedUpdate);
	}

	private IEnumerator<float> _ClientTeleportAnimation()
	{
		if (!(portalPrefab != null))
		{
			yield break;
		}
		if (base.isLocalPlayer)
		{
			goingViaThePortal = true;
			VignetteAndChromaticAberration vaca = GetComponentInChildren<VignetteAndChromaticAberration>();
			Recoil recoil = GetComponentInChildren<Recoil>();
			fpc.noclip = true;
			for (float i = 1f; i <= 175f; i += 1f)
			{
				recoil.positionOffset = -1.6f * (vaca.intensity = i / 175f);
				yield return 0f;
			}
			for (float i2 = 1f; i2 <= 25f; i2 += 1f)
			{
				yield return 0f;
			}
			for (float i3 = 1f; i3 <= 150f; i3 += 1f)
			{
				recoil.positionOffset = -1.6f * (vaca.intensity = 1f - i3 / 150f);
				yield return 0f;
			}
			vaca.intensity = 0.036f;
			recoil.positionOffset = 0f;
			fpc.noclip = false;
			goingViaThePortal = false;
		}
		else
		{
			GetComponent<AnimationController>().animator.SetTrigger("Teleporting");
		}
	}

	[Command(channel = 4)]
	private void CmdMakePortal()
	{
		if (GetComponent<FallDamage>().isGrounded)
		{
			UnityEngine.Debug.DrawRay(base.transform.position, -base.transform.up, Color.red, 10f);
			RaycastHit hitInfo;
			if (iAm106 && !goingViaThePortal && Physics.Raycast(new Ray(base.transform.position, -base.transform.up), out hitInfo, 10f, teleportPlacementMask))
			{
				SetPortalPosition(hitInfo.point - Vector3.up);
			}
		}
	}

	[Command(channel = 4)]
	public void CmdUsePortal()
	{
		if (GetComponent<FallDamage>().isGrounded && iAm106 && portalPosition != Vector3.zero && !goingViaThePortal)
		{
			Timing.RunCoroutine(_DoTeleportAnimation(), Segment.Update);
		}
	}

	[Command(channel = 2)]
	private void CmdMovePlayer(GameObject ply, int t)
	{
		if (!ServerTime.CheckSynchronization(t) || !iAm106 || !(Vector3.Distance(GetComponent<PlyMovementSync>().position, ply.transform.position) < 3f) || !ply.GetComponent<CharacterClassManager>().IsHuman())
		{
			return;
		}
		CharacterClassManager component = ply.GetComponent<CharacterClassManager>();
		if (!component.GodMode && component.klasy[component.curClass].team != Team.SCP)
		{
			GetComponent<CharacterClassManager>().CallRpcPlaceBlood(ply.transform.position, 1, 2f);
			if (blastDoor.isClosed)
			{
				GetComponent<CharacterClassManager>().CallRpcPlaceBlood(ply.transform.position, 1, 2f);
				GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(500f, GetComponent<NicknameSync>().myNick + " (" + GetComponent<CharacterClassManager>().SteamId + ")", DamageTypes.Scp106, GetComponent<QueryProcessor>().PlayerId), ply);
			}
			else
			{
				GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(40f, GetComponent<NicknameSync>().myNick + " (" + GetComponent<CharacterClassManager>().SteamId + ")", DamageTypes.Scp106, GetComponent<QueryProcessor>().PlayerId), ply);
				ply.GetComponent<PlyMovementSync>().SetPosition(Vector3.down * 1997f);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!base.isLocalPlayer || ccm.curClass != 3)
		{
			return;
		}
		Door componentInParent = other.GetComponentInParent<Door>();
		if (!(componentInParent == null))
		{
			doorCurrentlyIn = componentInParent;
			isCollidingDoorOpen = false;
			fpc.m_WalkSpeed = 1f;
			fpc.m_RunSpeed = 1f;
			if (componentInParent.isOpen && componentInParent.curCooldown <= 0f)
			{
				fpc.m_WalkSpeed = ccm.klasy[ccm.curClass].walkSpeed;
				fpc.m_RunSpeed = ccm.klasy[ccm.curClass].runSpeed;
				isCollidingDoorOpen = true;
			}
		}
	}

	private void ExitDoor()
	{
		if (base.isLocalPlayer && ccm.curClass == 3)
		{
			fpc.m_WalkSpeed = ccm.klasy[ccm.curClass].walkSpeed;
			fpc.m_RunSpeed = ccm.klasy[ccm.curClass].runSpeed;
			doorCurrentlyIn = null;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		ExitDoor();
	}

	private void DoorCollisionCheck()
	{
		if (doorCurrentlyIn != null && doorCurrentlyIn.destroyed)
		{
			ExitDoor();
		}
		else if (!isCollidingDoorOpen && doorCurrentlyIn != null && doorCurrentlyIn.isOpen && doorCurrentlyIn.curCooldown <= 0f && !isCollidingDoorOpen)
		{
			fpc.m_WalkSpeed = ccm.klasy[ccm.curClass].walkSpeed;
			fpc.m_RunSpeed = ccm.klasy[ccm.curClass].runSpeed;
			isCollidingDoorOpen = true;
		}
		else if (isCollidingDoorOpen && doorCurrentlyIn != null && !doorCurrentlyIn.isOpen)
		{
			isCollidingDoorOpen = false;
			fpc.m_WalkSpeed = 1f;
			fpc.m_RunSpeed = 1f;
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdMakePortal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdMakePortal called on client.");
		}
		else
		{
			((Scp106PlayerScript)obj).CmdMakePortal();
		}
	}

	protected static void InvokeCmdCmdUsePortal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdUsePortal called on client.");
		}
		else
		{
			((Scp106PlayerScript)obj).CmdUsePortal();
		}
	}

	protected static void InvokeCmdCmdMovePlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("Command CmdMovePlayer called on client.");
		}
		else
		{
			((Scp106PlayerScript)obj).CmdMovePlayer(reader.ReadGameObject(), (int)reader.ReadPackedUInt32());
		}
	}

	public void CallCmdMakePortal()
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("Command function CmdMakePortal called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdMakePortal();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdMakePortal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 4, "CmdMakePortal");
	}

	public void CallCmdUsePortal()
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("Command function CmdUsePortal called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUsePortal();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUsePortal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 4, "CmdUsePortal");
	}

	public void CallCmdMovePlayer(GameObject ply, int t)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("Command function CmdMovePlayer called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdMovePlayer(ply, t);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdMovePlayer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(ply);
		networkWriter.WritePackedUInt32((uint)t);
		SendCommandInternal(networkWriter, 2, "CmdMovePlayer");
	}

	protected static void InvokeRpcRpcContainAnimation(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcContainAnimation called on server.");
		}
		else
		{
			((Scp106PlayerScript)obj).RpcContainAnimation();
		}
	}

	protected static void InvokeRpcRpcTeleportAnimation(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			UnityEngine.Debug.LogError("RPC RpcTeleportAnimation called on server.");
		}
		else
		{
			((Scp106PlayerScript)obj).RpcTeleportAnimation();
		}
	}

	public void CallRpcContainAnimation()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("RPC Function RpcContainAnimation called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcContainAnimation);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcContainAnimation");
	}

	public void CallRpcTeleportAnimation()
	{
		if (!NetworkServer.active)
		{
			UnityEngine.Debug.LogError("RPC Function RpcTeleportAnimation called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcTeleportAnimation);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcTeleportAnimation");
	}

	static Scp106PlayerScript()
	{
		kCmdCmdMakePortal = 582440253;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Scp106PlayerScript), kCmdCmdMakePortal, InvokeCmdCmdMakePortal);
		kCmdCmdUsePortal = 1611005744;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Scp106PlayerScript), kCmdCmdUsePortal, InvokeCmdCmdUsePortal);
		kCmdCmdMovePlayer = -1259313323;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Scp106PlayerScript), kCmdCmdMovePlayer, InvokeCmdCmdMovePlayer);
		kRpcRpcContainAnimation = -1083358231;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Scp106PlayerScript), kRpcRpcContainAnimation, InvokeRpcRpcContainAnimation);
		kRpcRpcTeleportAnimation = 660537568;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Scp106PlayerScript), kRpcRpcTeleportAnimation, InvokeRpcRpcTeleportAnimation);
		NetworkCRC.RegisterBehaviour("Scp106PlayerScript", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(ultimatePoints);
			writer.Write(portalPosition);
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
			writer.Write(ultimatePoints);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(portalPosition);
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
			ultimatePoints = reader.ReadSingle();
			portalPosition = reader.ReadVector3();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			ultimatePoints = reader.ReadSingle();
		}
		if ((num & 2) != 0)
		{
			SetPortalPosition(reader.ReadVector3());
		}
	}
}
