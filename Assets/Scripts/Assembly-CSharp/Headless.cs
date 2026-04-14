using System;
using UnityEngine;
using UnityEngine.Rendering;
using Windows;

public static class Headless
{
	public static readonly string version = "1.4.4";

	private static bool isHeadless;

	private static bool checkedHeadless;

	private static bool initializedHeadless;

	private static bool buildingHeadless;

	private static HeadlessRuntime headlessRuntime;

	private static string currentProfile = string.Empty;

	public static string GetProfileName()
	{
		if (!IsHeadless())
		{
			return null;
		}
		InitializeHeadless();
		return currentProfile;
	}

	public static bool IsHeadless()
	{
		if (checkedHeadless)
		{
			return isHeadless;
		}
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (Array.IndexOf(commandLineArgs, "-batchmode") >= 0)
		{
			isHeadless = true;
		}
		else if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
		{
			isHeadless = true;
		}
		checkedHeadless = true;
		return isHeadless;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnBeforeSceneLoadRuntimeMethod()
	{
		if (IsHeadless())
		{
			InitializeHeadless();
			HeadlessCallbacks.InvokeCallbacks("HeadlessBeforeSceneLoad");
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void OnAfterSceneLoadRuntimeMethod()
	{
		if (!IsHeadless())
		{
			return;
		}
		if (headlessRuntime.valueCamera)
		{
			GameObject gameObject = GameObject.Find("HeadlessBehaviour");
			if (gameObject == null)
			{
				gameObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("HeadlessBehaviour"));
			}
			HeadlessBehaviour headlessBehaviour = gameObject.GetComponent<HeadlessBehaviour>();
			if (headlessBehaviour == null)
			{
				headlessBehaviour = gameObject.AddComponent<HeadlessBehaviour>();
			}
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(headlessBehaviour.GetComponent<HeadlessBehaviour>().NullifyCamera));
		}
		HeadlessCallbacks.InvokeCallbacks("HeadlessAfterSceneLoad");
	}

	private static void InitializeHeadless()
	{
		if (initializedHeadless)
		{
			return;
		}
		headlessRuntime = Resources.Load("HeadlessRuntime") as HeadlessRuntime;
		if (headlessRuntime != null)
		{
			currentProfile = headlessRuntime.profileName;
			if (headlessRuntime.valueConsole)
			{
				HeadlessConsole headlessConsole = new HeadlessConsole();
				headlessConsole.Initialize();
				headlessConsole.SetTitle(Application.productName);
				Application.logMessageReceived += HandleLog;
			}
			if (headlessRuntime.valueLimitFramerate)
			{
				Application.targetFrameRate = headlessRuntime.valueFramerate;
				Debug.Log("Application target framerate set to " + headlessRuntime.valueFramerate);
			}
		}
		initializedHeadless = true;
		HeadlessCallbacks.InvokeCallbacks("HeadlessBeforeFirstSceneLoad");
	}

	private static void HandleLog(string logString, string stackTrace, LogType type)
	{
		Console.WriteLine(logString);
		if (stackTrace.Length > 1)
		{
			Console.WriteLine("in: " + stackTrace);
		}
	}

	public static bool IsBuildingHeadless()
	{
		return buildingHeadless;
	}

	public static void SetBuildingHeadless(bool value, string profileName)
	{
		buildingHeadless = value;
		currentProfile = profileName;
	}
}
