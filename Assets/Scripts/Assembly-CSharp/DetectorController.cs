using UnityEngine;

public class DetectorController : MonoBehaviour
{
	public float detectionProgress;

	public float viewRange = 30f;

	public float fov = -0.75f;

	public GameObject[] detectors;

	private void Start()
	{
		InvokeRepeating("RefreshDetectorsList", 10f, 10f);
	}

	public void RefreshDetectorsList()
	{
		detectors = GameObject.FindGameObjectsWithTag("Detector");
	}

	private void Update()
	{
		if (detectors.Length == 0)
		{
			return;
		}
		bool flag = false;
		GameObject[] array = detectors;
		foreach (GameObject gameObject in array)
		{
			if (Vector3.Distance(gameObject.transform.position, base.transform.position) > viewRange)
			{
				Vector3 normalized = (base.transform.position - gameObject.transform.position).normalized;
				RaycastHit hitInfo;
				if (Vector3.Dot(gameObject.transform.forward, normalized) < fov && Physics.Raycast(gameObject.transform.position, normalized, out hitInfo) && hitInfo.transform.tag == "Detector")
				{
					flag = true;
					break;
				}
			}
		}
		detectionProgress += Time.deltaTime * ((!flag) ? (-0.5f) : 0.3f);
		detectionProgress = Mathf.Clamp01(detectionProgress);
	}
}
