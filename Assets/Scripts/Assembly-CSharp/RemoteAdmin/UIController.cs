using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using GameConsole;
using MEC;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteAdmin
{
	public class UIController : MonoBehaviour
	{
		public GameObject root_login;

		public GameObject root_panel;

		public GameObject root_tbra;

		public GameObject root_root;

		public Texture wrongPasswordTexture;

		public Button confirmButton;

		public InputField passwordField;

		public bool loggedIn;

		public bool opened;

		public int awaitingLogin;

		public bool textBasedVersion;

		public static UIController singleton;

		private void Awake()
		{
			singleton = this;
		}

		private void Update()
		{
			if (Input.GetKeyDown(NewInput.GetKey("Remote Admin")) || (opened && Input.GetKeyDown(KeyCode.Escape) && !LargeDataPrinter.Singleton.Panel.activeSelf && !Console.singleton.console.activeSelf))
			{
				ChangeConsoleStage();
			}
		}

		public bool IsAnyInputFieldFocused()
		{
			return GetComponentsInChildren<InputField>().Any((InputField item) => item.isFocused);
		}

		public void ChangeConsoleStage()
		{
			opened = !opened;
			QueryProcessor.StaticRefreshPlayerList();
			RefreshStatus();
		}

		public void CallSendPassword()
		{
			Timing.RunCoroutine(_SendPassword(), Segment.FixedUpdate);
		}

		public void ChangeTextMode(bool b)
		{
			textBasedVersion = b;
			RefreshStatus();
		}

		public void RefreshStatus()
		{
			if (IsAnyInputFieldFocused())
			{
				opened = true;
			}
			CursorManager.singleton.raOp = opened;
			root_panel.SetActive(opened && loggedIn && !textBasedVersion);
			root_tbra.SetActive(opened && loggedIn && textBasedVersion);
			root_login.SetActive(opened && !loggedIn);
			root_root.SetActive(opened);
			FirstPersonController.usingRemoteAdmin = opened;
		}

		public void ActivateRemoteAdmin()
		{
			loggedIn = true;
			RefreshStatus();
		}

		public void DeactivateRemoteAdmin()
		{
			loggedIn = false;
			RefreshStatus();
		}

		private IEnumerator<float> _SendPassword()
		{
			QueryProcessor queryProc = PlayerManager.localPlayer.GetComponent<QueryProcessor>();
			if (!queryProc.OverridePasswordEnabled)
			{
				Console.singleton.AddLog("Password authentication is disabled on this server!", Color.magenta);
			}
			else
			{
				if (awaitingLogin == 1)
				{
					yield break;
				}
				confirmButton.interactable = false;
				float t = 0f;
				bool gen = false;
				if (queryProc.ClientSalt == null)
				{
					byte[] array;
					using (RandomNumberGenerator randomNumberGenerator = new RNGCryptoServiceProvider())
					{
						array = new byte[32];
						randomNumberGenerator.GetBytes(array);
					}
					queryProc.ClientSalt = array;
					gen = true;
				}
				if (queryProc.Salt == null || gen)
				{
					queryProc.CallCmdRequestSalt(queryProc.ClientSalt);
				}
				while (t < 20f)
				{
					t += Time.fixedDeltaTime;
					yield return 0f;
					if (queryProc.Salt != null)
					{
						break;
					}
				}
				if (queryProc.Salt == null)
				{
					Console.singleton.AddLog("Can't obtain salt from server!", Color.magenta);
					yield break;
				}
				queryProc.Key = QueryProcessor.DerivePassword(passwordField.text, queryProc.Salt, queryProc.ClientSalt);
				queryProc.CallCmdSendPassword(queryProc.HmacSign("Login", -1));
				Console.singleton.AddLog("Sent auth request to the server!", Color.blue);
				awaitingLogin = 1;
				while (awaitingLogin == 1 && t < 5f)
				{
					t += Time.fixedDeltaTime;
					yield return 0f;
				}
				if (awaitingLogin == 2)
				{
					queryProc.PasswordSent = true;
					ActivateRemoteAdmin();
				}
				else
				{
					passwordField.GetComponent<RawImage>().texture = wrongPasswordTexture;
				}
				confirmButton.interactable = true;
				awaitingLogin = 0;
			}
		}
	}
}
