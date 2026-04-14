using TMPro;
using UnityEngine;

public class PrivateBetaWarning : MonoBehaviour
{
	public TextMeshProUGUI text;

	public string contentPrivateBeta;

	public string streamingAllowedSuffix;

	public string streamingDisallowedSuffix;

	public string doNotShareSuffix;

	private void Start()
	{
		if (CustomNetworkManager.isPrivateBeta)
		{
			text.text = contentPrivateBeta + "\n" + ((!CustomNetworkManager.isStreamingAllowed) ? streamingDisallowedSuffix : streamingAllowedSuffix) + "\n" + doNotShareSuffix;
		}
	}
}
