using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleMenu : MonoBehaviour
{
	private string targetSceneName;

	private static bool server;

	private static SimpleMenu singleton;

	public bool isPreloader;

	private void Awake()
	{
		if (isPreloader)
		{
			return;
		}
		singleton = this;
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			switch (commandLineArgs[i])
			{
			case "-fastmenu":
				PlayerPrefs.SetInt("fastmenu", 1);
				break;
			case "-nographics":
				server = true;
				break;
			}
		}
		Refresh();
	}

	private void Start()
	{
		if (isPreloader)
		{
			SceneManager.LoadScene("Loader");
		}
	}

	public void ChangeMode()
	{
		PlayerPrefs.SetInt("fastmenu", (PlayerPrefs.GetInt("fastmenu", 0) == 0) ? 1 : 0);
		Refresh();
		SceneManager.LoadScene("Loader");
	}

	private void Refresh()
	{
		if (server)
		{
			targetSceneName = "FastMenu";
		}
		else
		{
			targetSceneName = ((PlayerPrefs.GetInt("fastmenu", 0) != 0) ? "FastMenu" : "MainMenuRemastered");
		}
		UnityEngine.Object.FindObjectOfType<CustomNetworkManager>().offlineScene = targetSceneName;
	}

	public static void LoadCorrectScene()
	{
		SceneManager.LoadScene(singleton.targetSceneName);
	}
}
