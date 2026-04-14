using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
	public float borderWidthPercent;

	private float rotSpeed;

	private void Update()
	{
		float num = (float)Screen.width * (borderWidthPercent / 100f);
		Vector3 zero = Vector3.zero;
		Vector3 mousePosition = Input.mousePosition;
		if (mousePosition.x < num && base.transform.localRotation.eulerAngles.y > 41f)
		{
			zero += Vector3.down;
		}
		if (mousePosition.x > (float)Screen.width - num && base.transform.localRotation.eulerAngles.y < 74f)
		{
			zero += Vector3.up;
		}
		if (zero == Vector3.zero)
		{
			rotSpeed = 0f;
		}
		else
		{
			rotSpeed += Time.deltaTime * 200f;
			rotSpeed = Mathf.Clamp(rotSpeed, 0f, 120f);
		}
		zero.Normalize();
		base.transform.localRotation = Quaternion.Euler(base.transform.localRotation.eulerAngles + zero * Time.deltaTime * rotSpeed);
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			Raycast();
		}
	}

	private void Raycast()
	{
		RaycastHit hitInfo;
		if (Physics.Raycast(GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hitInfo))
		{
			ElementChoosen(hitInfo.transform.name);
		}
	}

	public void ElementChoosen(string id)
	{
		switch (id)
		{
		case "EXIT":
			Application.Quit();
			break;
		case "PLAY":
			Object.FindObjectOfType<NetManagerValueSetter>().HostGame();
			break;
		}
	}
}
