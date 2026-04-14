using System;
using System.Collections.Generic;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class Interface079 : MonoBehaviour
{
	[Serializable]
	public class Overcon
	{
		public GameObject prefab;

		public string id;

		public Vector3 scaleOrPosition;

		public Vector3 rotationalOffset;

		public Vector3 savedScale;

		public Color colorEmission;

		public Color colorAlbedo;

		public string customInfo;

		private bool aboutToDelete;

		public void Refresh(Color a, Color e)
		{
			aboutToDelete = false;
			if (e != colorEmission || a != colorAlbedo)
			{
				colorEmission = e;
				colorAlbedo = a;
				MeshRenderer component = prefab.GetComponent<MeshRenderer>();
				component.sharedMaterial.color = a;
				component.sharedMaterial.SetColor("_EmissionColor", e);
			}
		}

		public bool Delete()
		{
			if (aboutToDelete)
			{
				UnityEngine.Object.Destroy(prefab);
				return true;
			}
			aboutToDelete = true;
			return false;
		}
	}

	public Camera cameraNormal;

	public float targetFov = 60f;

	public List<Overcon> overconTemplates = new List<Overcon>();

	public List<Overcon> overconInstances = new List<Overcon>();

	public CameraFilterPack_Noise_TV_2 noiseTv2;

	public CameraFilterPack_TV_ARCADE noiseArcade;

	public Scp079Interactable[] allInteractables;

	public GameObject[] quicktpKeys;

	private KeyCode[] movementKeys = new KeyCode[4];

	private KeyCode changeModeKey;

	private KeyCode movementModeKey;

	private string lockedDoorInfo;

	public GameObject overlayCamFeed;

	public GameObject overlaySurvMap;

	public GameObject overlayStartup;

	public GameObject overlaySpeaker;

	public bool mapEnabled;

	public Image moveLeft;

	public Image moveRight;

	public Image moveTop;

	public Image moveBottom;

	public RawImage moveCrosshair;

	public float transitionInProgress;

	public static Interface079 singleton;

	public static Scp079PlayerScript lply;

	public Slider s_mana;

	public Slider s_exp;

	public TextMeshProUGUI t_mana;

	public TextMeshProUGUI t_exp;

	public TextMeshProUGUI t_level;

	public TextMeshProUGUI t_time;

	public TextMeshProUGUI t_room;

	public TextMeshProUGUI t_switchmode;

	public TextMeshProUGUI t_startup;

	public TextMeshProUGUI t_lockeddoors;

	public TextMeshProUGUI t_targets;

	public Camera079 defaultCamera;

	public LayerMask roomDetectionMask;

	public LayerMask overconMask;

	public RectTransform minimapTransform;

	public RectTransform minimapOperationalTransform;

	public RectTransform minimapHcz;

	public RectTransform minimapLcz;

	public RectTransform minimapEz;

	public bool startupAnimation = true;

	private bool animStarted;

	private float bigNotificationTime;

	private float smallNotificationTime;

	public AnimationCurve notificationVisibilityCurve;

	public TextMeshProUGUI bigNotificationText;

	public TextMeshProUGUI smallNotificationText;

	public GameObject warning;

	public bool mouseLookMode;

	private GraphicRaycaster m_Raycaster;

	private PointerEventData m_PointerEventData;

	private EventSystem m_EventSystem;

	[HideInInspector]
	public Quaternion prevCamRotation;

	private static bool debugMode;

	private string counterTemplate;

	private int warns;

	private float remtim;

	public AnimationCurve overconScaleOverDistance = AnimationCurve.Constant(0f, 1f, 1f);

	private List<string> bigNotificationQueue = new List<string>();

	private List<string> smallNotificationQueue = new List<string>();

	public static void Log(string s)
	{
		if (debugMode)
		{
			Debug.Log("SCP079: " + s);
			ServerConsole.AddLog("SCP079: " + s);
		}
	}

	private void UpdateWarning()
	{
		int num = 0;
		foreach (Generator079 generator in Generator079.generators)
		{
			if (generator.isTabletConnected)
			{
				num++;
			}
		}
		if (warns != num)
		{
			if (num > warns)
			{
				GetComponent<AudioSource>().Play();
				remtim = 6f;
			}
			warns = num;
		}
		if (remtim > 0f)
		{
			remtim -= Time.deltaTime;
		}
		warning.SetActive(remtim > 0f && num > 0);
	}

	private void LateUpdate()
	{
		if (startupAnimation)
		{
			if (!animStarted)
			{
				Timing.RunCoroutine(_StartupAnimation());
				animStarted = true;
			}
			return;
		}
		CheckForInput();
		ClearOvercons();
		UpdateOvercons();
		CalculateNearestCameras();
		UpdateTransition();
		UpdateMapMovement();
		UpdateBigNotifies();
		UpdateSmallNotifies();
		UpdateLockedDoors();
		UpdateWarning();
		UpdateCounter();
		UpdateSpeaker();
		DrawOvercons();
	}

	private void UpdateSpeaker()
	{
		if (!string.IsNullOrEmpty(lply.Speaker))
		{
			overlaySpeaker.SetActive(true);
			overlaySurvMap.SetActive(false);
			overlayCamFeed.SetActive(false);
			mouseLookMode = false;
		}
		else if (overlaySpeaker.activeSelf)
		{
			overlaySpeaker.SetActive(false);
			overlayCamFeed.SetActive(true);
		}
	}

	private void UpdateCounter()
	{
		if (!mapEnabled)
		{
			return;
		}
		int[] array = new int[5];
		string[] array2 = new string[5];
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
			switch (component.klasy[component.curClass].team)
			{
			case Team.SCP:
				array[0]++;
				break;
			case Team.MTF:
				array[1]++;
				break;
			case Team.RSC:
				array[2]++;
				break;
			case Team.CDP:
				array[3]++;
				break;
			case Team.CHI:
				array[4]++;
				break;
			}
		}
		for (int j = 0; j < 5; j++)
		{
			array2[j] = ": " + array[j] + "\n";
		}
		t_targets.text = string.Format(counterTemplate, array2);
	}

	private void UpdateLockedDoors()
	{
		t_lockeddoors.text = ((lply.lockedDoors.Count <= 0) ? string.Empty : lockedDoorInfo.Replace("[LOCKED_NUM]", lply.lockedDoors.Count + "\n"));
	}

	private void DrawOvercons()
	{
		Camera079 currentCamera = lply.currentCamera;
		foreach (Scp079Interactable nearbyInteractable in lply.nearbyInteractables)
		{
			if (!(nearbyInteractable != null))
			{
				continue;
			}
			if (nearbyInteractable.type == Scp079Interactable.InteractableType.Camera && nearbyInteractable.GetComponent<Camera079>() != currentCamera)
			{
				bool flag = nearbyInteractable.GetComponent<Camera079>() == lply.nearestCameras[0];
				bool flag2 = lply.CalculateCameraSwitchCost(nearbyInteractable.transform.position) > lply.Mana;
				AddOvercon(0, "CAMERA" + nearbyInteractable.GetComponent<Camera079>().cameraName, nearbyInteractable.transform.position, "CCTV:" + nearbyInteractable.transform.position.x + ":" + nearbyInteractable.transform.position.y + ":" + nearbyInteractable.transform.position.z, flag2 ? new Color(1f, 0f, 0f, 0.05f) : ((!flag) ? new Color(0f, 0f, 0f, 0.02f) : new Color(0f, 0f, 0f, 0.1f)), (!flag2) ? Color.white : Color.red);
			}
			else if (nearbyInteractable.type == Scp079Interactable.InteractableType.Speaker)
			{
				bool flag3 = lply.GetManaFromLabel("Speaker Start", lply.abilities) * 1.5f <= lply.Mana;
				AddOvercon(12, "SPEAKER" + nearbyInteractable.transform.position, nearbyInteractable.transform.position, "SPEAKER:" + lply.currentRoom, (!flag3) ? new Color(1f, 0f, 0f, 0.05f) : new Color(0f, 0f, 0f, 0.1f), (!flag3) ? Color.red : Color.white);
			}
			else if (nearbyInteractable.type == Scp079Interactable.InteractableType.Door)
			{
				if (AlphaWarheadController.host.inProgress)
				{
					continue;
				}
				Door component = nearbyInteractable.GetComponent<Door>();
				if (!(component != null) || component.destroyed)
				{
					continue;
				}
				bool flag4 = lply.GetManaFromLabel("Door Interaction " + ((!(component.permissionLevel == string.Empty)) ? component.permissionLevel : "DEFAULT"), lply.abilities) <= lply.Mana;
				bool flag5 = 15f <= lply.Mana;
				if (component.sound_checkpointWarning != null)
				{
					AddOvercon(component.isOpen ? 1 : 2, string.Concat("DOOR", component.netId, component.isOpen.ToString()), nearbyInteractable.transform.position + Vector3.up * 1.8f, "DOOR:" + nearbyInteractable.transform.parent.name + "/" + nearbyInteractable.name, (!flag4) ? new Color(1f, 0f, 0f, 0.05f) : new Color(0f, 0f, 0f, 0.1f), (!flag4) ? Color.red : Color.white);
				}
				else if (!component.destroyed)
				{
					AddOvercon(component.isOpen ? 1 : 2, string.Concat("DOOR", component.netId, component.isOpen.ToString()), nearbyInteractable.transform.position + Vector3.up * 2.2f, "DOOR:" + nearbyInteractable.transform.parent.name + "/" + nearbyInteractable.name, (!flag4) ? new Color(1f, 0f, 0f, 0.05f) : new Color(0f, 0f, 0f, 0.1f), (!flag4) ? Color.red : Color.white);
					if (!component.locked)
					{
						AddOvercon(3, "L_DOOR" + component.netId, nearbyInteractable.transform.position + Vector3.up * 1.2f, "DOORLOCK:" + nearbyInteractable.transform.parent.name + "/" + nearbyInteractable.name, (!flag5) ? new Color(1f, 0f, 0f, 0.05f) : new Color(0f, 0f, 0f, 0.1f), (!flag5) ? Color.red : Color.white);
					}
				}
			}
			else if (nearbyInteractable.type == Scp079Interactable.InteractableType.Tesla)
			{
				bool flag6 = lply.GetManaFromLabel("Tesla Gate Burst", lply.abilities) <= lply.Mana;
				AddOvercon(11, "TESLA" + nearbyInteractable.transform.position, nearbyInteractable.transform.position + Vector3.up * 2f, "TESLA:" + lply.currentRoom, (!flag6) ? new Color(1f, 0f, 0f, 0.05f) : new Color(0f, 0f, 0f, 0.1f), (!flag6) ? Color.red : Color.white);
			}
			else if (nearbyInteractable.type == Scp079Interactable.InteractableType.ElevatorTeleport)
			{
				bool flag7 = lply.GetManaFromLabel("Elevator Teleport", lply.abilities) <= lply.Mana;
				Vector3 position = nearbyInteractable.optionalObject.transform.position;
				AddOvercon(int.Parse(nearbyInteractable.optionalParameter), "ELEVATORTELEPORT" + nearbyInteractable.transform.position, nearbyInteractable.transform.position, "ELEVATORTELEPORT:" + position.x + ":" + position.y + ":" + position.z, (!flag7) ? new Color(1f, 0f, 0f, 0.05f) : new Color(0f, 0f, 0f, 0.1f), (!flag7) ? Color.red : Color.white);
			}
			else if (nearbyInteractable.type == Scp079Interactable.InteractableType.ElevatorUse)
			{
				bool flag8 = lply.GetManaFromLabel("Elevator Use", lply.abilities) <= lply.Mana;
				string elevatorName = nearbyInteractable.optionalObject.GetComponent<Lift>().elevatorName;
				AddOvercon(13, "ELEVATORTUSE" + nearbyInteractable.transform.position, nearbyInteractable.GetComponentInChildren<Animator>().transform.position + Vector3.up * 1.8f, "ELEVATORUSE:" + elevatorName, (!flag8) ? new Color(1f, 0f, 0f, 0.05f) : new Color(0f, 0f, 0f, 0.1f), (!flag8) ? Color.red : Color.white);
			}
			else
			{
				if (nearbyInteractable.type != Scp079Interactable.InteractableType.Lockdown)
				{
					continue;
				}
				if (AlphaWarheadController.host.inProgress)
				{
					break;
				}
				bool flag9 = true;
				foreach (Scp079Interactable nearbyInteractable2 in lply.nearbyInteractables)
				{
					if (nearbyInteractable2.type == Scp079Interactable.InteractableType.Door)
					{
						Door component2 = nearbyInteractable2.GetComponent<Door>();
						if (component2 == null || component2.destroyed || component2.locked || component2.scp079Lockdown > -2.5f || lply.Lvl == 0)
						{
							flag9 = false;
							break;
						}
					}
				}
				if (flag9)
				{
					bool flag10 = lply.GetManaFromLabel("Room Lockdown", lply.abilities) <= lply.Mana;
					AddOvercon(10, "LOCKDOWN" + lply.currentRoom + lply.currentZone, nearbyInteractable.transform.position, "LOCKDOWN:", (!flag10) ? new Color(1f, 0f, 0f, 0.05f) : new Color(0f, 0f, 0f, 0.1f), (!flag10) ? Color.red : Color.white);
				}
			}
		}
	}

	private void UpdateMapMovement()
	{
		CursorManager.singleton.is079 = !mapEnabled && !mouseLookMode;
		if (!mapEnabled || !(lply.currentZone != "Outside"))
		{
			return;
		}
		float x = minimapOperationalTransform.GetComponentInParent<Image>().rectTransform.localScale.x;
		x += Input.GetAxis("Mouse ScrollWheel");
		x = Mathf.Clamp(x, 1f, 4.5f);
		minimapOperationalTransform.GetComponentInParent<Image>().rectTransform.localScale = Vector3.one * x;
		minimapOperationalTransform.localPosition -= new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) / (x / 2f) * 7f;
		m_PointerEventData = new PointerEventData(m_EventSystem)
		{
			position = Input.mousePosition
		};
		List<RaycastResult> list = new List<RaycastResult>();
		m_Raycaster.Raycast(m_PointerEventData, list);
		string text = "NONE";
		foreach (RaycastResult item in list)
		{
			if (item.gameObject.name.Contains("Root"))
			{
				text = item.gameObject.name;
			}
		}
		RawImage[] componentsInChildren = minimapTransform.GetComponentsInChildren<RawImage>(true);
		foreach (RawImage rawImage in componentsInChildren)
		{
			bool flag = rawImage.name == "MINIMAP:" + lply.currentRoom;
			bool flag2 = rawImage.name == text;
			int num = 1;
			foreach (Generator079 generator in Generator079.generators)
			{
				if (generator.isTabletConnected && generator.curRoom == rawImage.name.Split(':')[1])
				{
					num = 0;
				}
			}
			rawImage.color = new Color(1f, num, num, (flag ? 0.054f : ((!flag2) ? 0.017f : 0.035f)) * ((num != 1) ? 2.5f : 1f));
			if (flag)
			{
				minimapTransform.localPosition = -rawImage.GetComponent<RectTransform>().localPosition / 10f;
			}
		}
		if (!Input.GetKeyDown(KeyCode.Mouse0) || !text.Contains(":"))
		{
			return;
		}
		Camera079[] componentsInChildren2 = GameObject.Find(lply.currentZone + "/" + text.Split(':')[1]).GetComponentsInChildren<Camera079>();
		foreach (Camera079 camera in componentsInChildren2)
		{
			if (camera.isMain)
			{
				SwitchToCamera(camera.transform.position, false);
			}
		}
	}

	private IEnumerator<float> _StartupAnimation()
	{
		string allChars = "1234567890!@#$%^&*()-=_+qwertyuiop[]asdfghjkl;'zxcvbnm,./QWERTYUIOP{}|ASDFGHJKL:ZXCVBNM";
		t_startup.text = string.Empty;
		for (int f = 0; f < 100; f++)
		{
			noiseArcade.Fade = (float)f / 100f;
			yield return 0f;
		}
		string ttd = "Hardware Safemode Active";
		for (int i = 0; i < ttd.Length; i++)
		{
			t_startup.text += ttd[i];
			yield return 0f;
		}
		for (int j = 0; j < 25; j++)
		{
			yield return 0f;
		}
		t_startup.text += "\n";
		ttd = "Deactivating Hibernation Mode...";
		for (int k = 0; k < ttd.Length; k++)
		{
			t_startup.text += ttd[k];
			yield return 0f;
		}
		t_startup.text += "\n \n";
		for (int l = 0; l < 50; l++)
		{
			yield return 0f;
		}
		ttd = "Rebooting Operating System...";
		for (int m = 0; m < ttd.Length; m++)
		{
			t_startup.text += ttd[m];
			yield return 0f;
		}
		for (int n = 0; n < 25; n++)
		{
			yield return 0f;
		}
		t_startup.text += "\n";
		ttd = "Complete. System ready.";
		for (int num = 0; num < ttd.Length; num++)
		{
			t_startup.text += ttd[num];
			yield return 0f;
		}
		for (int num2 = 0; num2 < 25; num2++)
		{
			yield return 0f;
		}
		t_startup.text += "\n \n";
		noiseTv2.enabled = true;
		noiseTv2.Fade = (noiseTv2.Fade_Additive = (noiseTv2.Fade_Distortion = 0f));
		for (int num3 = 0; num3 < 80; num3++)
		{
			for (int num4 = 0; num4 < 25; num4++)
			{
				t_startup.text += allChars[UnityEngine.Random.Range(0, allChars.Length)];
			}
			yield return 0f;
			if (num3 == 65)
			{
				PlayerManager.localPlayer.GetComponent<HorrorSoundController>().horrorSoundSource.PlayOneShot(PlayerManager.localPlayer.GetComponent<CharacterClassManager>().bell);
			}
			if (num3 > 50)
			{
				noiseTv2.Fade = (noiseTv2.Fade_Additive = (noiseTv2.Fade_Distortion = ((float)num3 - 50f) / 30f));
			}
		}
		lply.RefreshCurrentRoom();
		startupAnimation = false;
		animStarted = false;
		overlayCamFeed.SetActive(!mapEnabled);
		overlaySurvMap.SetActive(mapEnabled);
		overlayStartup.SetActive(false);
	}

	public void UseButton(int actionID)
	{
		if (actionID < 4 && !mapEnabled)
		{
			if (lply.nearestCameras[actionID] != null)
			{
				transitionInProgress = 10f;
				SwitchToCamera(lply.nearestCameras[actionID].transform.position, true);
			}
			return;
		}
		switch (actionID)
		{
		case 4:
			SwitchMode();
			break;
		case 5:
			lply.CallCmdInteract("STOPSPEAKER:", null);
			break;
		}
	}

	private void SwitchToCamera(Vector3 pos, bool lookatRotation)
	{
		prevCamRotation = lply.currentCamera.head.transform.rotation;
		lply.CallCmdSwitchCamera(pos, lookatRotation);
	}

	public void SwitchMode()
	{
		if ((lply.currentZone == "Outside" && !mapEnabled) || overlaySpeaker.activeSelf)
		{
			return;
		}
		mapEnabled = !mapEnabled;
		noiseArcade.Fade = ((!mapEnabled) ? 1f : 0.76f);
		overlayCamFeed.SetActive(!mapEnabled);
		overlaySurvMap.SetActive(mapEnabled);
		if (!mapEnabled)
		{
			return;
		}
		lply.RefreshCurrentRoom();
		minimapHcz.gameObject.SetActive(false);
		minimapLcz.gameObject.SetActive(false);
		minimapEz.gameObject.SetActive(false);
		switch (lply.currentZone)
		{
		case "HeavyRooms":
			minimapTransform = minimapHcz;
			break;
		case "LightRooms":
			minimapTransform = minimapLcz;
			break;
		case "EntranceRooms":
			minimapTransform = minimapEz;
			break;
		}
		if (lply.currentZone == "Outside")
		{
			return;
		}
		minimapTransform.gameObject.SetActive(true);
		minimapTransform.localScale = Vector3.one / 10f;
		RawImage[] componentsInChildren = minimapTransform.GetComponentsInChildren<RawImage>(true);
		foreach (RawImage rawImage in componentsInChildren)
		{
			bool flag = rawImage.name == "MINIMAP:" + lply.currentRoom;
			rawImage.color = new Color(1f, 1f, 1f, (!flag) ? 0.02f : 0.05f);
			if (flag)
			{
				minimapTransform.localPosition = -rawImage.GetComponent<RectTransform>().localPosition / 10f;
				minimapOperationalTransform.localPosition = Vector3.zero;
			}
		}
	}

	private void UpdateTransition()
	{
		Camera079 currentCamera = lply.currentCamera;
		s_exp.maxValue = ((lply.Lvl != lply.levels.Length - 1) ? lply.levels[lply.Lvl + 1].unlockExp : 0);
		s_exp.value = lply.Exp;
		s_mana.maxValue = lply.maxMana;
		s_mana.value = lply.Mana;
		t_mana.text = Mathf.Floor(s_mana.value) + "/" + s_mana.maxValue;
		t_exp.text = Mathf.Floor(s_exp.value) + "/" + s_exp.maxValue;
		t_level.text = lply.levels[lply.Lvl].label;
		t_time.text = "SP " + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + ":" + DateTime.Now.Second.ToString("00");
		t_room.text = ((!(currentCamera == null)) ? currentCamera.cameraName : "NO ROOM");
		if (currentCamera != null)
		{
			base.transform.position = currentCamera.targetPosition.position;
			base.transform.rotation = currentCamera.targetPosition.rotation;
		}
		if (currentCamera != null)
		{
			currentCamera.curRot = Mathf.Clamp(currentCamera.curRot, currentCamera.minRot, currentCamera.maxRot);
			currentCamera.curPitch = Mathf.Clamp(currentCamera.curPitch, currentCamera.minPitch, currentCamera.maxPitch);
			moveLeft.GetComponent<CanvasRenderer>().SetAlpha(mouseLookMode ? 0f : ((currentCamera.curRot != currentCamera.minRot) ? 1f : 0.3f));
			moveRight.GetComponent<CanvasRenderer>().SetAlpha(mouseLookMode ? 0f : ((currentCamera.curRot != currentCamera.maxRot) ? 1f : 0.3f));
			moveBottom.GetComponent<CanvasRenderer>().SetAlpha(mouseLookMode ? 0f : ((currentCamera.curPitch != currentCamera.maxPitch) ? 1f : 0.3f));
			moveTop.GetComponent<CanvasRenderer>().SetAlpha(mouseLookMode ? 0f : ((currentCamera.curPitch != currentCamera.minPitch) ? 1f : 0.3f));
			moveCrosshair.enabled = mouseLookMode;
		}
		transitionInProgress -= Time.deltaTime;
		noiseTv2.Fade = (noiseTv2.Fade_Additive = (noiseTv2.Fade_Distortion = Mathf.Lerp(noiseTv2.Fade_Distortion, (transitionInProgress > -0.2f) ? 1 : 0, Time.deltaTime * 12f)));
		noiseTv2.enabled = noiseTv2.Fade > 0f;
	}

	private void CheckForInput()
	{
		Camera079 currentCamera = lply.currentCamera;
		if (currentCamera == null || overlaySpeaker.activeSelf)
		{
			return;
		}
		if (!mapEnabled)
		{
			if (transitionInProgress < -0.4f)
			{
				for (int i = 0; i < movementKeys.Length; i++)
				{
					if (lply.nearestCameras[i] != null && Input.GetKeyDown(movementKeys[i]) && !CursorManager.singleton.raOp)
					{
						transitionInProgress = 10f;
						SwitchToCamera(lply.nearestCameras[i].transform.position, i == 0);
					}
				}
			}
			if (mouseLookMode)
			{
				float sens = Sensitivity.sens;
				currentCamera.curRot += Input.GetAxis("Mouse X") * sens;
				currentCamera.curPitch -= Input.GetAxis("Mouse Y") * sens;
			}
			else
			{
				float value = Input.mousePosition.x / (float)Screen.width - 0.5f;
				value = Mathf.Clamp(value, -0.35f, 0.35f);
				if (Mathf.Abs(value) == 0.35f)
				{
					currentCamera.curRot += value * 380f * Time.deltaTime;
				}
				value = Input.mousePosition.y / (float)Screen.height - 0.5f;
				value = Mathf.Clamp(value, -0.35f, 0.35f);
				if (Mathf.Abs(value) == 0.35f)
				{
					currentCamera.curPitch -= value * 300f * Time.deltaTime;
				}
			}
			currentCamera.UpdatePosition(currentCamera.curRot, currentCamera.curPitch);
			if (!CursorManager.singleton.raOp)
			{
				RaycastHit hitInfo;
				if (Input.GetKeyDown(KeyCode.Mouse0) && Physics.Raycast(GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition), out hitInfo, 100f, overconMask))
				{
					string[] array = hitInfo.transform.name.Split(':');
					switch (array[0])
					{
					case "CCTV":
					{
						Vector3 zero = Vector3.zero;
						for (int j = 0; j < 3; j++)
						{
							zero[j] = float.Parse(array[j + 1]);
						}
						SwitchToCamera(zero, true);
						break;
					}
					case "DOOR":
						lply.CallCmdInteract(hitInfo.transform.name, GameObject.Find(hitInfo.collider.name.Remove(0, 5)).GetComponentInParent<Door>().gameObject);
						break;
					case "DOORLOCK":
						lply.CallCmdInteract(hitInfo.transform.name, GameObject.Find(hitInfo.collider.name.Remove(0, 9)).GetComponentInParent<Door>().gameObject);
						break;
					default:
						if (array[0] == "ELEVATORTELEPORT" || array[0] == "ELEVATORUSE" || array[0] == "TESLA" || array[0].StartsWith("SPEAKER"))
						{
							lply.CallCmdInteract(hitInfo.transform.name, null);
						}
						else if (array[0] == "LOCKDOWN")
						{
							Log("Lockdown overcon selected");
							lply.CallCmdInteract("LOCKDOWN:", null);
						}
						break;
					}
				}
				if (Input.GetKeyDown(KeyCode.Mouse1) && lply.lockedDoors.Count > 0 && overlayCamFeed.activeSelf)
				{
					lply.CallCmdResetDoors();
				}
				if (Input.GetKeyDown(movementModeKey))
				{
					mouseLookMode = !mouseLookMode;
				}
			}
		}
		if (Input.GetKeyDown(changeModeKey) && !CursorManager.singleton.raOp)
		{
			SwitchMode();
		}
	}

	public void AddOvercon(int id, string name, Vector3 position, string info, Color colorA, Color colorE)
	{
		if (mapEnabled)
		{
			return;
		}
		foreach (Overcon overconInstance in overconInstances)
		{
			if (overconInstance.id == name)
			{
				overconInstance.Refresh(colorA, colorE);
				return;
			}
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(overconTemplates[id].prefab, cameraNormal.transform);
		gameObject.transform.localScale = overconTemplates[id].scaleOrPosition;
		MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
		component.sharedMaterial = new Material(component.sharedMaterial);
		component.sharedMaterial.SetColor("_EmissionColor", colorE);
		component.sharedMaterial.color = colorA;
		overconInstances.Add(new Overcon
		{
			prefab = gameObject,
			id = name,
			scaleOrPosition = position,
			rotationalOffset = overconTemplates[id].rotationalOffset,
			savedScale = overconTemplates[id].scaleOrPosition,
			customInfo = info
		});
	}

	private void UpdateOvercons()
	{
		foreach (Overcon overconInstance in overconInstances)
		{
			overconInstance.prefab.transform.position = overconInstance.scaleOrPosition;
			overconInstance.prefab.transform.LookAt(base.transform.position);
			overconInstance.prefab.transform.Rotate(overconInstance.rotationalOffset);
			Vector3 localPosition = overconInstance.prefab.transform.localPosition;
			overconInstance.prefab.transform.localScale = overconScaleOverDistance.Evaluate(localPosition.magnitude) * overconInstance.savedScale;
			overconInstance.prefab.transform.localPosition = localPosition.normalized;
			overconInstance.prefab.name = overconInstance.customInfo;
		}
	}

	private void ClearOvercons()
	{
		bool flag = false;
		while (!flag)
		{
			flag = true;
			for (int i = 0; i < overconInstances.Count; i++)
			{
				if (overconInstances[i].Delete())
				{
					flag = false;
					overconInstances.RemoveAt(i);
					return;
				}
			}
		}
	}

	private void Awake()
	{
		singleton = this;
		base.gameObject.SetActive(false);
		m_Raycaster = GetComponentInChildren<GraphicRaycaster>();
		m_EventSystem = GetComponentInChildren<EventSystem>();
		debugMode = PlayerPrefs.HasKey("DEBUGMODE");
		Debug.Log("Debug mode is " + ((!debugMode) ? "disabled." : "enabled."));
	}

	private void Start()
	{
		cameraNormal.gameObject.SetActive(true);
		for (int i = 0; i < 4; i++)
		{
			TextMeshProUGUI obj = quicktpKeys[i].GetComponentsInChildren<TextMeshProUGUI>(true)[1];
			KeyCode keyCode = (movementKeys[i] = NewInput.GetKey((new string[4] { "Move Forward", "Move Left", "Move Right", "Move Backward" })[i]));
			obj.text = keyCode.ToString();
		}
		TextMeshProUGUI textMeshProUGUI = t_switchmode;
		string text = TranslationReader.Get("SCP079", 3);
		KeyCode keyCode2 = (changeModeKey = NewInput.GetKey("Inventory"));
		textMeshProUGUI.text = text.Replace("[KEY]", keyCode2.ToString()).ToUpper();
		lockedDoorInfo = TranslationReader.Get("SCP079", 11);
		counterTemplate = TranslationReader.Get("SCP079", 16);
		movementModeKey = NewInput.GetKey("Jump");
	}

	public void RefreshInteractables()
	{
		allInteractables = UnityEngine.Object.FindObjectsOfType<Scp079Interactable>();
	}

	private void CalculateNearestCameras()
	{
		Camera079 currentCamera = lply.currentCamera;
		Vector3[] array = new Vector3[4]
		{
			base.transform.forward,
			-base.transform.right,
			base.transform.right,
			-base.transform.forward
		};
		lply.nearestCameras = new Camera079[4];
		for (int i = 0; i < 4; i++)
		{
			List<Camera079> list = new List<Camera079>();
			foreach (Scp079Interactable nearbyInteractable in lply.nearbyInteractables)
			{
				if (nearbyInteractable.type == Scp079Interactable.InteractableType.Camera && nearbyInteractable.GetComponent<Camera079>() != currentCamera)
				{
					float num = Vector3.Angle(nearbyInteractable.transform.position - base.transform.position, array[i]);
					if (num < 45f)
					{
						list.Add(nearbyInteractable.GetComponent<Camera079>());
					}
				}
			}
			float num2 = float.PositiveInfinity;
			Camera079 camera = null;
			foreach (Camera079 item in list)
			{
				float num3 = Vector3.Distance(item.transform.position, base.transform.position);
				float num4 = Vector3.Angle(item.transform.position - base.transform.position, array[i]);
				if (num3 + num4 < num2)
				{
					camera = item;
					num2 = num3 + num4;
				}
			}
			lply.nearestCameras[i] = camera;
		}
		for (int j = 0; j < 4; j++)
		{
			Graphic[] componentsInChildren = quicktpKeys[j].GetComponentsInChildren<Graphic>();
			foreach (Graphic graphic in componentsInChildren)
			{
				graphic.color = new Color(1f, 1f, 1f, (!(lply.nearestCameras[j] == null)) ? 0.15f : 0.03f);
			}
			quicktpKeys[j].GetComponentsInChildren<TextMeshProUGUI>(true)[0].text = ((!(lply.nearestCameras[j] == null)) ? lply.nearestCameras[j].cameraName : "NO CAMERA");
		}
	}

	public void NotifyNewLevel(int newLvl)
	{
		Log("Level has been changed! (" + newLvl + ")");
		bigNotificationQueue.Add("NEW ACCESS TIER\nUNLOCKED");
	}

	public void NotifyMoreExp(string info)
	{
		smallNotificationQueue.Add(info);
	}

	public void NotifyNotEnoughMana(int required)
	{
		Log("Not enough mana! (" + lply.Mana + "/" + required + ")");
	}

	public void AddBigNotification(string text)
	{
		bigNotificationQueue.Add(text);
	}

	private void ShowBigNotification(string text)
	{
		bigNotificationText.text = text;
		bigNotificationTime = 3f;
	}

	private void ShowSmallNotification(string text)
	{
		smallNotificationText.text = text;
		smallNotificationTime = 3f;
	}

	private void UpdateBigNotifies()
	{
		if (bigNotificationTime < 0f)
		{
			if (bigNotificationQueue.Count > 0)
			{
				ShowBigNotification(bigNotificationQueue[0]);
				bigNotificationQueue.RemoveAt(0);
			}
		}
		else
		{
			bigNotificationTime -= Time.deltaTime;
			float num = Mathf.Clamp01(notificationVisibilityCurve.Evaluate(Mathf.Clamp01(1f - bigNotificationTime / 3f)));
			CameraFilterPack_FX_Glitch3 componentInChildren = GetComponentInChildren<CameraFilterPack_FX_Glitch3>();
			bigNotificationText.color = new Color(1f, 0f, 0f, num);
			componentInChildren._Noise = num;
			componentInChildren._Glitch = num / 2f;
		}
	}

	private void UpdateSmallNotifies()
	{
		if (smallNotificationTime < 0f)
		{
			if (smallNotificationQueue.Count > 0)
			{
				ShowSmallNotification(smallNotificationQueue[0]);
				smallNotificationQueue.RemoveAt(0);
			}
		}
		else
		{
			smallNotificationTime -= Time.deltaTime * (float)Mathf.Clamp(smallNotificationQueue.Count / 2, 1, 4);
			float t = Mathf.Clamp01(notificationVisibilityCurve.Evaluate(Mathf.Clamp01(1f - smallNotificationTime / 3f)));
			smallNotificationText.color = Color.Lerp(Color.clear, new Color(1f, 1f, 1f, 0.15f), t);
		}
	}
}
