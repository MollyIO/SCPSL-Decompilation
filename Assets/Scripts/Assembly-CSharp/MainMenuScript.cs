using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
	public GameObject[] submenus;

	private CustomNetworkManager _mng;

	public int CurMenu;

	public static bool Openinfo;

	private bool allowQuit;

	private void Update()
	{
		if (SceneManager.GetActiveScene().name.ToLower().Contains("menu"))
		{
			CursorManager.UnsetAll();
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	public void SetIP(string ip)
	{
		NetworkServer.Reset();
		_mng.networkPort = 7777;
		try
		{
			string s = ip.Remove(0, ip.LastIndexOf(":", StringComparison.Ordinal) + 1);
			_mng.networkPort = int.Parse(s);
		}
		catch
		{
		}
		_mng.networkAddress = ((!ip.Contains(":")) ? ip : ip.Remove(ip.IndexOf(":", StringComparison.Ordinal)));
		CustomNetworkManager.ConnectionIp = ((!ip.Contains(":")) ? ip : ip.Remove(ip.IndexOf(":", StringComparison.Ordinal)));
	}

	public void ChangeMenu(int id)
	{
		CurMenu = id;
		for (int i = 0; i < submenus.Length; i++)
		{
			submenus[i].SetActive(i == id);
		}
		MenuAnimator.wasEverZoomed = id > 0;
	}

	public void ResetMenu()
	{
		for (int i = 0; i < submenus.Length; i++)
		{
			submenus[i].SetActive(i == CurMenu && !Openinfo);
		}
	}

	public void QuitGame()
	{
		allowQuit = true;
		Application.Quit();
	}

	private void Start()
	{
		_mng = UnityEngine.Object.FindObjectOfType<CustomNetworkManager>();
		CursorManager.UnsetAll();
		ChangeMenu(0);
	}

	private void OnApplicationQuit()
	{
		if (!allowQuit && !Input.GetKey(KeyCode.LeftAlt))
		{
			Application.CancelQuit();
		}
	}

	public void StartServer()
	{
		_mng.onlineScene = "Facility";
		_mng.maxConnections = 20;
		_mng.createpop.SetActive(true);
	}

	public void StartTutorial(string scene)
	{
		_mng.onlineScene = scene;
		_mng.maxConnections = 1;
		_mng.ShowLog(15, string.Empty, string.Empty);
		_mng.StartHost();
	}

	public void Connect()
	{
		if (!CrashDetector.Show())
		{
			NetworkServer.Reset();
			_mng.ShowLog(13, string.Empty, string.Empty);
			_mng.StartClient();
		}
	}
}
