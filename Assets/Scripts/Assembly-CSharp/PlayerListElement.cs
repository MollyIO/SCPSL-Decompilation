using Dissonance;
using Dissonance.Integrations.UNet_HLAPI;
using RemoteAdmin;
using TMPro;
using UnityEngine;

public class PlayerListElement : MonoBehaviour
{
	public GameObject instance;

	public void Mute(bool b)
	{
		CharacterClassManager component = instance.GetComponent<CharacterClassManager>();
		if (component.isLocalPlayer)
		{
			return;
		}
		if (!string.IsNullOrEmpty(component.SteamId) && !component.Muted)
		{
			if (b)
			{
				MuteHandler.IssuePersistantMute(component.SteamId);
			}
			else
			{
				MuteHandler.RevokePersistantMute(component.SteamId);
			}
		}
		Object.FindObjectOfType<DissonanceComms>().FindPlayer(instance.GetComponent<HlapiPlayer>().PlayerId).IsLocallyMuted = b;
	}

	public void OpenSteamAccount()
	{
		string steamId = instance.GetComponent<CharacterClassManager>().SteamId;
		if (!string.IsNullOrEmpty(steamId))
		{
			SteamManager.OpenProfile(ulong.Parse(steamId));
		}
	}

	public void Report()
	{
		if (!instance.GetComponent<CharacterClassManager>().CheatReported)
		{
			return;
		}
		PlayerList componentInParent = GetComponentInParent<PlayerList>();
		TextMeshProUGUI[] componentsInChildren = componentInParent.reportForm.GetComponentsInChildren<TextMeshProUGUI>();
		foreach (TextMeshProUGUI textMeshProUGUI in componentsInChildren)
		{
			if (textMeshProUGUI.name == "Player Name")
			{
				textMeshProUGUI.text = instance.GetComponent<NicknameSync>().myNick;
			}
			if (textMeshProUGUI.name == "Player ID")
			{
				textMeshProUGUI.text = instance.GetComponent<QueryProcessor>().PlayerId.ToString();
			}
		}
		PlayerManager.localPlayer.GetComponent<FirstPersonController>().isPaused = true;
		componentInParent.reportForm.SetActive(true);
		componentInParent.panel.SetActive(false);
	}
}
