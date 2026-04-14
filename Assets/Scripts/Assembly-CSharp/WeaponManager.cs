using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MEC;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

public class WeaponManager : NetworkBehaviour
{
	[Serializable]
	public class Weapon
	{
		[Serializable]
		public class WeaponMod
		{
			[Serializable]
			public class WeaponModEffects
			{
				[Header("Sights only effects")]
				public PostProcessingProfile customProfile;

				[Tooltip("FOV")]
				public float zoomFov = 70f;

				public float weaponCameraFov = 60f;

				[Tooltip("RECOIL SCALE")]
				public float zoomRecoilReduction = 1f;

				[Tooltip("WALK SLOW")]
				public float zoomSlowdown = 1f;

				[Tooltip("SENSITIVITY")]
				public float zoomSensitivity = 1f;

				[Tooltip("RECOIL ANIMATION SCALE")]
				public float zoomRecoilAnimScale = 1f;

				public Vector3 zoomPositionOffset = Vector3.zero;

				[Header("Barrels only effects")]
				public AudioClip shootSound;

				public float damageMultiplier = 1f;

				public float audioSourceRangeScale = 1f;

				public float firerateMultiplier = 1f;

				[Header("Ammo Counter Effects")]
				public Text counterText;

				public string counterTemplate;

				[Header("Mixed effects")]
				public float overallRecoilReduction = 1f;

				public float unfocusedSpread = 1f;

				public GameObject laserDirection;
			}

			public string name;

			public GameObject prefab_firstperson;

			public GameObject prefab_thirdperson;

			public WeaponModEffects effects;

			public Texture icon;

			public bool isActive;

			public void SetVisibility(bool b)
			{
				isActive = b;
				if (prefab_firstperson != null)
				{
					prefab_firstperson.SetActive(b);
				}
				if (prefab_thirdperson != null)
				{
					prefab_thirdperson.SetActive(b);
				}
			}
		}

		[Header("Misc properties")]
		public int inventoryID;

		public RecoilProperties recoil;

		public AnimationCurve damageOverDistance;

		public float shotsPerSecond;

		public bool allowFullauto;

		public Vector3 positionOffset;

		public Vector3 additionalUnfocusedOffset;

		public GameObject holeEffect;

		public ParticleSystem husks;

		public float recoilAnimation = 0.5f;

		public float bobAnimationScale = 1f;

		public float timeToPickup = 0.5f;

		public bool useProceduralPickupAnimation = true;

		public string[] customShootAnims;

		public string[] customZoomshotAnims;

		public string[] reloadingAnims;

		public string[] reloadingNoammoAnims;

		public string customShootAnimNoammo;

		public string customZoomshotAnimNoammo;

		public float maxAngleLaser;

		[Header("Ammo & reloading")]
		public AudioClip reloadClip;

		public int maxAmmo;

		public int ammoType;

		public float reloadingTime;

		[Header("Zooming")]
		public bool allowZoom;

		public float zoomingTime;

		public float unfocusedSpread = 5f;

		[Header("Mods")]
		public WeaponMod[] mod_sights;

		public WeaponMod[] mod_barrels;

		public WeaponMod[] mod_others;

		public WeaponMod.WeaponModEffects allEffects;

		public WeaponMod.WeaponModEffects GetAllEffects(int[] mods)
		{
			allEffects = new WeaponMod.WeaponModEffects
			{
				customProfile = mod_sights[mods[0]].effects.customProfile,
				zoomRecoilReduction = mod_sights[mods[0]].effects.zoomRecoilReduction,
				zoomFov = mod_sights[mods[0]].effects.zoomFov,
				weaponCameraFov = mod_sights[mods[0]].effects.weaponCameraFov,
				zoomSlowdown = mod_sights[mods[0]].effects.zoomSlowdown,
				zoomSensitivity = mod_sights[mods[0]].effects.zoomSensitivity,
				zoomPositionOffset = mod_sights[mods[0]].effects.zoomPositionOffset,
				shootSound = mod_barrels[mods[1]].effects.shootSound,
				firerateMultiplier = mod_barrels[mods[1]].effects.firerateMultiplier,
				zoomRecoilAnimScale = mod_sights[mods[0]].effects.zoomRecoilAnimScale,
				damageMultiplier = mod_barrels[mods[1]].effects.damageMultiplier,
				overallRecoilReduction = mod_sights[mods[0]].effects.overallRecoilReduction * mod_barrels[mods[1]].effects.overallRecoilReduction * mod_others[mods[2]].effects.overallRecoilReduction,
				unfocusedSpread = mod_sights[mods[0]].effects.unfocusedSpread * mod_barrels[mods[1]].effects.unfocusedSpread * mod_others[mods[2]].effects.unfocusedSpread,
				laserDirection = mod_others[mods[2]].effects.laserDirection,
				audioSourceRangeScale = mod_barrels[mods[1]].effects.audioSourceRangeScale,
				counterText = mod_others[mods[2]].effects.counterText,
				counterTemplate = mod_others[mods[2]].effects.counterTemplate
			};
			return allEffects;
		}

