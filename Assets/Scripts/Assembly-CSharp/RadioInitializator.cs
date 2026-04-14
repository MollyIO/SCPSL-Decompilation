using System.Collections.Generic;
using Dissonance;
using Dissonance.Audio.Playback;
using Dissonance.Integrations.UNet_HLAPI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RadioInitializator : NetworkBehaviour
{
	private class VoiceIndicator
	{
		public GameObject indicator;

		public string id;

		public VoiceIndicator(GameObject indicator, string id)
		{
			this.indicator = indicator;
			this.id = id;
		}
	}

	private class VoiceIndicatorManager
	{
		private List<VoiceIndicator> voices = new List<VoiceIndicator>();

		public bool ContainsId(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return false;
			}
			foreach (VoiceIndicator voice in voices)
			{
				if (((voice != null) ? voice.id : null) != null && voice.id == id)
				{
					return true;
				}
			}
			return false;
		}

		public VoiceIndicator GetFromId(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return null;
			}
			foreach (VoiceIndicator voice in voices)
			{
				if (((voice != null) ? voice.id : null) != null && voice.id == id)
				{
					return voice;
				}
			}
			return null;
		}

		public void RemoveId(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return;
			}
			foreach (VoiceIndicator voice in voices)
			{
				if (voice.id == id)
				{
					Remove(voice);
				}
			}
		}

		public void Add(VoiceIndicator voiceObject)
		{
			if (voiceObject != null)
			{
				voices.Add(voiceObject);
			}
		}

		private void Remove(VoiceIndicator voiceObject)
		{
			if (voiceObject != null)
			{
				if (voiceObject.indicator != null)
				{
					Object.Destroy(voiceObject.indicator);
				}
				voices.Remove(voiceObject);
			}
		}
	}

	private PlayerManager pm;

	public ServerRoles serverRoles;

	public Radio radio;

	public HlapiPlayer hlapiPlayer;

	public NicknameSync nicknameSync;

	private Transform parent;

	public GameObject prefab;

	private VoiceBroadcastTrigger bct;

	private static VoiceIndicatorManager voiceIndicators = new VoiceIndicatorManager();

	public AnimationCurve noiseOverLoudness;

	public float curAmplitude;

	public float multipl = 3f;

	private void Start()
	{
		bct = Object.FindObjectOfType<VoiceBroadcastTrigger>();
		pm = PlayerManager.singleton;
		try
		{
			parent = GameObject.Find("VoicechatPopups").transform;
		}
		catch
		{
		}
	}

	private void LateUpdate()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		GameObject[] players = pm.players;
		foreach (GameObject gameObject in players)
		{
			try
			{
				if (!(gameObject != base.gameObject))
				{
					continue;
				}
				RadioInitializator component = gameObject.GetComponent<RadioInitializator>();
				component.radio.SetRelationship();
				string playerId = component.hlapiPlayer.PlayerId;
				if (!(component.radio.mySource != null))
				{
					continue;
				}
				VoicePlayback component2 = component.radio.mySource.GetComponent<VoicePlayback>();
				bool flag = component.radio.mySource.spatialBlend == 0f && component2.Priority != ChannelPriority.None && (component.radio.ShouldBeVisible(base.gameObject) || Intercom.host.speaker == gameObject);
				curAmplitude = component2.Amplitude * multipl;
				if (NetworkServer.active)
				{
					gameObject.GetComponent<Scp939_VisionController>().MakeNoise(noiseOverLoudness.Evaluate(curAmplitude));
				}
				if (voiceIndicators.ContainsId(playerId))
				{
					if (!flag)
					{
						voiceIndicators.RemoveId(playerId);
						continue;
					}
					VoiceIndicator fromId = voiceIndicators.GetFromId(playerId);
					if (fromId != null && fromId.indicator != null)
					{
						fromId.indicator.GetComponent<Image>().color = component.serverRoles.GetGradient()[0].Evaluate(component2.Amplitude * 3f);
						fromId.indicator.GetComponent<Outline>().effectColor = component.serverRoles.GetGradient()[1].Evaluate(component2.Amplitude * 3f);
					}
				}
				else if (flag)
				{
					GameObject gameObject2 = Object.Instantiate(prefab, parent);
					gameObject2.transform.localScale = Vector3.one;
					gameObject2.GetComponentInChildren<Text>().text = component.nicknameSync.myNick;
					voiceIndicators.Add(new VoiceIndicator(gameObject2, playerId));
				}
			}
			catch
			{
			}
		}
	}

	private void OnDestroy()
	{
		try
		{
			voiceIndicators.RemoveId(GetComponent<HlapiPlayer>().PlayerId);
		}
		catch
		{
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
