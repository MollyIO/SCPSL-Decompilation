using TMPro;
using UnityEngine;

public class SpectatorInterface : MonoBehaviour
{
	public GameObject rootPanel;

	public TextMeshProUGUI playerList;

	public TextMeshProUGUI playerInfo;

	public static SpectatorInterface singleton;

	private void Awake()
	{
		singleton = this;
	}
}