		public void RefreshMods(int[] mods, bool _flashlight)
		{
			for (int i = 0; i < mod_sights.Length; i++)
			{
				mod_sights[i].SetVisibility(i == mods[0]);
			}
			for (int j = 0; j < mod_barrels.Length; j++)
			{
				mod_barrels[j].SetVisibility(j == mods[1]);
			}
			for (int k = 0; k < mod_others.Length; k++)
			{
				mod_others[k].SetVisibility(k == mods[2]);
				if (k != mods[2] || !mod_others[k].name.ToLower().Contains("flashlight"))
				{
					continue;
				}
				if (mod_others[k].prefab_firstperson != null)
				{
					Light[] componentsInChildren = mod_others[k].prefab_firstperson.GetComponentsInChildren<Light>();
					foreach (Light light in componentsInChildren)
					{
						light.enabled = _flashlight;
					}
				}
				if (mod_others[k].prefab_thirdperson != null)
				{
					Light[] componentsInChildren2 = mod_others[k].prefab_thirdperson.GetComponentsInChildren<Light>();
					foreach (Light light2 in componentsInChildren2)
					{
						light2.enabled = _flashlight;
					}
				}
			}
			allEffects = GetAllEffects(mods);
		}

		private string ConvertToStat(int value, bool lessTheBetter)
		{
			string empty = string.Empty;
			bool flag = false;
			if (value < 0)
			{
				flag = lessTheBetter;
				empty = "-" + Mathf.Abs(value) + "%";
			}
			else
			{
				flag = !lessTheBetter;
				empty = "+" + Mathf.Abs(value) + "%";
			}
			string text = ((!flag) ? "red" : "green");
			return "<color=" + text + ">" + empty + "</color>";
		}

		public string GetStats(ModPrefab.ModType type, int id)
		{
			string text = string.Empty;
			switch (type)
			{
			case ModPrefab.ModType.Barrel:
			{
				int num6 = Mathf.RoundToInt((mod_barrels[id].effects.damageMultiplier - 1f) * 100f);
				int num7 = Mathf.RoundToInt((mod_barrels[id].effects.audioSourceRangeScale - 1f) * 100f);
				int num8 = Mathf.RoundToInt((mod_barrels[id].effects.overallRecoilReduction - 1f) * 100f);
				int num9 = Mathf.RoundToInt((mod_barrels[id].effects.firerateMultiplier - 1f) * 100f);
				if (num6 != 0)
				{
					text = text + "Damage " + ConvertToStat(num6, false) + "\n";
				}
				if (num7 != 0)
				{
					text = text + "Shot loudness " + ConvertToStat(num7, true) + "\n";
				}
				if (num8 != 0)
				{
					text = text + "Recoil " + ConvertToStat(num8, true) + "\n";
				}
				if (num9 != 0)
				{
					text = text + "Fire rate " + ConvertToStat(num9, false) + "\n";
				}
				break;
			}
			case ModPrefab.ModType.Sight:
			{
				int num3 = Mathf.RoundToInt((mod_sights[id].effects.zoomRecoilReduction - 1f) * 100f);
				bool flag = mod_sights[id].effects.customProfile != null;
				float num4 = ((!flag) ? (Mathf.Round(70f / mod_sights[id].effects.zoomFov * 100f) / 100f) : 1f);
				int num5 = Mathf.RoundToInt((mod_sights[id].effects.overallRecoilReduction - 1f) * 100f);
				if (num3 != 0)
				{
					text = text + "Recoil (while aiming) " + ConvertToStat(num3, true) + "\n";
				}
				if (flag)
				{
					text += "<color=green>Telescopic-type sight</color>\n";
				}
				if (num4 != 1f)
				{
					string text2 = text;
					text = text2 + "Zoom scale <color=green>" + num4 + "</color>";
				}
				if (num5 != 0)
				{
					text = text + "Recoil " + ConvertToStat(num5, true) + "\n";
				}
				break;
			}
			default:
			{
				int num = Mathf.RoundToInt((mod_others[id].effects.overallRecoilReduction - 1f) * 100f);
				int num2 = Mathf.RoundToInt((mod_others[id].effects.unfocusedSpread - 1f) * 100f);
				if (num != 0)
				{
					text = text + "Recoil " + ConvertToStat(num, true) + "\n";
				}
				if (num2 != 0)
				{
					text = text + "Bullet spread (without aiming)  " + ConvertToStat(num2, true) + "\n";
				}
				break;
			}
			}
			if (string.IsNullOrEmpty(text))
			{
				text = "No effects";
			}
			return text;
		}
	}

	[SyncVar]
	public bool friendlyFire;

	[SyncVar(hook = "HookCurWeapon")]
	public int curWeapon = -1;

	private CharacterClassManager ccm;

	private BloodDrawer drawer;

	private Inventory inv;

	private AmmoBox abox;

	private WeaponShootAnimation weaponShootAnimation;

	private FirstPersonController fpc;

	private AnimationController animationController;

	private KeyCode kc_fire;

	private KeyCode kc_reload;

	private KeyCode kc_zoom;

	private float fireCooldown;

	private float reloadCooldown;

	private float zoomCooldown;

	private Light globalLightSource;

	public float normalFov = 70f;

	public Transform camera;

	public Transform weaponInventoryGroup;

	public Camera weaponModelCamera;

	public float fovAdjustingSpeed;

	public bool zoomed;

	public AnimationCurve viewBob;

	public float overallDamagerFactor = 1.65f;

	public LayerMask raycastMask;

	public LayerMask raycastServerMask;

	public LayerMask bloodMask;

	public HitboxIdentity[] hitboxes;

	public Weapon[] weapons;

	public WeaponLaser laserLight;

	public Transform globalLight;

	public int[,] modPreferences;

	private bool prevFlash;

	private int prevSyncGun = -1;

	public bool flashlightEnabled = true;

	[SyncVar]
	private bool syncFlash;

	public bool forceSyncModsNextFrame;

	private bool firstSet = true;

	private static int kCmdCmdSyncFlash;

