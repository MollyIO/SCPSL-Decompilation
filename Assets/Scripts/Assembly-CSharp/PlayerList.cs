using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GameConsole;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerList : NetworkBehaviour
{
	[Serializable]
	public class Instance
	{
		public GameObject text;

		public GameObject owner;
	}

	public Transform parent;

	public Transform template;

	public GameObject reportForm;

	public GameObject panel;

	public TextMeshProUGUI badgeText;

	public TextMeshProUGUI timerText;

	public TextMeshProUGUI serverNameText;

	public static InterfaceColorAdjuster ica;

	public static PlayerList singleton;

	[SyncVar]
	public int RoundStartTime;

	[SyncVar]
	public string syncServerName;

	private int timer;

	private static Transform s_parent;

	private static Transform s_template;

	private KeyCode openKey;

	public static bool anyAdminOnServer = false;

	public static List<Instance> instances = new List<Instance>();

	public int NetworkRoundStartTime
	{
		get
		{
			return RoundStartTime;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref RoundStartTime, 1u);
		}
	}

	public string NetworksyncServerName
	{
		get
		{
			return syncServerName;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref syncServerName, 2u);
		}
	}

	private void FixedUpdate()
	{
		if (!ServerStatic.IsDedicated && panel.activeSelf && (int)Time.realtimeSinceStartup - timer >= 1)
		{
			timer = (int)Time.realtimeSinceStartup;
			int num = timer - RoundStartTime;
			int num2 = num / 3600;
			int num3 = num % 3600 / 60;
			num %= 60;
			timerText.text = num2.ToString("00") + ":" + num3.ToString("00") + ":" + num.ToString("00");
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(openKey) && !reportForm.activeSelf)
		{
			if (panel.activeSelf)
			{
				panel.SetActive(false);
			}
			else if (!Cursor.visible || CursorManager.singleton.is079)
			{
				panel.SetActive(true);
			}
			CursorManager.singleton.plOp = panel.activeSelf;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			CursorManager.singleton.plOp = false;
			panel.SetActive(false);
			reportForm.SetActive(false);
		}
		badgeText.enabled = anyAdminOnServer;
		RectTransform component = GetComponent<RectTransform>();
		component.localPosition = Vector3.zero;
		component.sizeDelta = Vector2.zero;
		if (!string.IsNullOrEmpty(syncServerName) && serverNameText.text != syncServerName)
		{
			serverNameText.text = syncServerName;
		}
	}

	private void Start()
	{
		openKey = NewInput.GetKey("Player List");
		s_parent = parent;
		s_template = template;
		anyAdminOnServer = false;
		if (NetworkServer.active)
		{
			NetworksyncServerName = ((!string.IsNullOrEmpty(ConfigFile.ServerConfig.GetString("player_list_title", string.Empty))) ? ConfigFile.ServerConfig.GetString("player_list_title", string.Empty) : ConfigFile.ServerConfig.GetString("server_name", "Player List"));
		}
	}

	private void Awake()
	{
		instances.Clear();
		singleton = this;
	}

	public static void AddPlayer(GameObject instance)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(s_template.gameObject, s_parent);
		gameObject.transform.localScale = Vector3.one;
		gameObject.GetComponentInChildren<TextMeshProUGUI>().text = instance.GetComponent<NicknameSync>().myNick;
		gameObject.GetComponent<PlayerListElement>().instance = instance;
		instances.Add(new Instance
		{
			owner = instance,
			text = gameObject
		});
		UpdatePlayerRole(instance);
	}

	public static void UpdatePlayerRole(GameObject instance)
	{
		anyAdminOnServer = false;
		foreach (Instance instance2 in instances)
		{
			if (!anyAdminOnServer && !string.IsNullOrEmpty(instance2.owner.GetComponent<ServerRoles>().GetUncoloredRoleString()))
			{
				anyAdminOnServer = true;
			}
			if (instance != instance2.owner)
			{
				continue;
			}
			ServerRoles component = instance.GetComponent<ServerRoles>();
			TextMeshProUGUI[] componentsInChildren = instance2.text.GetComponentsInChildren<TextMeshProUGUI>();
			foreach (TextMeshProUGUI textMeshProUGUI in componentsInChildren)
			{
				switch (textMeshProUGUI.name)
				{
				case "Nickname":
					textMeshProUGUI.text = instance.GetComponent<NicknameSync>().myNick;
					textMeshProUGUI.color = component.GetColor();
					break;
				case "Badge":
					textMeshProUGUI.text = component.GetUncoloredRoleString();
					textMeshProUGUI.color = component.GetColor();
					break;
				}
			}
			instance2.text.GetComponent<Image>().color = ica.graphicsToChange[0].color;
		}
	}

	public static void UpdateColors()
	{
		Color color = ica.graphicsToChange[0].color;
		color.a = 1f;
		foreach (Instance instance in instances)
		{
			instance.text.GetComponent<Image>().color = color;
		}
	}

	public static void DestroyPlayer(GameObject instance)
	{
		foreach (Instance instance2 in instances)
		{
			if (instance2.owner != instance)
			{
				continue;
			}
			UnityEngine.Object.Destroy(instance2.text.gameObject);
			instances.Remove(instance2);
			break;
		}
	}

	public void Report()
	{
		int result = -1;
		TextMeshProUGUI[] componentsInChildren = reportForm.GetComponentsInChildren<TextMeshProUGUI>();
		foreach (TextMeshProUGUI textMeshProUGUI in componentsInChildren)
		{
			if (textMeshProUGUI.name == "Player ID")
			{
				int.TryParse(textMeshProUGUI.text, out result);
				break;
			}
		}
		if (result > 0)
		{
			GameConsole.Console.singleton.TypeCommand("report " + result + " " + reportForm.GetComponentInChildren<InputField>().text);
		}
		CloseForm();
	}

	public void CloseForm()
	{
		reportForm.SetActive(false);
		CursorManager.singleton.plOp = false;
		PlayerManager.localPlayer.GetComponent<FirstPersonController>().isPaused = false;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)RoundStartTime);
			writer.Write(syncServerName);
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
			writer.WritePackedUInt32((uint)RoundStartTime);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(syncServerName);
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
			RoundStartTime = (int)reader.ReadPackedUInt32();
			syncServerName = reader.ReadString();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			RoundStartTime = (int)reader.ReadPackedUInt32();
		}
		if ((num & 2) != 0)
		{
			syncServerName = reader.ReadString();
		}
	}
}
