using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorManager : MonoBehaviour
{
	public bool eqOpen;

	public bool pauseOpen;

	public bool isServerOnly;

	public bool consoleOpen;

	public bool is079;

	public bool scp106;

	public bool roundStarted;

	public bool raOp;

	public bool plOp;

	public bool debuglogopen;

	public bool isNotFacility;

	public bool isApplicationNotFocused;

	public static CursorManager singleton;

	private void LateUpdate()
	{
		if (!ServerStatic.IsDedicated)
		{
			bool flag = eqOpen | pauseOpen | isServerOnly | consoleOpen | is079 | scp106 | roundStarted | raOp | plOp | debuglogopen | isNotFacility | isApplicationNotFocused;
			Cursor.lockState = ((!flag) ? CursorLockMode.Locked : CursorLockMode.None);
			Cursor.visible = flag;
		}
	}

	public static bool ShouldBeBlurred()
	{
     if (singleton == null)
		{
			return false;
		}
		return singleton.eqOpen | singleton.pauseOpen | singleton.plOp;
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneWasLoaded;
	}

	private void OnDisable()
	{
       SceneManager.sceneLoaded -= OnSceneWasLoaded;
	}

	private void Awake()
	{
		singleton = this;
	}

	private void OnSceneWasLoaded(Scene scene, LoadSceneMode mode)
	{
		UnsetAll();
		isNotFacility = true;
		NonFacilityCompatibility.SceneDescription[] allScenes = NonFacilityCompatibility.singleton.allScenes;
		foreach (NonFacilityCompatibility.SceneDescription sceneDescription in allScenes)
		{
			if (sceneDescription.sceneName == scene.name)
			{
				isNotFacility = false;
			}
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		isApplicationNotFocused = !focus;
	}

	public static void UnsetAll()
	{
       if (singleton == null)
		{
			return;
		}
		singleton.eqOpen = false;
		singleton.pauseOpen = false;
		singleton.isServerOnly = false;
		singleton.consoleOpen = false;
		singleton.is079 = false;
		singleton.scp106 = false;
		singleton.roundStarted = false;
		singleton.raOp = false;
		singleton.plOp = false;
		singleton.debuglogopen = false;
	}
}
