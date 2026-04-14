using System.Collections.Generic;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScpInterfaces : MonoBehaviour
{
	public GameObject Scp106_eq;

	public TextMeshProUGUI Scp106_ability_highlight;

	public Text Scp106_ability_points;

	public GameObject Scp049_eq;

	public Image Scp049_loading;

	public TextMeshProUGUI remainingTargets;

	public static int remTargs;

	private void Start()
	{
		Timing.RunCoroutine(_UpdateTargets());
	}

	private IEnumerator<float> _UpdateTargets()
	{
		while (PlayerManager.localPlayer == null)
		{
			yield return 0f;
		}
		CharacterClassManager myCCM = PlayerManager.localPlayer.GetComponent<CharacterClassManager>();
		while (this != null)
		{
			while (!myCCM.IsScpButNotZombie())
			{
				remainingTargets.text = string.Empty;
				yield return 0f;
			}
			int targets = 0;
			GameObject[] players = PlayerManager.singleton.players;
			foreach (GameObject item in players)
			{
				if (item != null && item.GetComponent<CharacterClassManager>().IsTargetForSCPs())
				{
					targets++;
					yield return 0f;
					if (!myCCM.IsScpButNotZombie())
					{
						break;
					}
				}
			}
			remTargs = targets;
			if (myCCM.curClass != 7)
			{
				remainingTargets.text = "Remaining targets: " + targets;
			}
			else
			{
				remainingTargets.text = string.Empty;
			}
			yield return 0f;
		}
	}

	private GameObject FindLocalPlayer()
	{
		return PlayerManager.localPlayer;
	}

	public void CreatePortal()
	{
		FindLocalPlayer().GetComponent<Scp106PlayerScript>().CreatePortalInCurrentPosition();
	}

	public void Update106Highlight(int id)
	{
		FindLocalPlayer().GetComponent<Scp106PlayerScript>().highlightID = id;
	}

	public void Use106Portal()
	{
		FindLocalPlayer().GetComponent<Scp106PlayerScript>().UseTeleport();
	}
}
