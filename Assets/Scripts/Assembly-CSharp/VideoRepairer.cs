using UnityEngine;
using UnityEngine.Video;

public class VideoRepairer : MonoBehaviour
{
	private void Start()
	{
		Invoke("Repair", 5f);
	}

	private void Repair()
	{
		GetComponent<VideoPlayer>().targetMaterialProperty = "_EmissionMap";
	}
}
