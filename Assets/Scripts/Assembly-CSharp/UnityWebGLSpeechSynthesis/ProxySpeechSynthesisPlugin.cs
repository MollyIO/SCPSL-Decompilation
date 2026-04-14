using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace UnityWebGLSpeechSynthesis
{
	public class ProxySpeechSynthesisPlugin : BaseSpeechSynthesisPlugin, ISpeechSynthesisPlugin
	{
		private const string KEY_CHROME_SPEECH_PROXY = "CHROME_SPEECH_PROXY";

		public int _mPort = 83;

		private static ProxySpeechSynthesisPlugin _sInstance;

		private bool _mIsAvailable;

		private int _mNextPort;

		private List<Action<SpeechSynthesisUtterance>> _mUtteranceCallbacks = new List<Action<SpeechSynthesisUtterance>>();

		private List<Action<VoiceResult>> _mGetVoicesCallbacks = new List<Action<VoiceResult>>();

		private List<string> _mPendingCommands = new List<string>();

		public static ProxySpeechSynthesisPlugin GetInstance()
		{
			return _sInstance;
		}

		protected virtual void SafeStartCoroutine(string routineName, IEnumerator routine)
		{
			StartCoroutine(routine);
		}

		private void Awake()
		{
			_sInstance = this;
		}

		protected virtual void Start()
		{
			SafeStartCoroutine("Init", Init());
		}

		protected IEnumerator Init()
		{
			while (true)
			{
				string url = string.Format("http://localhost:{0}/SpeechSynthesisConnect", _mPort);
				IWWW www = CreateWWW(url);
				while (www.GetError() == null && !www.IsDone())
				{
					yield return null;
				}
				string error = www.GetError();
				bool hasError = !string.IsNullOrEmpty(error);
				www.Dispose();
				if (!hasError)
				{
					break;
				}
				DateTime wait = DateTime.Now + TimeSpan.FromSeconds(1.0);
				while (DateTime.Now < wait)
				{
					yield return null;
				}
			}
			_mIsAvailable = true;
			SafeStartCoroutine("RunPendingCommands", RunPendingCommands());
			SafeStartCoroutine("ProxyOnEnd", ProxyOnEnd());
		}

		private IEnumerator RunPendingCommands()
		{
			while (true)
			{
				if (_mPendingCommands.Count == 0)
				{
					yield return null;
					continue;
				}
				string command = _mPendingCommands[0];
				if (command == "ClearPendingCommands")
				{
					_mPendingCommands.Clear();
					yield return null;
					continue;
				}
				if (command == "SetProxyPort")
				{
					_mPendingCommands.Clear();
					_mPort = _mNextPort;
					yield return null;
					continue;
				}
				string url = string.Format("http://localhost:{0}/{1}", _mPort, command);
				IWWW www = CreateWWW(url);
				while (www.GetError() == null && !www.IsDone())
				{
					yield return null;
				}
				string error = www.GetError();
				bool hasError = !string.IsNullOrEmpty(error);
				www.Dispose();
				if (hasError)
				{
					UnityEngine.Debug.LogError(error);
					DateTime wait = DateTime.Now + TimeSpan.FromSeconds(1.0);
					while (DateTime.Now < wait)
					{
						yield return null;
					}
				}
				else
				{
					_mPendingCommands.RemoveAt(0);
					yield return null;
				}
			}
		}

		private IEnumerator ProxyUtterance()
		{
			int index;
			while (true)
			{
				string url = string.Format("http://localhost:{0}/SpeechSynthesisProxyUtterance", _mPort);
				IWWW www = CreateWWW(url);
				while (www.GetError() == null && !www.IsDone())
				{
					yield return null;
				}
				string error = www.GetError();
				bool hasError = !string.IsNullOrEmpty(error);
				string text = null;
				if (!hasError)
				{
					text = www.GetText();
				}
				www.Dispose();
				DateTime wait;
				if (hasError)
				{
					wait = DateTime.Now + TimeSpan.FromSeconds(1.0);
					while (DateTime.Now < wait)
					{
						yield return null;
					}
					continue;
				}
				if (int.TryParse(text, out index))
				{
					break;
				}
				wait = DateTime.Now + TimeSpan.FromSeconds(1.0);
				while (DateTime.Now < wait)
				{
					yield return null;
				}
			}
			if (_mUtteranceCallbacks.Count > 0)
			{
				Action<SpeechSynthesisUtterance> action = _mUtteranceCallbacks[0];
				_mUtteranceCallbacks.RemoveAt(0);
				if (action != null)
				{
					SpeechSynthesisUtterance speechSynthesisUtterance = new SpeechSynthesisUtterance();
					speechSynthesisUtterance._mReference = index;
					action(speechSynthesisUtterance);
				}
			}
		}

		private IEnumerator ProxyVoices()
		{
			string jsonData;
			while (true)
			{
				string url = string.Format("http://localhost:{0}/SpeechSynthesisProxyVoices", _mPort);
				IWWW www = CreateWWW(url);
				while (www.GetError() == null && !www.IsDone())
				{
					yield return null;
				}
				string error = www.GetError();
				bool hasError = !string.IsNullOrEmpty(error);
				jsonData = null;
				if (!hasError)
				{
					jsonData = www.GetText();
				}
				www.Dispose();
				DateTime wait;
				if (hasError)
				{
					wait = DateTime.Now + TimeSpan.FromSeconds(1.0);
					while (DateTime.Now < wait)
					{
						yield return null;
					}
					continue;
				}
				if (!string.IsNullOrEmpty(jsonData))
				{
					break;
				}
				wait = DateTime.Now + TimeSpan.FromSeconds(1.0);
				while (DateTime.Now < wait)
				{
					yield return null;
				}
			}
			VoiceResult result = JsonUtility.FromJson<VoiceResult>(jsonData);
			if (((result != null) ? result.voices : null) != null)
			{
				for (int i = 0; i < result.voices.Length; i++)
				{
					Voice voice = result.voices[i];
					if (((voice != null) ? voice.lang : null) != null)
					{
						string text = null;
						switch (voice.lang)
						{
						case "de-DE":
							text = "German (Germany)";
							break;
						case "es-ES":
							text = "Spanish (Spain)";
							break;
						case "es-US":
							text = "Spanish (United States)";
							break;
						case "fr-FR":
							text = "French (France)";
							break;
						case "hi-IN":
							text = "Hindi (India)";
							break;
						case "id-ID":
							text = "Indonesian (Indonesia)";
							break;
						case "it-IT":
							text = "Italian (Italy)";
							break;
						case "ja-JP":
							text = "Japanese (Japan)";
							break;
						case "ko-KR":
							text = "Korean (South Korea)";
							break;
						case "nl-NL":
							text = "Dutch (Netherlands)";
							break;
						case "pl-PL":
							text = "Polish (Poland)";
							break;
						case "pt-BR":
							text = "Portuguese (Brazil)";
							break;
						case "ru-RU":
							text = "Russian (Russia)";
							break;
						case "zh-CN":
							text = "Mandarin (China)";
							break;
						case "zh-HK":
							text = "Cantonese (China)";
							break;
						case "zh-TW":
							text = "Mandarin (Taiwan)";
							break;
						}
						if (text != null)
						{
							voice.display = text;
						}
					}
				}
			}
			if (_mGetVoicesCallbacks.Count > 0)
			{
				Action<VoiceResult> action = _mGetVoicesCallbacks[0];
				_mGetVoicesCallbacks.RemoveAt(0);
				if (action != null)
				{
					action(result);
				}
			}
		}

		private IEnumerator ProxyOnEnd()
		{
			while (true)
			{
				string url = string.Format("http://localhost:{0}/SpeechSynthesisProxyOnEnd", _mPort);
				IWWW www = CreateWWW(url);
				while (www.GetError() == null && !www.IsDone())
				{
					yield return null;
				}
				string error = www.GetError();
				bool hasError = !string.IsNullOrEmpty(error);
				string jsonData = null;
				if (!hasError)
				{
					jsonData = www.GetText();
				}
				www.Dispose();
				if (hasError)
				{
					DateTime wait = DateTime.Now + TimeSpan.FromSeconds(1.0);
					while (DateTime.Now < wait)
					{
						yield return null;
					}
					continue;
				}
				if (string.IsNullOrEmpty(jsonData))
				{
					DateTime wait = DateTime.Now + TimeSpan.FromSeconds(1.0);
					while (DateTime.Now < wait)
					{
						yield return null;
					}
					continue;
				}
				SpeechSynthesisEvent speechSynthesisEvent = JsonUtility.FromJson<SpeechSynthesisEvent>(jsonData);
				if (speechSynthesisEvent != null)
				{
					for (int i = 0; i < BaseSpeechSynthesisPlugin._sOnSynthesisOnEnd.Count; i++)
					{
						DelegateHandleSynthesisOnEnd delegateHandleSynthesisOnEnd = BaseSpeechSynthesisPlugin._sOnSynthesisOnEnd[i];
						if (delegateHandleSynthesisOnEnd != null)
						{
							delegateHandleSynthesisOnEnd(speechSynthesisEvent);
						}
					}
				}
				yield return new WaitForFixedUpdate();
			}
		}

		public bool IsAvailable()
		{
			return _mIsAvailable;
		}

		private void AddCommand(string command)
		{
			_mPendingCommands.Add(command);
		}

		public void CreateSpeechSynthesisUtterance(Action<SpeechSynthesisUtterance> callback)
		{
			if (callback == null)
			{
				UnityEngine.Debug.LogError("Callback was not set!");
				return;
			}
			_mUtteranceCallbacks.Add(callback);
			AddCommand("SpeechSynthesisCreateSpeechSynthesisUtterance");
			SafeStartCoroutine("ProxyUtterance", ProxyUtterance());
		}

		public void GetVoices(Action<VoiceResult> callback)
		{
			if (callback == null)
			{
				UnityEngine.Debug.LogError("Callback was not set!");
				return;
			}
			AddCommand("SpeechSynthesisGetVoices");
			_mGetVoicesCallbacks.Add(callback);
			SafeStartCoroutine("ProxyVoices", ProxyVoices());
		}

		public void SetPitch(SpeechSynthesisUtterance utterance, float pitch)
		{
			if (utterance == null)
			{
				UnityEngine.Debug.LogError("Utterance was not set!");
				return;
			}
			string command = string.Format("SpeechSynthesisSetPitch?utterance={0}&pitch={1}", utterance._mReference, pitch);
			AddCommand(command);
		}

		public void SetRate(SpeechSynthesisUtterance utterance, float rate)
		{
			if (utterance == null)
			{
				UnityEngine.Debug.LogError("Utterance was not set!");
				return;
			}
			string command = string.Format("SpeechSynthesisSetRate?utterance={0}&rate={1}", utterance._mReference, rate);
			AddCommand(command);
		}

		public void SetText(SpeechSynthesisUtterance utterance, string text)
		{
			if (utterance == null)
			{
				UnityEngine.Debug.LogError("Utterance was not set!");
				return;
			}
			string arg = string.Empty;
			if (!string.IsNullOrEmpty(text))
			{
				byte[] bytes = Encoding.UTF8.GetBytes(text);
				arg = Convert.ToBase64String(bytes);
			}
			string command = string.Format("SpeechSynthesisSetText?utterance={0}&text={1}", utterance._mReference, arg);
			AddCommand(command);
		}

		public void Speak(SpeechSynthesisUtterance utterance)
		{
			if (utterance == null)
			{
				UnityEngine.Debug.LogError("Utterance was not set!");
				return;
			}
			string command = string.Format("SpeechSynthesisSpeak?utterance={0}", utterance._mReference);
			AddCommand(command);
		}

		public void Cancel()
		{
			AddCommand("SpeechSynthesisCancel");
		}

		public void SetVoice(SpeechSynthesisUtterance utterance, Voice voice)
		{
			if (utterance == null)
			{
				UnityEngine.Debug.LogError("Utterance was not set!");
				return;
			}
			string arg = string.Empty;
			string voiceURI = voice.voiceURI;
			if (!string.IsNullOrEmpty(voiceURI))
			{
				byte[] bytes = Encoding.UTF8.GetBytes(voiceURI);
				arg = Convert.ToBase64String(bytes);
			}
			string command = string.Format("SpeechSynthesisSetVoice?utterance={0}&voice={1}", utterance._mReference, arg);
			AddCommand(command);
		}

		public void ManagementCloseBrowserTab()
		{
			AddCommand("CloseBrowserTab");
		}

		public void ManagementCloseProxy()
		{
			AddCommand("CloseProxy");
			_mPendingCommands.Add("ClearPendingCommands");
		}

		private static string GetAppDataFolder()
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CHROME_SPEECH_PROXY");
		}

		private static string GetAppConfig()
		{
			return Path.Combine(GetAppDataFolder(), "app.config");
		}

		private static void SetupAppDataFolder()
		{
			string appDataFolder = GetAppDataFolder();
			try
			{
				if (!Directory.Exists(appDataFolder))
				{
					Directory.CreateDirectory(appDataFolder);
				}
			}
			catch (Exception)
			{
			}
		}

		private static SpeechProxyConfig GetSpeechProxyConfig()
		{
			SetupAppDataFolder();
			string appConfig = GetAppConfig();
			SpeechProxyConfig speechProxyConfig = null;
			try
			{
				if (File.Exists(appConfig))
				{
					using (FileStream stream = File.Open(appConfig, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						using (StreamReader streamReader = new StreamReader(stream))
						{
							string json = streamReader.ReadToEnd();
							speechProxyConfig = JsonUtility.FromJson<SpeechProxyConfig>(json);
						}
					}
				}
			}
			catch (Exception)
			{
			}
			if (speechProxyConfig == null)
			{
				speechProxyConfig = new SpeechProxyConfig();
			}
			return speechProxyConfig;
		}

		public void ManagementLaunchProxy()
		{
			_mPendingCommands.Insert(0, "ClearPendingCommands");
			string appDataFolder = GetAppDataFolder();
			if (!Directory.Exists(appDataFolder))
			{
				UnityEngine.Debug.LogError("The Speech Proxy needs to run once to set the install path!");
				return;
			}
			SpeechProxyConfig speechProxyConfig = GetSpeechProxyConfig();
			try
			{
				Process process = new Process();
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat("/c start \"\" \"{0}\\{1}\" {2}", speechProxyConfig.installDirectory, speechProxyConfig.appName, _mPort);
				process.StartInfo = new ProcessStartInfo("C:\\Windows\\System32\\cmd.exe", stringBuilder.ToString());
				process.StartInfo.WorkingDirectory = speechProxyConfig.installDirectory;
				process.Exited += delegate
				{
					process.Dispose();
				};
				process.Start();
			}
			catch (Exception)
			{
			}
		}

		public void ManagementOpenBrowserTab()
		{
			AddCommand("OpenBrowserTab");
		}

		public void ManagementSetProxyPort(int port)
		{
			_mNextPort = port;
			string command = string.Format("SetProxyPort?port={0}", port);
			AddCommand(command);
			AddCommand("SetProxyPort");
		}

		protected virtual IWWW CreateWWW(string url)
		{
			return new WWWPlayMode(url);
		}
	}
}
