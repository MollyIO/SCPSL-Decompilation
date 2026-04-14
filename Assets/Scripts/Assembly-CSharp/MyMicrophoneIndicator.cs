using Dissonance;
using Dissonance.Integrations.UNet_HLAPI;
using UnityEngine;
using UnityEngine.UI;

public class MyMicrophoneIndicator : MonoBehaviour
{
	public VoiceBroadcastTrigger dissonanceSetup;

	public Image background;

	public Image volume;

	public float amplitudeFactor = 3.4f;

	public float increaseLerp = 5f;

	public float decreaseLerp = 2f;

	public static float curVolume;

	public static bool isTransmitting;

	private VoicePlayerState lplaystate;

	private bool isSupported;

	public void SetColor(Color classColor)
	{
		Image image = background;
		Color color = new Color(classColor.r, classColor.g, classColor.b, 0.4f);
		volume.color = color;
		image.color = color;
	}

	private void Start()
	{
		isSupported = NonFacilityCompatibility.currentSceneSettings.voiceChatSupport != NonFacilityCompatibility.SceneDescription.VoiceChatSupportMode.Unsupported;
	}

	private void Update()
	{
		if (isSupported)
		{
			if (lplaystate == null && PlayerManager.localPlayer != null)
			{
				lplaystate = dissonanceSetup.GetComponent<DissonanceComms>().FindPlayer(PlayerManager.localPlayer.GetComponent<HlapiPlayer>().PlayerId);
			}
			if (lplaystate != null)
			{
				curVolume = lplaystate.Amplitude;
			}
			bool flag = dissonanceSetup.IsTransmitting;
			background.enabled = flag;
			flag = flag;
			volume.enabled = flag;
			isTransmitting = flag;
			float num = curVolume * amplitudeFactor;
			float fillAmount = volume.fillAmount;
			volume.fillAmount = Mathf.Lerp(fillAmount, num, Time.deltaTime * ((!(fillAmount < num)) ? decreaseLerp : increaseLerp));
		}
	}
}
