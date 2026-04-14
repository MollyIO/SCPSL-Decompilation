using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class DebugScreenController : MonoBehaviour
{
	public GameObject Gui;

	public static int Asserts;

	public static int Errors;

	public static int Exceptions;

	private static bool _logged;

	private void Start()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		Application.logMessageReceived += LogMessage;
		if (Process.GetProcessesByName("SharpMonoInjector").Length > 0)
		{
			Application.Quit();
		}
		Log();
	}

	private static void Log()
	{
		if (!_logged)
		{
			UnityEngine.Debug.Log("OS: " + SystemInfo.operatingSystem + Environment.NewLine + "Build ID: " + Application.buildGUID);
			if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows && (SystemInfo.operatingSystem.Contains("7") || SystemInfo.operatingSystem.Contains("8.1")) && !File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.System) + Path.DirectorySeparatorChar + "API-MS-WIN-CRT-MATH-L1-1-0.dll"))
			{
				UnityEngine.Debug.Log("Important system file that is needed for voicechat is missing, please install this windows update in order to get your voicechat working https://support.microsoft.com/en-us/help/2999226/update-for-universal-c-runtime-in-windows");
			}
		}
	}

	private static void LogMessage(string condition, string stackTrace, LogType type)
	{
		switch (type)
		{
		case LogType.Assert:
			Asserts++;
			break;
		case LogType.Error:
			Errors++;
			break;
		case LogType.Exception:
			Exceptions++;
			break;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F3))
		{
			Gui.SetActive(!Gui.activeSelf);
		}
	}
}
