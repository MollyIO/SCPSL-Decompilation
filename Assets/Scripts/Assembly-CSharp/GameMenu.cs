using UnityEngine;
using UnityEngine.Networking;

public class GameMenu : MonoBehaviour
{
	public GameObject background;

	public GameObject[] minors;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && (!Cursor.visible || Application.isEditor || background.activeSelf || CursorManager.singleton.is079))
		{
			ToggleMenu();
		}
	}

	public void ToggleMenu()
	{
		GameObject[] array = minors;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(false);
			}
		}
		background.SetActive(!background.activeSelf);
		CursorManager.singleton.pauseOpen = background.activeSelf;
		PlayerManager.localPlayer.GetComponent<FirstPersonController>().isPaused = background.activeSelf;
	}

	public void SelectMinor(int id)
	{
		GameObject[] array = minors;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.activeSelf)
			{
				gameObject.SetActive(false);
			}
		}
		minors[id].SetActive(true);
	}

	public void Disconnect()
	{
		if (NetworkServer.active)
		{
			NetworkManager.singleton.StopHost();
		}
		else
		{
			NetworkManager.singleton.StopClient();
		}
	}

	public void Exit()
	{
	}
}
