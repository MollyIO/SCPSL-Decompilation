using UnityEngine;

public class Camera079 : MonoBehaviour
{
	public string cameraName;

	public bool isMain;

	[Space]
	public float curPitch;

	public float curRot;

	private float smoothPitch;

	private float smoothRot;

	[Space]
	public float minRot;

	public float maxRot;

	public float minPitch;

	public float maxPitch;

	[Space]
	public float stepSpeed = 1f;

	public Material activatedMaterial;

	public Material deactivatedMaterial;

	public MeshRenderer renderer;

	[Space]
	public Transform head;

	public Transform targetPosition;

	[SerializeField]
	private float timeToAnimate;

	private void FixedUpdate()
	{
		if (timeToAnimate > 0f)
		{
			timeToAnimate -= 0.02f;
			Animate();
		}
	}

	private void Awake()
	{
		targetPosition = GetComponentInChildren<CameraPos079>(true).transform;
		Object.Destroy(targetPosition.GetComponent<CameraPos079>());
	}

	private void Start()
	{
		UpdatePosition(curRot, curPitch);
	}

	public void UpdatePosition(float _curRot, float _curPitch)
	{
		timeToAnimate = 5f;
		curRot = _curRot;
		curPitch = _curPitch;
	}

	private void Animate()
	{
		curRot = Mathf.Clamp(curRot, minRot, maxRot);
		curPitch = Mathf.Clamp(curPitch, minPitch, maxPitch);
		bool flag = false;
		foreach (Scp079PlayerScript instance in Scp079PlayerScript.instances)
		{
			if (instance.currentCamera == this)
			{
				flag = true;
			}
		}
		if (renderer != null)
		{
			renderer.sharedMaterial = ((!flag) ? deactivatedMaterial : activatedMaterial);
		}
		if (smoothRot > curRot + stepSpeed)
		{
			smoothRot -= stepSpeed;
		}
		if (smoothRot < curRot - stepSpeed)
		{
			smoothRot += stepSpeed;
		}
		if (smoothPitch > curPitch + stepSpeed)
		{
			smoothPitch -= stepSpeed;
		}
		if (smoothPitch < curPitch - stepSpeed)
		{
			smoothPitch += stepSpeed;
		}
		if (Interface079.lply != null && Interface079.lply.currentCamera == this)
		{
			head.localRotation = Quaternion.Lerp(head.localRotation, Quaternion.Euler(curPitch, curRot, 0f), Time.deltaTime * 12f);
		}
		else
		{
			head.localRotation = Quaternion.Euler(smoothPitch, smoothRot, 0f);
		}
	}
}
