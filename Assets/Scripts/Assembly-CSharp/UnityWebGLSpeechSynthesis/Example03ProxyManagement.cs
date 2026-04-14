using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UnityWebGLSpeechSynthesis
{
	public class Example03ProxyManagement : MonoBehaviour
	{
		private ISpeechSynthesisPlugin _mSpeechSynthesisPlugin;

		public Button _mButtonCloseBrowserTab;

		public Button _mButtonCloseProxy;

		public Button _mButtonLaunchProxy;

		public Button _mButtonOpenBrowserTab;

		public Button _mButtonSetProxyPort;

		public InputField _mInputPort;

		private IEnumerator Start()
		{
			_mSpeechSynthesisPlugin = ProxySpeechSynthesisPlugin.GetInstance();
			if (_mSpeechSynthesisPlugin == null)
			{
				Debug.LogError("Proxy Speech Detection Plugin is not set!");
				yield break;
			}
			if ((bool)_mButtonLaunchProxy)
			{
				_mButtonLaunchProxy.onClick.AddListener(delegate
				{
					_mSpeechSynthesisPlugin.ManagementLaunchProxy();
				});
			}
			while (!_mSpeechSynthesisPlugin.IsAvailable())
			{
				yield return null;
			}
			if ((bool)_mButtonCloseBrowserTab)
			{
				_mButtonCloseBrowserTab.onClick.AddListener(delegate
				{
					_mSpeechSynthesisPlugin.ManagementCloseBrowserTab();
				});
			}
			if ((bool)_mButtonCloseProxy)
			{
				_mButtonCloseProxy.onClick.AddListener(delegate
				{
					_mSpeechSynthesisPlugin.ManagementCloseProxy();
				});
			}
			if ((bool)_mButtonOpenBrowserTab)
			{
				_mButtonOpenBrowserTab.onClick.AddListener(delegate
				{
					_mSpeechSynthesisPlugin.ManagementOpenBrowserTab();
				});
			}
			if (!_mButtonSetProxyPort || !_mInputPort)
			{
				yield break;
			}
			_mButtonSetProxyPort.onClick.AddListener(delegate
			{
				int result;
				if (int.TryParse(_mInputPort.text, out result))
				{
					_mSpeechSynthesisPlugin.ManagementSetProxyPort(result);
				}
				else
				{
					_mInputPort.text = "83";
				}
			});
		}
	}
}
