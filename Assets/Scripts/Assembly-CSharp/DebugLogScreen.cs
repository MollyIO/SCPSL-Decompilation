using UnityEngine;

public class DebugLogScreen : MonoBehaviour
{
	public GameObject log;

	public GameObject info;

	private void OnEnable()
	{
		info.SetActive(true);
		CursorManager.singleton.debuglogopen = log.activeSelf;
	}

	private void OnDisable()
	{
		info.SetActive(false);
		CursorManager.singleton.debuglogopen = false;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F4) && DebugLogReader.SuccesfullyInitialized())
		{
			log.SetActive(!log.activeSelf);
			CursorManager.singleton.debuglogopen = log.activeSelf;
		}
	}
}
