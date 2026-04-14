using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResolutionManager : MonoBehaviour
{
	[Serializable]
	public class ResolutionPreset
	{
		public int Height;

		public int Width;

		public ResolutionPreset(Resolution template)
		{
			Width = template.width;
			Height = template.height;
		}

		public void SetResolution()
		{
			Screen.SetResolution(Width, Height, Fullscreen);
		}
	}

	public static int Preset;

	public static bool Fullscreen;

	private static bool _initialized;

	public static List<ResolutionPreset> Presets = new List<ResolutionPreset>();

	private static bool FindResolution(Resolution res)
	{
		foreach (ResolutionPreset preset in Presets)
		{
			if (preset.Height == res.height && preset.Width == res.width)
			{
				return true;
			}
		}
		return false;
	}

	private void Start()
	{
		if (!_initialized)
		{
			InitialisePresets();
		}
		Preset = Mathf.Clamp(PlayerPrefs.GetInt("SavedResolutionSet", Presets.Count - 1), 0, Presets.Count - 1);
		Fullscreen = PlayerPrefs.GetInt("SavedFullscreen", 1) != 0;
		if (!ServerStatic.IsDedicated)
		{
			int num = PlayerPrefs.GetInt("MaxFramerate", 969);
			Application.targetFrameRate = ((num != 969) ? num : (-1));
		}
		RefreshScreen();
		SceneManager.sceneLoaded += OnSceneWasLoaded;
	}

	private static void OnSceneWasLoaded(Scene scene, LoadSceneMode mode)
	{
		RefreshScreen();
	}

	private static void InitialisePresets()
	{
		Presets.Clear();
		Resolution[] resolutions = Screen.resolutions;
		foreach (Resolution resolution in resolutions)
		{
			if (!FindResolution(resolution))
			{
				Presets.Add(new ResolutionPreset(resolution));
			}
		}
		_initialized = true;
	}

	public static void RefreshScreen()
	{
		if (!_initialized)
		{
			InitialisePresets();
		}
		if (Presets.Count == 0)
		{
			return;
		}
		Presets[Mathf.Clamp(Preset, 0, Presets.Count - 1)].SetResolution();
		try
		{
			UnityEngine.Object.FindObjectOfType<ResolutionText>().txt.text = Presets[Mathf.Clamp(Preset, 0, Presets.Count - 1)].Width + " × " + Presets[Mathf.Clamp(Preset, 0, Presets.Count - 1)].Height;
		}
		catch
		{
		}
	}

	public static void ChangeResolution(int id)
	{
		Preset = Mathf.Clamp(Preset + id, 0, Presets.Count - 1);
		PlayerPrefs.SetInt("SavedResolutionSet", Preset);
		RefreshScreen();
	}

	public static void ChangeFullscreen(bool isTrue)
	{
		Fullscreen = isTrue;
		PlayerPrefs.SetInt("SavedFullscreen", isTrue ? 1 : 0);
		RefreshScreen();
	}
}
