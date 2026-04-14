using System;
using System.Collections.Generic;
using System.Linq;
using GameConsole;
using SCPSL.GameOn;
using SCPSL.GameOn.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameonMenu : MonoBehaviour
{
	public enum GameonMenuType
	{
		DEFAULT = 0,
		VALIDATE = 1,
		TOURNAMENTLIST = 2
	}

	[SerializeField]
	private GameObject errorWindow;

	[SerializeField]
	private Text errorText;

	[SerializeField]
	private MainMenuScript mainMenu;

	[SerializeField]
	private TMP_InputField inputField;

	[SerializeField]
	private GameObject[] menus;

	[SerializeField]
	private GameObject tournamentListUi;

	[SerializeField]
	private GameObject tournamentInfo;

	[SerializeField]
	private GameObject tournamentFilter;

	[SerializeField]
	private GameObject passwordPopup;

	[SerializeField]
	private GameObject passwordEntry;

	[SerializeField]
	private GameObject tournamentListPlatformSlider;

	[SerializeField]
	private GameObject tournamentListPasswordSlider;

	[SerializeField]
	private GameObject tournamentListDropdownUI;

	[SerializeField]
	private RectTransform contentParent;

	[SerializeField]
	private RectTransform element;

	private List<GameObject> spawns = new List<GameObject>();

	private PlayerTournament[] tournamentList;

	private bool isShowingAdditionalInfo;

	private bool isShowingPasswordPopup;

	private int selectedTourneyID = -1;

	private bool showingOnlyEntered;

	public GameonMenuType CurrentMenuType { get; private set; }

	public Client PlayerClientInstance { get; private set; }

	private void Awake()
	{
		SwitchMenu(GameonMenuType.DEFAULT);
		Debug.Log("Called");
	}

	private void Update()
	{
	}

	public void CreateClient()
	{
		Client.networkSim = false;
		string text = SteamManager.SteamId64.ToString();
		string personaName = SteamManager.GetPersonaName(0uL);
		PlayerClientInstance = new Client(text, FileManager.GetAppFolder(ServerStatic.ShareNonConfigs) + "GameOn_" + text + ".txt", personaName);
		if ((!PlayerClientInstance.IsRegistered && !RegisterPlayer()) || !PlayerClientInstance.IsRegistered)
		{
			return;
		}
		if (!PlayerClientInstance.IsAuthenticated)
		{
			if (AuthenticatePlayer())
			{
			}
		}
		else
		{
			string message = "Succesfully connected to existing GameOn connection.";
			Debug.Log(message);
		}
	}

	private bool RegisterPlayer()
	{
		try
		{
			PlayerClientInstance.RegisterPlayer();
			string message = "Succesfully created and registered new GameOn authentication.";
			Debug.Log(message);
			return true;
		}
		catch (Exception ex)
		{
			string message = "Could not register you to the Amazon GameOn API. This is likely a problem on our end.";
			ShowError(message, ex.Message);
			return false;
		}
	}

	private bool AuthenticatePlayer()
	{
		try
		{
			DeviceOSType deviceOSType = ((SystemInfo.operatingSystemFamily != OperatingSystemFamily.Linux) ? DeviceOSType.Pc : DeviceOSType.Linux);
			PlayerClientInstance.AuthenticatePlayer(AppBuildType.Development, deviceOSType);
			string message = "Succesfully created new GameOn authentication.";
			Debug.Log(message);
			return true;
		}
		catch (Exception ex)
		{
			string message = "Could not Authenticate you to the Amazon GameOn API. This is likely a problem on our end.";
			ShowError(message, ex.Message);
			return false;
		}
	}

	public void ShowError(string message = "", string exception = "")
	{
		errorWindow.SetActive(true);
		if (string.IsNullOrEmpty(message))
		{
			errorText.text = "Unidentified error.";
			return;
		}
		Debug.Log(message);
		errorText.text = message;
		if (GameConsole.Console.singleton != null)
		{
			GameConsole.Console.singleton.AddLog(message, Color.red);
		}
		if (!string.IsNullOrEmpty(exception))
		{
			Debug.Log(exception);
			if (GameConsole.Console.singleton != null)
			{
				GameConsole.Console.singleton.AddLog(exception, Color.red);
			}
		}
	}

	public void HideError()
	{
		errorWindow.SetActive(false);
	}

	public void SwitchMenu(GameonMenuType menuType)
	{
		for (int i = 0; i < menus.Length; i++)
		{
			menus[i].SetActive(i == (int)menuType);
			CurrentMenuType = menuType;
		}
		if (menuType == GameonMenuType.TOURNAMENTLIST && PlayerClientInstance.IsRegistered)
		{
			InitializeTournamentList();
		}
	}

	public void ValidateKey()
	{
		string text = inputField.text;
		text = text.Trim();
		if (IsSessionExpired())
		{
			return;
		}
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				PlayerClientInstance.ValidateStreamAccountCode(text);
			}
			catch (Exception ex)
			{
				ShowError("Failed to authenticate your key. Make sure you didn't input it incorrectly.", ex.Message);
				return;
			}
			SwitchMenu(GameonMenuType.DEFAULT);
		}
		else
		{
			ShowError("You have to input a key. Click the button above to find it.", string.Empty);
		}
	}

	private bool IsSessionExpired()
	{
		if (PlayerClientInstance.IsSessionExpired)
		{
			if (!AuthenticatePlayer())
			{
				ShowError("Session has expired, reopen the GameOn menu and it should initialize a new session.", string.Empty);
				mainMenu.ChangeMenu(0);
				return true;
			}
			return false;
		}
		return false;
	}

	private void InitializeTournamentList()
	{
		if (!IsSessionExpired())
		{
			Debug.Log("List");
			RefreshTourneyList();
		}
	}

	private GameObject AddRecord()
	{
		RectTransform rectTransform = UnityEngine.Object.Instantiate(element);
		rectTransform.SetParent(contentParent);
		rectTransform.localScale = Vector3.one;
		rectTransform.localPosition = Vector3.zero;
		spawns.Add(rectTransform.gameObject);
		contentParent.sizeDelta = Vector2.up * 150f * spawns.Count;
		return rectTransform.gameObject;
	}

	public void RefreshTourneyList()
	{
		Dropdown component = this.tournamentFilter.GetComponent<Dropdown>();
		TournamentFilter tournamentFilter = StringToTourneyFilter(component.options[component.value].text);
		StreamingPlatform streamingPlatform = ((tournamentListPlatformSlider.GetComponent<Slider>().value != 0f) ? StreamingPlatform.Twitch : null);
		Debug.Log((!(tournamentFilter != null)) ? "Null filter" : tournamentFilter.ToString());
		if (streamingPlatform == null)
		{
			List<PlayerTournament> list = new List<PlayerTournament>();
			list.AddRange(PlayerClientInstance.GetPlayerTournaments(tournamentFilter));
			Client playerClientInstance = PlayerClientInstance;
			TournamentFilter filterBy = tournamentFilter;
			StreamingPlatform twitch = StreamingPlatform.Twitch;
			list.AddRange(playerClientInstance.GetPlayerTournaments(filterBy, -1, null, null, twitch));
			tournamentList = list.ToArray();
		}
		else
		{
			Client playerClientInstance2 = PlayerClientInstance;
			TournamentFilter filterBy = tournamentFilter;
			StreamingPlatform twitch = streamingPlatform;
			tournamentList = playerClientInstance2.GetPlayerTournaments(filterBy, -1, null, null, twitch);
		}
		if (showingOnlyEntered)
		{
			tournamentList = tournamentList.Where((PlayerTournament tournament) => tournament.HasEntered).ToArray();
		}
		else if (tournamentListPasswordSlider.GetComponent<Slider>().value == 1f)
		{
			tournamentList = tournamentList.Where((PlayerTournament tournament) => !tournament.HasAccessKey).ToArray();
		}
		foreach (GameObject spawn in spawns)
		{
			UnityEngine.Object.Destroy(spawn);
		}
		spawns.Clear();
		for (int num = 0; num < tournamentList.Length; num++)
		{
			TournamentButton component2 = AddRecord().GetComponent<TournamentButton>();
			component2.ButtonId = num;
			string creatorPlayerName = tournamentList[num].CreatorPlayerName;
			string title = tournamentList[num].Title;
			string subtitle = tournamentList[num].Subtitle;
			string empty = string.Empty;
			empty = ((!string.IsNullOrEmpty(creatorPlayerName)) ? ((!string.IsNullOrEmpty(subtitle)) ? (subtitle + " - Made By: " + creatorPlayerName) : ("Made By: " + creatorPlayerName)) : ((!string.IsNullOrEmpty(subtitle)) ? subtitle : string.Empty));
			component2.Subtitle.text = empty;
			component2.Title.text = tournamentList[num].Title;
			component2.ChangeBorderColor();
			if (tournamentList[num].HasAccessKey && !showingOnlyEntered)
			{
				component2.ShowLockIcon();
			}
		}
	}

	public void JoinTournament(int buttonId, string password = "")
	{
		Debug.Log("Joining tourney");
		if (tournamentList == null)
		{
			ShowError("Tournamentlist not initialized.", string.Empty);
		}
		else if (buttonId < 0)
		{
			ShowError("Attempted to join tournament with negative id", string.Empty);
		}
		else if (buttonId >= tournamentList.Length)
		{
			ShowError("Attempted to join tournament with an id outside the list's range", string.Empty);
		}
		else
		{
			if (IsSessionExpired())
			{
				return;
			}
			PlayerTournament playerTournament = tournamentList[buttonId];
			try
			{
				if (playerTournament.HasAccessKey)
				{
					if (string.IsNullOrEmpty(password))
					{
						selectedTourneyID = buttonId;
						ShowPasswordPopup();
						return;
					}
					PlayerClientInstance.EnterPlayerTournament(playerTournament, password);
				}
				PlayerClientInstance.EnterPlayerTournament(playerTournament);
			}
			catch (Exception)
			{
				ShowError("Failed to join tournament.", string.Empty);
			}
			Debug.Log("Joined tourney");
		}
	}

	public void ShowTournaments(bool onlyEntered = false)
	{
		showingOnlyEntered = onlyEntered;
		SwitchMenu(GameonMenuType.TOURNAMENTLIST);
	}

	public void ShowTournamentInfo(int buttonId)
	{
		isShowingAdditionalInfo = true;
		tournamentInfo.SetActive(isShowingAdditionalInfo);
		tournamentListUi.SetActive(!isShowingAdditionalInfo);
		if (tournamentList == null)
		{
			ShowError("Tournamentlist not initialized.", string.Empty);
		}
		else if (buttonId < 0)
		{
			ShowError("Attempted to show tournament with negative id", string.Empty);
		}
		else if (buttonId >= tournamentList.Length)
		{
			ShowError("Attempted to show tournament with an id outside the list's range", string.Empty);
		}
		else if (!IsSessionExpired())
		{
			tournamentInfo.GetComponent<TournamentInfo>().SetInfo(tournamentList[buttonId]);
		}
	}

	public void HideTournamentInfo()
	{
		isShowingAdditionalInfo = false;
		tournamentInfo.SetActive(isShowingAdditionalInfo);
		tournamentListUi.SetActive(!isShowingAdditionalInfo);
	}

	public void GoBack()
	{
		switch (CurrentMenuType)
		{
		case GameonMenuType.DEFAULT:
			mainMenu.ChangeMenu(0);
			break;
		case GameonMenuType.VALIDATE:
			SwitchMenu(GameonMenuType.DEFAULT);
			break;
		case GameonMenuType.TOURNAMENTLIST:
			if (isShowingAdditionalInfo)
			{
				isShowingAdditionalInfo = false;
			}
			else if (isShowingPasswordPopup)
			{
				isShowingPasswordPopup = false;
			}
			SwitchMenu(GameonMenuType.DEFAULT);
			break;
		default:
			mainMenu.ChangeMenu(0);
			break;
		}
	}

	private TournamentFilter StringToTourneyFilter(string input)
	{
		return (TournamentFilter)StringEnum.FromString(input, typeof(TournamentFilter));
	}

	public void SubmitPassword()
	{
		HidePasswordPopup();
		if (selectedTourneyID == -1)
		{
			Debug.Log("Tournament wasn't selected before attempting to submit password");
			ShowError("Tournament wasn't selected before attempting to submit password", string.Empty);
		}
		else
		{
			JoinTournament(selectedTourneyID, passwordEntry.GetComponent<TMP_InputField>().text.Trim());
		}
	}

	public void ShowPasswordPopup()
	{
		isShowingPasswordPopup = true;
		tournamentListUi.SetActive(false);
		passwordPopup.SetActive(true);
	}

	public void HidePasswordPopup()
	{
		isShowingPasswordPopup = false;
		tournamentListUi.SetActive(true);
		passwordPopup.SetActive(false);
	}

	public void TestScore()
	{
		if (IsSessionExpired())
		{
			return;
		}
		if (!PlayerClientInstance.IsAccessCodeValidated)
		{
			ShowError("You need to be validated to submit scores.", string.Empty);
			return;
		}
		ListedMatch[] matches = PlayerClientInstance.GetMatches();
		if (matches == null || matches.Length <= 0)
		{
			ShowError("Can't obtain matches.", string.Empty);
			return;
		}
		ListedMatch listedMatch = matches[0];
		PlayerClientInstance.SubmitScore(listedMatch.Id, 10L);
	}
}
