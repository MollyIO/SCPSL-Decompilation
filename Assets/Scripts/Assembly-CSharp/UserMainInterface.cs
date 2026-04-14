using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UserMainInterface : MonoBehaviour
{
	public Slider sliderHP;

	public Slider searchProgress;

	public Text textHP;

	public Text specatorInfo;

	public Text playerlistText;

	public Text voiceInfo;

	public GameObject hpOBJ;

	public GameObject searchOBJ;

	public GameObject overloadMsg;

	public GameObject summary;

	public Image dimSph;

	[Space]
	public Text fps;

	public static UserMainInterface singleton;

	public float lerpSpeed = 3f;

	public float lerpedHP;

	private void Awake()
	{
		singleton = this;
	}

	private void Start()
	{
		playerlistText.text = "PRESS<b> " + NewInput.GetKey("Player List").ToString() + " </b>TO OPEN THE PLAYER LIST";
		voiceInfo.text = NewInput.GetKey("Voice Chat").ToString();
		ResolutionManager.RefreshScreen();
	}

	public void SearchProgress(float curProgress, float targetProgress)
	{
		searchProgress.maxValue = targetProgress;
		searchProgress.value = curProgress;
		searchOBJ.SetActive(curProgress != 0f);
	}

	public void SetHP(int _hp, int _maxhp)
	{
		float num = _maxhp;
		lerpedHP = Mathf.Lerp(lerpedHP, _hp, Time.deltaTime * lerpSpeed);
		sliderHP.value = lerpedHP;
		textHP.text = Mathf.Max(Mathf.Round(sliderHP.value / num * 100f), 1f) + "%";
		sliderHP.maxValue = num;
	}

	private void Update()
	{
		try
		{
			fps.text = NetworkManager.singleton.client.GetRTT() + " ms";
		}
		catch
		{
		}
	}
}
