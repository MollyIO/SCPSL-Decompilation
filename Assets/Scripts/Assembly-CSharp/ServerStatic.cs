using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerStatic : MonoBehaviour
{
	public static bool IsDedicated;

	public static bool ProcessIdPassed;

	public bool Simulate;

	public static bool DisableConfigValidation;

	public static bool ShareNonConfigs;

	internal static YamlConfig RolesConfig;

	internal static string RolesConfigPath;

	internal static PermissionsHandler PermissionsHandler;

	private void Awake()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		DisableConfigValidation = commandLineArgs.Contains("-disableconfigvalidation");
		ShareNonConfigs = commandLineArgs.Contains("-sharenonconfigs");
		for (int i = 0; i < commandLineArgs.Length - 1; i++)
		{
			if (commandLineArgs[i] == "-configpath")
			{
				FileManager.SetAppFolder(commandLineArgs[i + 1].Replace("\"", string.Empty).Trim());
			}
		}
		string[] array = commandLineArgs;
		foreach (string text in array)
		{
			if (text == "-nographics" && !Simulate)
			{
				Simulate = true;
			}
			if (text.Contains("-key"))
			{
				ServerConsole.Session = text.Remove(0, 4);
			}
			if (!text.Contains("-id"))
			{
				continue;
			}
			ProcessIdPassed = true;
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
			{
				if (process.Id.ToString() == text.Remove(0, 3))
				{
					ServerConsole.ConsoleId = process;
					ServerConsole.ConsoleId.Exited += OnConsoleExited;
					break;
				}
			}
		}
		if (Simulate)
		{
			IsDedicated = true;
			AudioListener.volume = 0f;
			AudioListener.pause = true;
			QualitySettings.pixelLightCount = 0;
			GUI.enabled = false;
			ServerConsole.AddLog("SCP Secret Laboratory process started. Creating match... LOGTYPE02");
		}
		SceneManager.sceneLoaded += OnSceneWasLoaded;
	}

	private static void OnConsoleExited(object sender, EventArgs e)
	{
		ServerConsole.DisposeStatic();
		IsDedicated = false;
		Application.Quit();
	}

	private void OnSceneWasLoaded(Scene scene, LoadSceneMode mode)
	{
		if (IsDedicated && (scene.buildIndex == 3 || scene.buildIndex == 4))
		{
			GetComponent<CustomNetworkManager>().CreateMatch();
		}
	}

	public static PermissionsHandler GetPermissionsHandler()
	{
		return PermissionsHandler;
	}
}
