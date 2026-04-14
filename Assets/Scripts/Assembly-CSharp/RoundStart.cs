using System.Collections.Generic;
using System.Runtime.InteropServices;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RoundStart : NetworkBehaviour
{
	public GameObject window;

	public GameObject forceButton;

	public TextMeshProUGUI playersNumber;

	public Image loadingbar;

	public static RoundStart singleton;

	public static bool RoundJustStarted;

	[SyncVar(hook = "SetInfo")]
	public string info = string.Empty;

	public string Networkinfo
	{
		get
		{
			return info;
		}
		[param: In]
		set
		{
			RoundStart roundStart = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetInfo(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref roundStart.info, 1u);
		}
	}

	private void Start()
	{
		if (base.isServer)
		{
			RoundJustStarted = true;
		}
		GetComponent<RectTransform>().localPosition = Vector3.zero;
		if (NetworkServer.active)
		{
			Timing.RunCoroutine(_AntiNonclass(), Segment.FixedUpdate);
		}
	}

	private IEnumerator<float> _AntiNonclass()
	{
		while (info != "started")
		{
			yield return 0f;
		}
		RoundJustStarted = true;
		if (base.isServer)
		{
			Timing.RunCoroutine(AntiFloorStuck(), Segment.FixedUpdate);
		}
		for (int i = 0; i < 500; i++)
		{
			yield return 0f;
		}
		while (this != null)
		{
			yield return 0f;
			GameObject[] objects = PlayerManager.singleton.players;
			GameObject[] array = objects;
			foreach (GameObject item in array)
			{
				yield return 0f;
				if (!(item == null))
				{
					CharacterClassManager c = item.GetComponent<CharacterClassManager>();
					if (c.curClass < 0 && c.IsVerified)
					{
						c.SetPlayersClass(2, c.gameObject);
					}
				}
			}
		}
	}

	private IEnumerator<float> AntiFloorStuck()
	{
		yield return Timing.WaitForSeconds(5f);
		RoundJustStarted = false;
	}

	private void Awake()
	{
		singleton = this;
		if (base.isServer)
		{
			RoundJustStarted = true;
		}
	}

	private void Update()
	{
		window.SetActive(info != string.Empty && info != "started");
		float result = 0f;
		float.TryParse(info, out result);
		result -= 1f;
		result /= 19f;
		loadingbar.fillAmount = Mathf.Lerp(loadingbar.fillAmount, result, Time.deltaTime);
		playersNumber.text = PlayerManager.singleton.players.Length.ToString();
	}

	public void SetInfo(string i)
	{
		Networkinfo = i;
	}

	public void ShowButton()
	{
		forceButton.SetActive(true);
	}

	public void UseButton()
	{
		forceButton.SetActive(false);
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
			if (component.isLocalPlayer && gameObject.name == "Host")
			{
				component.ForceRoundStart();
			}
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(info);
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
			writer.Write(info);
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
			info = reader.ReadString();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetInfo(reader.ReadString());
		}
	}
}
