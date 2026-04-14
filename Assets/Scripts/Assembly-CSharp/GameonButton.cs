using GameConsole;
using UnityEngine;

public class GameonButton : MonoBehaviour
{
	[SerializeField]
	private GameonMenu gameonMenu;

	[SerializeField]
	private MainMenuScript mainMenu;

	private void Start()
	{
		if (gameonMenu == null)
		{
			gameonMenu = (GameonMenu)Object.FindObjectOfType(typeof(GameonMenu));
			if (gameonMenu == null)
			{
				Console.singleton.AddLog("[Amazon GameOn] GameOn Menu not found.", Color.red);
			}
		}
		if (mainMenu == null)
		{
			mainMenu = (MainMenuScript)Object.FindObjectOfType(typeof(MainMenuScript));
			if (mainMenu == null)
			{
				Console.singleton.AddLog("[Amazon GameOn] Main Menu not found.", Color.red);
			}
		}
	}

	public void ShowMenu()
	{
		if (!SteamManager.Running)
		{
			gameonMenu.ShowError("Failed to initialize Steam connection. You must be signed into Steam in order to access Amazon GameOn functionality.", string.Empty);
			return;
		}
		mainMenu.ChangeMenu(9);
		gameonMenu.CreateClient();
		Debug.Log("Created Client");
	}

	public void ShowRegisterMenu()
	{
		gameonMenu.SwitchMenu(GameonMenu.GameonMenuType.VALIDATE);
	}

	public void ShowTournaments(bool onlyEntered = false)
	{
		gameonMenu.ShowTournaments(onlyEntered);
	}

	private void Authenticate()
	{
		string personaName = SteamManager.GetPersonaName(0uL);
	}

	public void HideError()
	{
		gameonMenu.HideError();
	}

	public void GoBack()
	{
		gameonMenu.GoBack();
	}

	public void ValidateKey()
	{
		gameonMenu.ValidateKey();
	}
}