	private static int kCmdCmdChangeModPreferences;

	private static int kCmdCmdShoot;

	private static int kCmdCmdReload;

	private static int kRpcRpcPlaceDecal;

	private static int kRpcRpcConfirmShot;

	private static int kRpcRpcReload;

	public bool NetworkfriendlyFire
	{
		get
		{
			return friendlyFire;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref friendlyFire, 1u);
		}
	}

	public int NetworkcurWeapon
	{
		get
		{
			return curWeapon;
		}
		[param: In]
		set
		{
			WeaponManager weaponManager = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				HookCurWeapon(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref weaponManager.curWeapon, 2u);
		}
	}

	public bool NetworksyncFlash
	{
		get
		{
			return syncFlash;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref syncFlash, 4u);
		}
	}

	private void HookCurWeapon(int i)
	{
		NetworkcurWeapon = i;
	}

	private void Start()
	{
		abox = GetComponent<AmmoBox>();
		fpc = GetComponent<FirstPersonController>();
		inv = GetComponent<Inventory>();
		animationController = GetComponent<AnimationController>();
		weaponShootAnimation = GetComponentInChildren<WeaponShootAnimation>();
		drawer = GetComponent<BloodDrawer>();
		NetworkfriendlyFire = ServerConsole.FriendlyFire;
		ccm = GetComponent<CharacterClassManager>();
		kc_fire = NewInput.GetKey("Shoot");
		kc_reload = NewInput.GetKey("Reload");
		kc_zoom = NewInput.GetKey("Zoom");
		globalLightSource = globalLight.GetComponentInChildren<Light>();
		if (base.isLocalPlayer)
		{
			string text = string.Empty;
			for (int i = 0; i < weapons.Length; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					text = text + PlayerPrefs.GetInt("W_" + i + "_" + j) + ":";
				}
				text += "#";
			}
			CallCmdChangeModPreferences(text);
		}
		else
		{
			UnityEngine.Object.Destroy(weaponModelCamera.gameObject);
		}
	}

	private void Update()
	{
		DeductCooldown();
		if (base.isLocalPlayer)
		{
			CheckForInput();
			DoLaserStuff();
			UpdateFov();
			SetupCameras();
			RefreshPositions();
		}
	}

	private void LateUpdate()
	{
		if (base.isLocalPlayer && !Cursor.visible && Input.GetKeyDown(NewInput.GetKey("Toggle flashlight")) && curWeapon >= 0 && weapons[curWeapon].mod_others[inv.GetItemInHand().modOther].name.ToLower().Contains("flashlight"))
		{
			flashlightEnabled = !flashlightEnabled;
		}
		UpdateGlobalLight();
		RefreshMods();
	}

	private void RefreshMods()
	{
		if (curWeapon < 0)
		{
			prevSyncGun = -1;
			return;
		}
		bool flag = false;
		Inventory.SyncItemInfo itemInHand = inv.GetItemInHand();
		if (curWeapon != prevSyncGun)
		{
			prevSyncGun = curWeapon;
			flag = true;
		}
		else if (!base.isLocalPlayer)
		{
			try
			{
				if (prevFlash != syncFlash)
				{
					flag = true;
					prevFlash = syncFlash;
				}
				else if (!weapons[curWeapon].mod_sights[itemInHand.modSight].prefab_thirdperson.activeSelf)
				{
					flag = true;
				}
				else if (!weapons[curWeapon].mod_barrels[itemInHand.modBarrel].prefab_thirdperson.activeSelf)
				{
					flag = true;
				}
				else if (!weapons[curWeapon].mod_others[itemInHand.modOther].prefab_thirdperson.activeSelf)
				{
					flag = true;
				}
			}
			catch
			{
			}
		}
		else if (prevFlash != flashlightEnabled)
		{
			flag = true;
			prevFlash = flashlightEnabled;
		}
		else if (!weapons[curWeapon].mod_sights[itemInHand.modSight].prefab_firstperson.activeSelf)
		{
			flag = true;
		}
		else if (!weapons[curWeapon].mod_barrels[itemInHand.modBarrel].prefab_firstperson.activeSelf)
		{
			flag = true;
		}
		else if (!weapons[curWeapon].mod_others[itemInHand.modOther].prefab_firstperson.activeSelf)
		{
			flag = true;
		}
		if (flag)
		{
			if (base.isLocalPlayer)
			{
				CallCmdSyncFlash(flashlightEnabled);
			}
			weapons[curWeapon].RefreshMods(new int[3] { itemInHand.modSight, itemInHand.modBarrel, itemInHand.modOther }, (!base.isLocalPlayer) ? syncFlash : flashlightEnabled);
		}
	}

	private void UpdateGlobalLight()
	{
		if (globalLightSource != null)
		{
			globalLightSource.intensity = Mathf.Lerp(0.1f, 0.22f, (Vector3.Dot(base.transform.forward, Vector3.forward) + 1f) / 2f);
		}
	}

	[Command]
	private void CmdSyncFlash(bool b)
	{
		NetworksyncFlash = b;
	}

	private void DeductCooldown()
	{
		if (fireCooldown >= 0f)
		{
			fireCooldown -= Time.deltaTime;
		}
		if (reloadCooldown >= 0f)
		{
			reloadCooldown -= Time.deltaTime;
		}
		if (zoomCooldown >= 0f)
		{
			zoomCooldown -= Time.deltaTime;
		}
	}

	[ClientCallback]
	private void DoLaserStuff()
	{
		if (NetworkClient.active)
		{
			if (curWeapon < 0)
			{
				laserLight.forwardDirection = null;
				return;
			}
			laserLight.maxAngle = weapons[curWeapon].maxAngleLaser;
			laserLight.forwardDirection = weapons[curWeapon].allEffects.laserDirection;
		}
	}

	[ClientCallback]
	private void UpdateFov()
	{
		if (NetworkClient.active)
		{
			float zoomFov = normalFov;
			float num = normalFov - 10f;
			bool flag = curWeapon >= 0 && weapons[curWeapon].allEffects.customProfile != null;
			if (curWeapon >= 0 && zoomed && !flag)
			{
				zoomFov = weapons[curWeapon].allEffects.zoomFov;
				num = weapons[curWeapon].allEffects.weaponCameraFov;
			}
			Camera component = camera.GetComponent<Camera>();
			component.fieldOfView = ((!flag) ? Mathf.Lerp(component.fieldOfView, zoomFov, Time.deltaTime * fovAdjustingSpeed) : zoomFov);
			weaponModelCamera.fieldOfView = ((!flag) ? Mathf.Lerp(weaponModelCamera.fieldOfView, num, Time.deltaTime * fovAdjustingSpeed) : num);
		}
	}

	[ClientCallback]
	private void RefreshPositions()
	{
		if (NetworkClient.active && curWeapon >= 0)
		{
			Vector3 positionOffset = weapons[curWeapon].positionOffset;
			if (zoomed)
			{
				positionOffset += weapons[curWeapon].allEffects.zoomPositionOffset;
			}
			else
			{
				positionOffset += camera.transform.localPosition * (viewBob.Evaluate(new Vector3(fpc.m_MoveDir.x, 0f, fpc.m_MoveDir.z).magnitude) * weapons[curWeapon].bobAnimationScale) + weapons[curWeapon].additionalUnfocusedOffset;
			}
			weaponInventoryGroup.localPosition = Vector3.Lerp(weaponInventoryGroup.localPosition, positionOffset, Time.deltaTime * 8f);
		}
	}

	[ClientCallback]
	private void SetZoom(bool b)
	{
		if (!NetworkClient.active)
		{
			return;
		}
		bool flag = false;
		if (curWeapon >= 0 && weapons[curWeapon].allowZoom && Inventory.inventoryCooldown <= ((!inv.WeaponReadyToInstantPickup()) ? (0f - weapons[curWeapon].timeToPickup) : 0f))
		{
			if (b != zoomed && fireCooldown <= 0f)
			{
				fireCooldown += weapons[curWeapon].zoomingTime;
				zoomCooldown = weapons[curWeapon].zoomingTime;
				zoomed = b;
				flag = true;
			}
		}
		else if (zoomed)
		{
			flag = true;
			zoomed = false;
		}
		if (curWeapon >= 0)
		{
			if (flag)
			{
				inv.availableItems[inv.curItem].firstpersonModel.GetComponent<Animator>().SetBool("Zoomed", zoomed);
				fpc.zoomSlowdown = ((!zoomed) ? 1f : weapons[curWeapon].allEffects.zoomSlowdown);
			}
		}
		else
		{
			fpc.zoomSlowdown = 1f;
		}
	}

	public int AmmoLeft()
	{
		if (curWeapon >= 0)
		{
			return (int)inv.items[inv.GetItemIndex()].durability;
		}
		return -1;
	}

	[ClientCallback]
	private void SetupCameras()
	{
		if (!NetworkClient.active)
		{
			return;
		}
		fpc.m_MouseLook.sensitivityMultiplier = 1f;
		if (ccm.curClass < 0)
		{
			return;
		}
		weaponModelCamera.nearClipPlane = 0.01f;
		PostProcessingBehaviour component = weaponModelCamera.GetComponent<PostProcessingBehaviour>();
		component.profile = ccm.klasy[ccm.curClass].postprocessingProfile;
		if (curWeapon >= 0)
		{
			if (weapons[curWeapon].allEffects.counterText != null)
			{
				weapons[curWeapon].allEffects.counterText.text = string.Format(weapons[curWeapon].allEffects.counterTemplate, AmmoLeft(), weapons[curWeapon].maxAmmo, abox.GetAmmo(weapons[curWeapon].ammoType)).Replace("\\n", Environment.NewLine);
			}
			if (zoomed && zoomCooldown <= 0f)
			{
				fpc.m_MouseLook.sensitivityMultiplier = weapons[curWeapon].allEffects.zoomSensitivity;
				if (weapons[curWeapon].allEffects.customProfile != null)
				{
					component.profile = weapons[curWeapon].allEffects.customProfile;
					camera.GetComponent<Camera>().fieldOfView = weapons[curWeapon].allEffects.zoomFov;
					weaponModelCamera.nearClipPlane = 3.5f;
				}
			}
			Animator component2 = inv.availableItems[inv.curItem].firstpersonModel.GetComponent<Animator>();
			component2.SetBool("Noammo", inv.items[inv.GetItemIndex()].durability == 0f);
		}
		Inventory.targetCrosshairAlpha = ((!zoomed && (curWeapon < 0 || !(weapons[curWeapon].allEffects.laserDirection != null)) && !Cursor.visible) ? 1 : 0);
	}

	[ClientCallback]
	private void CheckForInput()
	{
		if (!NetworkClient.active)
		{
			return;
		}
		if (!Cursor.visible && Inventory.inventoryCooldown <= 0f && fireCooldown <= 0f && (reloadCooldown <= 0f || zoomed))
		{
			SetZoom(Input.GetKey(NewInput.GetKey("Zoom")));
		}
		if (curWeapon >= 0 && reloadCooldown <= 0f && !Cursor.visible && Inventory.inventoryCooldown <= ((!inv.WeaponReadyToInstantPickup()) ? (0f - weapons[curWeapon].timeToPickup) : 0f) && fireCooldown <= 0f)
		{
			if ((!weapons[curWeapon].allowFullauto) ? Input.GetKeyDown(kc_fire) : Input.GetKey(kc_fire))
			{
				Shoot();
			}
			else if (Input.GetKey(kc_reload))
			{
				Timing.RunCoroutine(_Reload(), Segment.FixedUpdate);
			}
		}
	}

	[ClientCallback]
	private void Shoot()
	{
		if (!NetworkClient.active || inv.items[inv.GetItemIndex()].durability == 0f)
		{
			return;
		}
		fireCooldown = 1f / (weapons[curWeapon].shotsPerSecond * weapons[curWeapon].allEffects.firerateMultiplier);
		Animator component = inv.availableItems[inv.curItem].firstpersonModel.GetComponent<Animator>();
		if (zoomed)
		{
			if (inv.items[inv.GetItemIndex()].durability == 1f && weapons[curWeapon].customZoomshotAnimNoammo != string.Empty)
			{
				component.Play(weapons[curWeapon].customZoomshotAnimNoammo, 0, 0f);
			}
			else if (weapons[curWeapon].customZoomshotAnims.Length != 0)
			{
				component.Play(weapons[curWeapon].customZoomshotAnims[UnityEngine.Random.Range(0, weapons[curWeapon].customZoomshotAnims.Length)], 0, 0f);
			}
		}
		else if (inv.items[inv.GetItemIndex()].durability == 1f && weapons[curWeapon].customShootAnimNoammo != string.Empty)
		{
			component.Play(weapons[curWeapon].customShootAnimNoammo, 0, 0f);
		}
		else if (weapons[curWeapon].customShootAnims.Length != 0)
		{
			component.Play(weapons[curWeapon].customShootAnims[UnityEngine.Random.Range(0, weapons[curWeapon].customShootAnims.Length)], 0, 0f);
		}
		animationController.gunSource.PlayOneShot(weapons[curWeapon].allEffects.shootSound);
		float num = weapons[curWeapon].allEffects.overallRecoilReduction * weapons[curWeapon].recoil.fovKick;
		camera.GetComponent<Camera>().fieldOfView -= num;
		weaponModelCamera.GetComponent<Camera>().fieldOfView += num;
		PlayMuzzleFlashes(true, curWeapon);
		weapons[curWeapon].husks.Play();
		Vector3 vector = camera.transform.forward;
		if (!zoomed)
		{
			vector = Quaternion.Euler(new Vector3(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1)) * (weapons[curWeapon].unfocusedSpread * weapons[curWeapon].allEffects.unfocusedSpread / 5f)) * vector;
		}
		Ray ray = new Ray(camera.transform.position, vector);
		RaycastHit hitInfo;
		if (Physics.Raycast(ray, out hitInfo, 500f, raycastMask))
		{
			BreakableWindow component2 = hitInfo.collider.GetComponent<BreakableWindow>();
			HitboxIdentity component3 = hitInfo.collider.GetComponent<HitboxIdentity>();
			if (component3 != null)
			{
				DoRecoil();
				CallCmdShoot(component3.GetComponentInParent<NetworkIdentity>().gameObject, component3.id, ray.direction, camera.transform.position, hitInfo.point);
			}
			else if (component2 != null)
			{
				DoRecoil();
				CallCmdShoot(component2.GetComponent<NetworkIdentity>().gameObject, "window", ray.direction, Vector3.zero, Vector3.zero);
			}
			else
			{
				DoRecoil();
				CallCmdShoot(null, "hole", ray.direction, Vector3.zero, Vector3.zero);
			}
		}
		else
		{
			DoRecoil();
			CallCmdShoot(null, string.Empty, Vector3.zero, Vector3.zero, Vector3.zero);
		}
	}

	private void DoRecoil()
	{
		weaponShootAnimation.Recoil(weapons[curWeapon].recoilAnimation * ((!zoomed) ? 1f : weapons[curWeapon].allEffects.zoomRecoilAnimScale));
		Recoil.StaticDoRecoil(weapons[curWeapon].recoil, weapons[curWeapon].allEffects.overallRecoilReduction * ((!zoomed) ? 1f : weapons[curWeapon].allEffects.zoomRecoilReduction));
	}

	[Command]
	public void CmdChangeModPreferences(string info)
	{
		if (firstSet)
		{
			try
			{
				modPreferences = new int[weapons.Length, 3];
				firstSet = false;
				for (int i = 0; i < weapons.Length; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						modPreferences[i, j] = int.Parse(info.Split('#')[i].Split(':')[j]);
					}
				}
				return;
			}
			catch
			{
				Debug.Log("Mods unsuccessfully loaded.");
				return;
			}
		}
		WorkStation[] array = UnityEngine.Object.FindObjectsOfType<WorkStation>();
		try
		{
			WorkStation[] array2 = array;
			foreach (WorkStation workStation in array2)
			{
				if (Vector3.Distance(workStation.transform.position, base.transform.position) < 5f && workStation.isTabletConnected)
				{
					int[] array3 = new int[3];
					for (int l = 0; l < 3; l++)
					{
						array3[l] = int.Parse(info.Split(':')[l]);
					}
					modPreferences[array3[0], array3[1]] = array3[2];
					if (array3[1] == 0)
					{
						inv.items.ModifyAttachments(inv.GetItemIndex(), array3[2], inv.GetItemInHand().modBarrel, inv.GetItemInHand().modOther);
					}
					if (array3[1] == 1)
					{
						inv.items.ModifyAttachments(inv.GetItemIndex(), inv.GetItemInHand().modSight, array3[2], inv.GetItemInHand().modOther);
					}
					if (array3[1] == 2)
					{
						inv.items.ModifyAttachments(inv.GetItemIndex(), inv.GetItemInHand().modSight, inv.GetItemInHand().modBarrel, array3[2]);
					}
				}
			}
		}
		catch
		{
			Debug.Log("Mods unsuccessfully loaded.");
		}
	}

	[Command]
	private void CmdShoot(GameObject target, string hitboxType, Vector3 dir, Vector3 sourcePos, Vector3 targetPos)
	{
		if (curWeapon < 0 || ((reloadCooldown > 0f || fireCooldown > 0f) && !base.isLocalPlayer) || inv.curItem != weapons[curWeapon].inventoryID || inv.items[inv.GetItemIndex()].durability <= 0f)
		{
			return;
		}
		inv.items.ModifyDuration(inv.GetItemIndex(), inv.items[inv.GetItemIndex()].durability - 1f);
		fireCooldown = 1f / (weapons[curWeapon].shotsPerSecond * weapons[curWeapon].allEffects.firerateMultiplier) * 0.8f;
		CharacterClassManager characterClassManager = null;
		if (target != null)
		{
			characterClassManager = target.GetComponent<CharacterClassManager>();
		}
		float audioSourceRangeScale = weapons[curWeapon].allEffects.audioSourceRangeScale;
		audioSourceRangeScale = audioSourceRangeScale / 2f * 70f;
		GetComponent<Scp939_VisionController>().MakeNoise(Mathf.Clamp(audioSourceRangeScale, 5f, 100f));
		if (target != null && hitboxType == "window" && target.GetComponent<BreakableWindow>() != null)
		{
			float time = Vector3.Distance(camera.transform.position, target.transform.position);
			float damage = weapons[curWeapon].damageOverDistance.Evaluate(time);
			target.GetComponent<BreakableWindow>().ServerDamageWindow(damage);
			CallRpcConfirmShot(true, curWeapon);
		}
		else if (characterClassManager != null && GetShootPermission(characterClassManager))
		{
			if (Math.Abs(camera.transform.position.y - characterClassManager.transform.position.y) > 40f)
			{
				GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Shot rejected - Code 2.1 (too big Y-axis difference between source and target)", "gray");
				return;
			}
			if (Vector3.Distance(camera.transform.position, sourcePos) > 6.5f)
			{
				GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Shot rejected - Code 2.2 (difference between real source position and provided source position is too big)", "gray");
				return;
			}
			if (Vector3.Distance(characterClassManager.transform.position, targetPos) > 6.5f)
			{
				GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Shot rejected - Code 2.3 (difference between real target position and provided target position is too big)", "gray");
				return;
			}
			if (Physics.Linecast(sourcePos, targetPos, raycastServerMask))
			{
				GetComponent<CharacterClassManager>().TargetConsolePrint(base.connectionToClient, "Shot rejected - Code 2.4 (collision detected)", "gray");
				return;
			}
			float num = Vector3.Distance(camera.transform.position, target.transform.position);
			float num2 = weapons[curWeapon].damageOverDistance.Evaluate(num);
			switch (hitboxType.ToUpper())
			{
			case "HEAD":
				num2 *= 4f;
				break;
			case "LEG":
				num2 /= 2f;
				break;
			case "SCP106":
				num2 /= 10f;
				break;
			}
			num2 *= weapons[curWeapon].allEffects.damageMultiplier;
			num2 *= overallDamagerFactor;
			bool flag = false;
			GameObject[] players = PlayerManager.singleton.players;
			foreach (GameObject gameObject in players)
			{
				if (gameObject.GetComponent<Handcuffs>().cuffTarget == base.gameObject)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(num2, ccm.GetComponent<NicknameSync>().myNick + " (" + ccm.SteamId + ")", DamageTypes.FromWeaponId(curWeapon), GetComponent<QueryProcessor>().PlayerId), characterClassManager.gameObject);
			}
			CallRpcConfirmShot(true, curWeapon);
			PlaceDecal(true, new Ray(camera.position, dir), characterClassManager.curClass, num);
		}
		else
		{
			PlaceDecal(false, new Ray(camera.position, dir), curWeapon, 0f);
			CallRpcConfirmShot(false, curWeapon);
		}
	}

	[Command]
	private void CmdReload(bool animationOnly)
	{
		if (curWeapon < 0 || inv.curItem != weapons[curWeapon].inventoryID || inv.items[inv.GetItemIndex()].durability >= (float)weapons[curWeapon].maxAmmo)
		{
			return;
		}
		if (animationOnly)
		{
			CallRpcReload(curWeapon);
			return;
		}
		int ammoType = weapons[curWeapon].ammoType;
		int num = abox.GetAmmo(ammoType);
		int num2 = (int)inv.items[inv.GetItemIndex()].durability;
		int maxAmmo = weapons[curWeapon].maxAmmo;
		while (num > 0 && num2 < maxAmmo)
		{
			num--;
			num2++;
		}
		inv.items.ModifyDuration(inv.GetItemIndex(), num2);
		abox.SetOneAmount(ammoType, num.ToString());
	}

	[ServerCallback]
	private void PlaceDecal(bool isBlood, Ray ray, int classId, float distanceAddition)
	{
		RaycastHit hitInfo;
		if (NetworkServer.active && Physics.Raycast(ray, out hitInfo, (!isBlood) ? 100f : (10f + distanceAddition), bloodMask) && classId >= 0)
		{
			CallRpcPlaceDecal(isBlood, (!isBlood) ? classId : ccm.klasy[classId].bloodType, hitInfo.point + hitInfo.normal * 0.01f, Quaternion.FromToRotation(Vector3.up, hitInfo.normal));
		}
	}

	[ClientRpc]
	private void RpcPlaceDecal(bool isBlood, int type, Vector3 pos, Quaternion rot)
	{
		if (isBlood)
		{
			drawer.DrawBlood(pos, rot, type);
			return;
		}
		GameObject gameObject;
		UnityEngine.Object.Destroy(gameObject = UnityEngine.Object.Instantiate(weapons[type].holeEffect), 4f);
		gameObject.transform.position = pos;
		gameObject.transform.rotation = rot;
		gameObject.transform.localScale = Vector3.one;
	}

	[ClientRpc]
	private void RpcConfirmShot(bool hitmarker, int weapon)
	{
		if (base.isLocalPlayer)
		{
			if (hitmarker)
			{
				Hitmarker.Hit();
			}
		}
		else if (animationController != null)
		{
			animationController.DoAnimation("Shoot");
			PlayMuzzleFlashes(false, curWeapon);
			animationController.gunSource.maxDistance = 80f * ((curWeapon < 0) ? 1f : weapons[curWeapon].allEffects.audioSourceRangeScale) * (float)((!(base.transform.position.y > 900f)) ? 1 : 3);
			animationController.gunSource.PlayOneShot(weapons[weapon].allEffects.shootSound);
		}
	}

	[ClientRpc]
	private void RpcReload(int weapon)
	{
		if (!base.isLocalPlayer && !(reloadCooldown > 0f))
		{
			animationController.DoAnimation("Reload");
			Timing.RunCoroutine(_ReloadRpc(weapon), Segment.FixedUpdate);
		}
	}

	private IEnumerator<float> _Reload()
	{
		if (!(inv.items[inv.GetItemIndex()].durability < (float)weapons[curWeapon].maxAmmo) || abox.GetAmmo(weapons[curWeapon].ammoType) <= 0 || zoomed)
		{
			yield break;
		}
		Animator a = inv.availableItems[inv.curItem].firstpersonModel.GetComponent<Animator>();
		int w = curWeapon;
		animationController.gunSource.PlayOneShot(weapons[curWeapon].reloadClip);
		reloadCooldown = weapons[w].reloadingTime;
		a.SetBool("Reloading", true);
		if (inv.items[inv.GetItemIndex()].durability == 0f)
		{
			if (weapons[curWeapon].reloadingNoammoAnims.Length != 0)
			{
				a.Play(weapons[curWeapon].reloadingNoammoAnims[UnityEngine.Random.Range(0, weapons[curWeapon].reloadingNoammoAnims.Length)], 0, 0f);
			}
		}
		else if (weapons[curWeapon].reloadingAnims.Length != 0)
		{
			a.Play(weapons[curWeapon].reloadingAnims[UnityEngine.Random.Range(0, weapons[curWeapon].reloadingAnims.Length)], 0, 0f);
		}
		CallCmdReload(true);
		while (reloadCooldown > 0.4f)
		{
			if (w != curWeapon)
			{
				a.SetBool("Reloading", false);
				animationController.gunSource.Stop();
				reloadCooldown = 0f;
				yield break;
			}
			yield return 0f;
		}
		a.SetBool("Reloading", false);
		CallCmdReload(false);
	}

	private IEnumerator<float> _ReloadRpc(int weapon)
	{
		reloadCooldown = weapons[weapon].reloadingTime;
		AudioSource s = animationController.gunSource;
		s.maxDistance = 15f;
		s.PlayOneShot(weapons[weapon].reloadClip);
		while (reloadCooldown > 0f)
		{
			if (curWeapon != weapon)
			{
				s.Stop();
				reloadCooldown = 0f;
			}
			yield return 0f;
		}
	}

	public bool GetShootPermission(Team target, bool forceFriendlyFire = false)
	{
		if (ccm.curClass < 0 || ccm.curClass == 2 || ccm.klasy[ccm.curClass].team == Team.SCP)
		{
			return false;
		}
		if (friendlyFire && !forceFriendlyFire)
		{
			return true;
		}
		Team team = ccm.klasy[ccm.curClass].team;
		if ((team == Team.MTF || team == Team.RSC) && (target == Team.MTF || target == Team.RSC))
		{
			return false;
		}
		if ((team == Team.CDP || team == Team.CHI) && (target == Team.CDP || target == Team.CHI))
		{
			return false;
		}
		return true;
	}

	public bool GetShootPermission(CharacterClassManager c, bool forceFriendlyFire = false)
	{
		return c.curClass >= 0 && GetShootPermission(c.klasy[c.curClass].team, forceFriendlyFire);
	}

	private void PlayMuzzleFlashes(bool firstperson, int gunId)
	{
		GameObject gameObject = ((!firstperson) ? weapons[gunId].mod_barrels[inv.GetItemInHand().modBarrel].prefab_thirdperson : weapons[gunId].mod_barrels[inv.GetItemInHand().modBarrel].prefab_firstperson);
		if (!(gameObject == null))
		{
			ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>(true);
			foreach (ParticleSystem particleSystem in componentsInChildren)
			{
				particleSystem.Play();
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSyncFlash(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSyncFlash called on client.");
		}
		else
		{
			((WeaponManager)obj).CmdSyncFlash(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdChangeModPreferences(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeModPreferences called on client.");
		}
		else
		{
			((WeaponManager)obj).CmdChangeModPreferences(reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdShoot(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdShoot called on client.");
		}
		else
		{
			((WeaponManager)obj).CmdShoot(reader.ReadGameObject(), reader.ReadString(), reader.ReadVector3(), reader.ReadVector3(), reader.ReadVector3());
		}
	}

	protected static void InvokeCmdCmdReload(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdReload called on client.");
		}
		else
		{
			((WeaponManager)obj).CmdReload(reader.ReadBoolean());
		}
	}

	public void CallCmdSyncFlash(bool b)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSyncFlash called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSyncFlash(b);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSyncFlash);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(b);
		SendCommandInternal(networkWriter, 0, "CmdSyncFlash");
	}

	public void CallCmdChangeModPreferences(string info)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdChangeModPreferences called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdChangeModPreferences(info);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdChangeModPreferences);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(info);
		SendCommandInternal(networkWriter, 0, "CmdChangeModPreferences");
	}

	public void CallCmdShoot(GameObject target, string hitboxType, Vector3 dir, Vector3 sourcePos, Vector3 targetPos)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdShoot called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdShoot(target, hitboxType, dir, sourcePos, targetPos);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdShoot);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(target);
		networkWriter.Write(hitboxType);
		networkWriter.Write(dir);
		networkWriter.Write(sourcePos);
		networkWriter.Write(targetPos);
		SendCommandInternal(networkWriter, 0, "CmdShoot");
	}

	public void CallCmdReload(bool animationOnly)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdReload called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdReload(animationOnly);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdReload);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(animationOnly);
		SendCommandInternal(networkWriter, 0, "CmdReload");
	}

	protected static void InvokeRpcRpcPlaceDecal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaceDecal called on server.");
		}
		else
		{
			((WeaponManager)obj).RpcPlaceDecal(reader.ReadBoolean(), (int)reader.ReadPackedUInt32(), reader.ReadVector3(), reader.ReadQuaternion());
		}
	}

	protected static void InvokeRpcRpcConfirmShot(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcConfirmShot called on server.");
		}
		else
		{
			((WeaponManager)obj).RpcConfirmShot(reader.ReadBoolean(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcReload(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcReload called on server.");
		}
		else
		{
			((WeaponManager)obj).RpcReload((int)reader.ReadPackedUInt32());
		}
	}

	public void CallRpcPlaceDecal(bool isBlood, int type, Vector3 pos, Quaternion rot)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPlaceDecal called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPlaceDecal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(isBlood);
		networkWriter.WritePackedUInt32((uint)type);
		networkWriter.Write(pos);
		networkWriter.Write(rot);
		SendRPCInternal(networkWriter, 0, "RpcPlaceDecal");
	}

	public void CallRpcConfirmShot(bool hitmarker, int weapon)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcConfirmShot called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcConfirmShot);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(hitmarker);
		networkWriter.WritePackedUInt32((uint)weapon);
		SendRPCInternal(networkWriter, 0, "RpcConfirmShot");
	}

	public void CallRpcReload(int weapon)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcReload called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcReload);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)weapon);
		SendRPCInternal(networkWriter, 0, "RpcReload");
	}

	static WeaponManager()
	{
		kCmdCmdSyncFlash = 257185540;
		NetworkBehaviour.RegisterCommandDelegate(typeof(WeaponManager), kCmdCmdSyncFlash, InvokeCmdCmdSyncFlash);
		kCmdCmdChangeModPreferences = -1505242601;
		NetworkBehaviour.RegisterCommandDelegate(typeof(WeaponManager), kCmdCmdChangeModPreferences, InvokeCmdCmdChangeModPreferences);
		kCmdCmdShoot = -1101833074;
		NetworkBehaviour.RegisterCommandDelegate(typeof(WeaponManager), kCmdCmdShoot, InvokeCmdCmdShoot);
		kCmdCmdReload = 171423498;
		NetworkBehaviour.RegisterCommandDelegate(typeof(WeaponManager), kCmdCmdReload, InvokeCmdCmdReload);
		kRpcRpcPlaceDecal = -472825811;
		NetworkBehaviour.RegisterRpcDelegate(typeof(WeaponManager), kRpcRpcPlaceDecal, InvokeRpcRpcPlaceDecal);
		kRpcRpcConfirmShot = -986408589;
		NetworkBehaviour.RegisterRpcDelegate(typeof(WeaponManager), kRpcRpcConfirmShot, InvokeRpcRpcConfirmShot);
		kRpcRpcReload = 953492064;
		NetworkBehaviour.RegisterRpcDelegate(typeof(WeaponManager), kRpcRpcReload, InvokeRpcRpcReload);
		NetworkCRC.RegisterBehaviour("WeaponManager", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(friendlyFire);
			writer.WritePackedUInt32((uint)curWeapon);
			writer.Write(syncFlash);
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
			writer.Write(friendlyFire);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)curWeapon);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(syncFlash);
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
			friendlyFire = reader.ReadBoolean();
			curWeapon = (int)reader.ReadPackedUInt32();
			syncFlash = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			friendlyFire = reader.ReadBoolean();
		}
		if ((num & 2) != 0)
		{
			HookCurWeapon((int)reader.ReadPackedUInt32());
		}
		if ((num & 4) != 0)
		{
			syncFlash = reader.ReadBoolean();
		}
	}
}
