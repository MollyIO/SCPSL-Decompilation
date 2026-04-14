using UnityEngine;
using UnityEngine.UI;

public class TournamentButton : MonoBehaviour
{
	private static GameonMenu gameonMenu;

	[SerializeField]
	private GameObject lockIcon;

	public Text Title;

	public Text Subtitle;

	public int ButtonId { get; set; }

	private void Start()
	{
		if (gameonMenu == null)
		{
			Debug.Log("Menu is null");
			gameonMenu = (GameonMenu)Object.FindObjectOfType(typeof(GameonMenu));
			if (gameonMenu == null)
			{
				Debug.Log("[Amazon GameOn] GameOn Menu not found.");
			}
		}
	}

	public void ChangeBorderColor()
	{
		GetComponent<Image>().color = Color.yellow;
	}

	public void ShowLockIcon()
	{
		lockIcon.SetActive(true);
	}

	public void Join()
	{
		Debug.Log("Join Button Pressed");
		gameonMenu.JoinTournament(ButtonId, string.Empty);
	}

	public void MoreInfo()
	{
		gameonMenu.ShowTournamentInfo(ButtonId);
	}
}
