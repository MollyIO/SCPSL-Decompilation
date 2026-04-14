using UnityEngine;

public class DoorColor : MonoBehaviour
{
	public Color Open;

	public Color Close;

	public Color LockedUnselected;

	public Color LockedSelected;

	public Color UnlockedUnselected;

	public Color UnlockedSelected;

	public static DoorColor singleton;

	private void Start()
	{
		singleton = this;
	}
}
