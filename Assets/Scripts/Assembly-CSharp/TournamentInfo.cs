using SCPSL.GameOn;
using UnityEngine;
using UnityEngine.UI;

public class TournamentInfo : MonoBehaviour
{
	[SerializeField]
	private Text title;

	[SerializeField]
	private Text subtitle;

	[SerializeField]
	private Text players;

	[SerializeField]
	private Text startDate;

	[SerializeField]
	private Text endDate;

	[SerializeField]
	private Text tournamentId;

	[SerializeField]
	private Text status;

	[SerializeField]
	private GameObject normalInfo;

	[SerializeField]
	private GameObject description;

	private PlayerTournament tourney;

	public bool IsShowingDescription { get; set; }

	private void Start()
	{
		IsShowingDescription = false;
		description.SetActive(IsShowingDescription);
		normalInfo.SetActive(!IsShowingDescription);
	}

	private void Update()
	{
	}

	public void SetInfo(PlayerTournament tournament)
	{
		tourney = tournament;
		title.text = "Title: " + tournament.Title;
		subtitle.text = "Subtitle: " + tournament.Subtitle;
		startDate.text = "Starts: " + tournament.DateStart.LocalDateTime;
		endDate.text = "Ends: " + tournament.DateEnd.LocalDateTime;
		tournamentId.text = "Tournament ID: " + tournament.Id;
		status.text = "Status: " + tournament.TournamentState.ToString();
		players.text = "Participants: " + tournament.PlayersEntered;
		description.GetComponent<Text>().text = "Description: " + tournament.Description;
		IsShowingDescription = false;
		description.SetActive(IsShowingDescription);
		normalInfo.SetActive(!IsShowingDescription);
	}

	public void SetInfo(DeveloperTournament tournament)
	{
		title.text = "Title: " + tournament.Title;
		subtitle.text = "Subtitle: " + tournament.Subtitle;
		startDate.text = "Starts: " + tournament.DateStart.LocalDateTime;
		endDate.text = "Ends: " + tournament.DateEnd.LocalDateTime;
		tournamentId.text = "Tournament ID: " + tournament.Id;
		status.text = "Status: " + tournament.TournamentState.ToString();
		players.text = "Participants: Unknown";
		description.GetComponent<Text>().text = "Description: " + tournament.Description;
		IsShowingDescription = false;
		description.SetActive(IsShowingDescription);
		normalInfo.SetActive(!IsShowingDescription);
	}

	public void ToggleDescription()
	{
		IsShowingDescription = !IsShowingDescription;
		description.SetActive(IsShowingDescription);
		normalInfo.SetActive(!IsShowingDescription);
	}

	public void ShowHighscores()
	{
	}
}
