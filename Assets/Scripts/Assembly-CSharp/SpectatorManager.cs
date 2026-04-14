using System.Collections.Generic;
using MEC;
using RemoteAdmin;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class SpectatorManager : NetworkBehaviour
{
	private SpectatorManager curPlayer;

	private PlayerManager pmng;

	private SpectatorInterface inf;

	private CharacterClassManager ccm;

	private SpectatorCamera cam;

	private Transform myCamera;

	private PlayerStats stats;

	private PlyMovementSync pms;

	private AnimationController actrl;

	private ServerRoles rls;

	private QueryProcessor qrpr;

	private static ServerRoles rlsMy;

	private Scp079PlayerScript scp079;

	public Transform cameraPosition;

	public GameObject weaponCams;

	public Camera mainCam;

	private int prevClass;

	private void Start()
	{
		scp079 = GetComponent<Scp079PlayerScript>();
		actrl = GetComponent<AnimationController>();
		cam = Object.FindObjectOfType<SpectatorCamera>();
		ccm = GetComponent<CharacterClassManager>();
		inf = SpectatorInterface.singleton;
		pmng = PlayerManager.singleton;
		stats = GetComponent<PlayerStats>();
		pms = GetComponent<PlyMovementSync>();
		rls = GetComponent<ServerRoles>();
		qrpr = GetComponent<QueryProcessor>();
		if (rlsMy == null)
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				if (gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					rlsMy = gameObject.GetComponent<ServerRoles>();
				}
			}
		}
		myCamera = GetComponent<Scp049PlayerScript>().plyCam.transform;
		if (base.isLocalPlayer)
		{
			Timing.RunCoroutine(_PeriodicRefresher(), Segment.FixedUpdate);
		}
		else
		{
			base.enabled = false;
		}
	}

	private IEnumerator<float> _PeriodicRefresher()
	{
		while (this != null)
		{
			if (inf == null || ccm.curClass != 2)
			{
				yield return 0f;
				continue;
			}
			if (curPlayer == null || curPlayer == this)
			{
				inf.playerInfo.text = string.Empty;
			}
			string t = string.Empty;
			GameObject[] players = pmng.players;
			foreach (GameObject gameObject in players)
			{
				if (gameObject != null)
				{
					CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
					if (component == null)
					{
						break;
					}
					string myNick = gameObject.GetComponent<NicknameSync>().myNick;
					if (component.curClass >= 0 && component.curClass != 2)
					{
						t = ((!(curPlayer != null) || !(curPlayer.gameObject == gameObject)) ? (t + string.Format("{1}{0}</color>\n", myNick, ColorToHex(component.klasy[component.curClass].classColor))) : (t + string.Format("<u>{1}{0}</color></u>\n", myNick, ColorToHex(component.klasy[component.curClass].classColor))));
					}
				}
			}
			inf.playerList.text = t;
			yield return 0f;
		}
	}

	public void RefreshList()
	{
		if (!base.isLocalPlayer || inf == null || cam == null)
		{
			return;
		}
		TextMeshProUGUI playerList = inf.playerList;
		playerList.text = string.Empty;
		if (ccm.curClass == 2)
		{
			if (!cam.freeCam.enabled)
			{
				cam.cam.enabled = true;
			}
			mainCam.enabled = false;
			weaponCams.SetActive(false);
			inf.rootPanel.SetActive(true);
			if (curPlayer == null || curPlayer == this)
			{
				inf.playerInfo.text = string.Empty;
			}
			GameObject[] players = pmng.players;
			foreach (GameObject gameObject in players)
			{
				CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
				string myNick = gameObject.GetComponent<NicknameSync>().myNick;
				if (component.curClass >= 0 && component.curClass != 2)
				{
					if (curPlayer != null && curPlayer.gameObject == gameObject)
					{
						playerList.text += string.Format("<u>{1}{0}</color></u>\n", myNick, ColorToHex(component.klasy[component.curClass].classColor));
					}
					else
					{
						playerList.text += string.Format("{1}{0}</color>\n", myNick, ColorToHex(component.klasy[component.curClass].classColor));
					}
				}
			}
		}
		else
		{
			if (curPlayer != null && !curPlayer.isLocalPlayer && curPlayer.actrl.headAnimator != null)
			{
				curPlayer.actrl.headAnimator.transform.localScale = Vector3.one;
			}
			curPlayer = this;
			mainCam.enabled = true;
			cam.cam.enabled = false;
			cam.freeCam.enabled = false;
			weaponCams.SetActive(true);
			inf.rootPanel.SetActive(false);
		}
	}

	private void NextPlayer()
	{
		cam.cam.enabled = true;
		cam.freeCam.enabled = false;
		List<GameObject> list = new List<GameObject>();
		GameObject[] players = pmng.players;
		foreach (GameObject item in players)
		{
			list.Add(item);
		}
		if (curPlayer == null)
		{
			curPlayer = list[0].GetComponent<SpectatorManager>();
		}
		if (curPlayer != null && !curPlayer.isLocalPlayer && curPlayer.actrl.headAnimator != null)
		{
			curPlayer.actrl.headAnimator.transform.localScale = Vector3.one;
		}
		if (curPlayer != null)
		{
			int num = list.IndexOf(curPlayer.gameObject);
			for (int j = 1; j <= list.Count; j++)
			{
				int num2 = j + num;
				if (num2 >= list.Count)
				{
					num2 -= list.Count;
				}
				int curClass = list[num2].GetComponent<CharacterClassManager>().curClass;
				if (curClass >= 0 && curClass != 2)
				{
					curPlayer = list[num2].GetComponent<SpectatorManager>();
					RefreshList();
					return;
				}
			}
		}
		if (curPlayer != null && !curPlayer.isLocalPlayer && curPlayer.actrl.headAnimator != null)
		{
			curPlayer.actrl.headAnimator.transform.localScale = Vector3.zero;
		}
		cam.cam.enabled = false;
		cam.freeCam.enabled = true;
		RefreshList();
	}

	private void PreviousPlayer()
	{
		cam.cam.enabled = true;
		cam.freeCam.enabled = false;
		List<GameObject> list = new List<GameObject>();
		GameObject[] players = pmng.players;
		foreach (GameObject item in players)
		{
			list.Add(item);
		}
		if (curPlayer == null)
		{
			curPlayer = list[0].GetComponent<SpectatorManager>();
		}
		if (curPlayer != null && !curPlayer.isLocalPlayer && curPlayer.actrl.headAnimator != null)
		{
			curPlayer.actrl.headAnimator.transform.localScale = Vector3.one;
		}
		if (curPlayer != null)
		{
			int num = list.IndexOf(curPlayer.gameObject);
			for (int num2 = num - 1; num2 >= -list.Count; num2--)
			{
				int num3 = num2;
				if (num3 < 0)
				{
					num3 += list.Count;
				}
				int curClass = list[num3].GetComponent<CharacterClassManager>().curClass;
				if (curClass >= 0 && curClass != 2)
				{
					curPlayer = list[num3].GetComponent<SpectatorManager>();
					RefreshList();
					return;
				}
			}
		}
		if (curPlayer != null && !curPlayer.isLocalPlayer && curPlayer.actrl.headAnimator != null)
		{
			curPlayer.actrl.headAnimator.transform.localScale = Vector3.zero;
		}
		cam.cam.enabled = false;
		cam.freeCam.enabled = true;
		RefreshList();
	}

	private void LateUpdate()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		if (curPlayer != null)
		{
			curPlayer.TrackPlayer();
		}
		if (ccm.curClass == 2 && !Cursor.visible && Radio.roundStarted)
		{
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				NextPlayer();
			}
			if (Input.GetKeyDown(KeyCode.Mouse1))
			{
				PreviousPlayer();
			}
		}
		if (ccm.curClass != prevClass)
		{
			prevClass = ccm.curClass;
			RefreshList();
		}
	}

	public void TrackPlayer()
	{
		if (ccm.curClass != 2)
		{
			bool flag = ccm.curClass == 7;
			if (flag)
			{
				Scp079PlayerScript component = ccm.GetComponent<Scp079PlayerScript>();
				cam.cam.transform.position = ((!(scp079.currentCamera == null)) ? scp079.currentCamera.targetPosition.position : Interface079.singleton.defaultCamera.targetPosition.position);
				cam.cam.transform.rotation = ((!(scp079.currentCamera == null)) ? scp079.currentCamera.targetPosition.rotation : Interface079.singleton.defaultCamera.targetPosition.rotation);
				string arg = string.Format("AP: {0}/{1} ({2})", Mathf.Round(component.Mana), component.levels[component.Lvl].maxMana, component.levels[component.Lvl].label);
				inf.playerInfo.text = string.Format("{0}\n{1}{2}", ((!rlsMy.AmIInOverwatch) ? string.Empty : ("<color=#008080>OVERWATCH MODE</color>\nPlayer ID: " + qrpr.PlayerId + "\n")) + ((ccm.curClass >= 0) ? ccm.klasy[ccm.curClass].fullName : string.Empty), arg, rls.GetColoredRoleString(true));
			}
			else
			{
				cam.cam.transform.position = ((!flag) ? cameraPosition.position : ((!(scp079.currentCamera == null)) ? scp079.currentCamera.targetPosition.position : Interface079.singleton.defaultCamera.targetPosition.position));
				cam.cam.transform.rotation = Quaternion.Lerp(cam.cam.transform.rotation, Quaternion.Euler(pms.rotX, cameraPosition.eulerAngles.y, 0f), Time.deltaTime * 23f);
				inf.playerInfo.text = string.Format("{0}\n{1} HP{2}", ((!rlsMy.AmIInOverwatch) ? string.Empty : ("<color=#008080>OVERWATCH MODE</color>\nPlayer ID: " + qrpr.PlayerId + "\n")) + ((ccm.curClass >= 0) ? ccm.klasy[ccm.curClass].fullName : string.Empty), stats.health, rls.GetColoredRoleString(true));
			}
		}
	}

	private string ColorToHex(Color c)
	{
		Color32 color = c;
		string text = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
		return "<color=#" + text + ">";
	}

	private void UNetVersion()
	{
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
