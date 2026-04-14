using UnityEngine;

public class GunAim : MonoBehaviour
{
	public int borderLeft;

	public int borderRight;

	public int borderTop;

	public int borderBottom;

	private Camera parentCamera;

	private bool isOutOfBounds;

	private void Start()
	{
		parentCamera = GetComponentInParent<Camera>();
	}

	private void Update()
	{
		float x = Input.mousePosition.x;
		float y = Input.mousePosition.y;
		if (x <= (float)borderLeft || x >= (float)(Screen.width - borderRight) || y <= (float)borderBottom || y >= (float)(Screen.height - borderTop))
		{
			isOutOfBounds = true;
		}
		else
		{
			isOutOfBounds = false;
		}
		if (!isOutOfBounds)
		{
			base.transform.LookAt(parentCamera.ScreenToWorldPoint(new Vector3(x, y, 5f)));
		}
	}

	public bool GetIsOutOfBounds()
	{
		return isOutOfBounds;
	}
}
