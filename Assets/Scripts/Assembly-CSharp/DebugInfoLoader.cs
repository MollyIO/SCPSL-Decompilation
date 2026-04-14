using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DebugInfoLoader : MonoBehaviour
{
	public Text Audio;

	public Text Cpu;

	public Text CpuThreadsAndFrequency;

	public Text Gpu;

	public Text GpuMemory;

	public Text GraphicApi;

	public Text Os;

	public Text Ram;

	public Text Resolution;

	public Text Fullscreen;

	public Text ShaderLevel;

	public Text Steam;

	public Text UnityVersion;

	public Text GameVersion;

	public Text Build;

	public Text GameLanguage;

	public Text GameScene;

	public Text Errors;

	public Text CentralServerText;

	private string _centralserver = string.Empty;

	private void OnEnable()
	{
		Gpu.text = SystemInfo.graphicsDeviceName;
		GpuMemory.text = "VRAM: " + SystemInfo.graphicsMemorySize + "MB";
		ShaderLevel.text = "ShaderLevel " + SystemInfo.graphicsShaderLevel.ToString().Insert(1, ".");
		GraphicApi.text = string.Concat(SystemInfo.graphicsDeviceType, " ", SystemInfo.graphicsDeviceVersion);
		Resolution.text = Screen.width + "x" + Screen.height + "  " + Application.targetFrameRate;
		Fullscreen.text = "Fullscreen: " + ResolutionManager.Fullscreen;
		Cpu.text = SystemInfo.processorType;
		CpuThreadsAndFrequency.text = "Threads: " + SystemInfo.processorCount + "   " + SystemInfo.processorFrequency + "MHz";
		Ram.text = "RAM: " + SystemInfo.systemMemorySize + "MB";
		Audio.text = "Audio Supported: " + SystemInfo.supportsAudio;
		Os.text = SystemInfo.operatingSystem.Replace("  ", " ");
		Steam.text = "Steam: " + SteamManager.GetApiState();
		UnityVersion.text = "Unity " + Application.unityVersion;
		GameVersion.text = "Version: " + CustomNetworkManager.CompatibleVersions[0];
		string text = Application.buildGUID;
		if (string.IsNullOrEmpty(text))
		{
			text = "Unity Editor";
		}
		Build.text = "Build: " + text;
		_centralserver = "Central Server: " + CentralServer.SelectedServer;
		CentralServerText.text = _centralserver;
		GameLanguage.text = "Language:" + PlayerPrefs.GetString("translation_path", "English (default)");
		GameScene.text = "Scene: " + SceneManager.GetActiveScene().name;
		Errors.text = "Asserts: " + DebugScreenController.Asserts + " Errors: " + DebugScreenController.Errors + " Exceptions: " + DebugScreenController.Exceptions;
	}

	private void FixedUpdate()
	{
		if (!string.IsNullOrEmpty(CentralServer.SelectedServer) && !_centralserver.Contains(CentralServer.SelectedServer))
		{
			_centralserver = "Central Server: " + CentralServer.SelectedServer;
			CentralServerText.text = _centralserver;
		}
	}
}
