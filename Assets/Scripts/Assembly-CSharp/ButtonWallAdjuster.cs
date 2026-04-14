using UnityEngine;

public class ButtonWallAdjuster : MonoBehaviour
{
	public bool onAwake;

	private bool _adjusted;

	public float offset = 0.1f;

	private void Start()
	{
		if (onAwake)
		{
			Adjust();
		}
	}

	public void Adjust()
	{
		if (!_adjusted || onAwake)
		{
			_adjusted = true;
			base.transform.position += base.transform.up;
			RaycastHit hitInfo;
			if (Physics.Raycast(new Ray(base.transform.position, -base.transform.up), out hitInfo, 2.5f))
			{
				base.transform.position = hitInfo.point;
				base.transform.position -= base.transform.up * offset * 0.1f;
			}
		}
	}
}
