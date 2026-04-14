using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityWebGLSpeechSynthesis
{
	public class SpeechSynthesisUtils
	{
		private const string KEY_WEBGL_SPEECH_SYNTHESIS_PLUGIN_VOICE = "WEBGL_SPEECH_SYNTHESIS_PLUGIN_VOICE";

		private static bool _sPausePreferences;

		public static void PausePlayerPrefs(bool toggle)
		{
			_sPausePreferences = toggle;
		}

		public static void SetActive(bool value, params Component[] args)
		{
			if (args == null)
			{
				return;
			}
			foreach (Component component in args)
			{
				if ((bool)component && (bool)component.gameObject)
				{
					component.gameObject.SetActive(value);
				}
			}
		}

		public static void SetInteractable(bool interactable, params Selectable[] args)
		{
			if (args == null)
			{
				return;
			}
			foreach (Selectable selectable in args)
			{
				if ((bool)selectable)
				{
					selectable.interactable = interactable;
				}
			}
		}

		public static void PopulateDropdown(Dropdown dropdown, List<string> strings)
		{
			if (!dropdown)
			{
				return;
			}
			dropdown.ClearOptions();
			List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
			foreach (string @string in strings)
			{
				Dropdown.OptionData optionData = new Dropdown.OptionData();
				optionData.text = @string;
				list.Add(optionData);
			}
			dropdown.AddOptions(list);
		}

		public static void PopulateVoicesDropdown(Dropdown dropdown, VoiceResult result)
		{
			if (!dropdown)
			{
				return;
			}
			List<string> list = new List<string>();
			list.Add("Voices");
			List<string> list2 = list;
			if (((result != null) ? result.voices : null) != null)
			{
				for (int i = 0; i < result.voices.Length; i++)
				{
					Voice voice = result.voices[i];
					if (voice != null)
					{
						if (!string.IsNullOrEmpty(voice.display))
						{
							list2.Add(voice.display);
						}
						else if (!string.IsNullOrEmpty(voice.name))
						{
							list2.Add(voice.name);
						}
					}
				}
			}
			PopulateDropdown(dropdown, list2);
		}

		public static void PopulateVoices(out string[] voiceOptions, out int voiceIndex, VoiceResult result)
		{
			voiceOptions = null;
			voiceIndex = 0;
			if (result == null)
			{
				Debug.LogError("VoiceResult is null!");
				return;
			}
			List<string> list = new List<string>();
			list.Add("Voices");
			if (result.voices != null)
			{
				for (int i = 0; i < result.voices.Length; i++)
				{
					Voice voice = result.voices[i];
					if (voice != null)
					{
						if (!string.IsNullOrEmpty(voice.display))
						{
							list.Add(voice.display);
						}
						else if (!string.IsNullOrEmpty(voice.name))
						{
							list.Add(voice.name);
						}
					}
				}
			}
			voiceOptions = list.ToArray();
		}

		public static void SelectIndex(Dropdown dropdown, int val)
		{
			if ((bool)dropdown && val < dropdown.options.Count)
			{
				dropdown.value = val;
			}
		}

		public static Voice GetVoice(VoiceResult voiceResult, string display)
		{
			if (((voiceResult != null) ? voiceResult.voices : null) == null)
			{
				Debug.LogError("Voices are not set!");
				return null;
			}
			Voice[] voices = voiceResult.voices;
			foreach (Voice voice in voices)
			{
				if (!string.IsNullOrEmpty(voice.display) && voice.display.Equals(display))
				{
					return voice;
				}
				if (!string.IsNullOrEmpty(voice.name) && voice.name.Equals(display))
				{
					return voice;
				}
			}
			return null;
		}

		public static void HandleVoiceChangedDropdown(Dropdown dropdown, VoiceResult voiceResult, SpeechSynthesisUtterance utterance, ISpeechSynthesisPlugin plugin)
		{
			if (null == dropdown)
			{
				Debug.LogError("The dropdown for voices is not set!");
				return;
			}
			if (voiceResult == null)
			{
				Debug.LogError("The voice result is not set!");
				return;
			}
			if (utterance == null)
			{
				Debug.LogError("The utterance is not set!");
				return;
			}
			if (plugin == null)
			{
				Debug.LogError("The plugin is not set!");
				return;
			}
			string text = dropdown.options[dropdown.value].text;
			Voice voice = null;
			if (dropdown.value > 0)
			{
				SetDefaultVoice(text);
				voice = GetVoice(voiceResult, text);
				if (voice == null)
				{
					Debug.LogError("Did not find specified voice!");
				}
			}
			if (voice != null)
			{
				plugin.SetVoice(utterance, voice);
			}
		}

		public static void HandleVoiceChanged(string[] voiceOptions, int voiceIndex, VoiceResult voiceResult, SpeechSynthesisUtterance utterance, ISpeechSynthesisPlugin plugin)
		{
			if (voiceOptions == null)
			{
				Debug.LogError("Voice options are not set!");
				return;
			}
			if (voiceResult == null)
			{
				Debug.LogError("The voice result is not set!");
				return;
			}
			if (utterance == null)
			{
				Debug.LogError("The utterance is not set!");
				return;
			}
			if (plugin == null)
			{
				Debug.LogError("The plugin is not set!");
				return;
			}
			string text = voiceOptions[voiceIndex];
			Voice voice = null;
			if (voiceIndex > 0)
			{
				SetDefaultVoice(text);
				voice = GetVoice(voiceResult, text);
				if (voice == null)
				{
					Debug.LogError("Did not find specified voice!");
				}
			}
			if (voice != null)
			{
				plugin.SetVoice(utterance, voice);
			}
		}

		public static string GetDefaultVoice()
		{
			if (!PlayerPrefs.HasKey("WEBGL_SPEECH_SYNTHESIS_PLUGIN_VOICE"))
			{
				return string.Empty;
			}
			string text = PlayerPrefs.GetString("WEBGL_SPEECH_SYNTHESIS_PLUGIN_VOICE");
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}
			return text;
		}

		public static void SetDefaultVoice(string voice)
		{
			if (!_sPausePreferences)
			{
				PlayerPrefs.SetString("WEBGL_SPEECH_SYNTHESIS_PLUGIN_VOICE", voice);
			}
		}

		public static void RestoreVoice(Dropdown dropdownVoices)
		{
			try
			{
				PausePlayerPrefs(true);
				if (null == dropdownVoices)
				{
					Debug.LogError("Dropdown Voices not set!");
					return;
				}
				string defaultVoice = GetDefaultVoice();
				if (string.IsNullOrEmpty(defaultVoice))
				{
					return;
				}
				for (int i = 0; i < dropdownVoices.options.Count; i++)
				{
					Dropdown.OptionData optionData = dropdownVoices.options[i];
					if (optionData.text == defaultVoice)
					{
						dropdownVoices.value = i;
						break;
					}
				}
			}
			finally
			{
				PausePlayerPrefs(false);
			}
		}

		public static void RestoreVoice(string[] voiceOptions, out int voiceIndex)
		{
			try
			{
				PausePlayerPrefs(true);
				voiceIndex = 0;
				if (voiceOptions == null)
				{
					Debug.LogError("Voices options are not set!");
					return;
				}
				string defaultVoice = GetDefaultVoice();
				if (string.IsNullOrEmpty(defaultVoice))
				{
					return;
				}
				for (int i = 0; i < voiceOptions.Length; i++)
				{
					string text = voiceOptions[i];
					if (text == defaultVoice)
					{
						voiceIndex = i;
						break;
					}
				}
			}
			finally
			{
				PausePlayerPrefs(false);
			}
		}
	}
}
