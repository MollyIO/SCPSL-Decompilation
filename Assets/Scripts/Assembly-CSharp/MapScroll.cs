using UnityEngine;

public class MapScroll : MonoBehaviour
{
	public RectTransform map;

	public RectTransform rootTransf;

	public float minZoom;

	public float maxZoom;

	public float speed;

	private void Start()
	{
		rootTransf = GetComponent<RectTransform>();
	}

	private void Update()
	{
		rootTransf.localScale += Vector3.one * Input.GetAxis("Mouse ScrollWheel") * 2f * minZoom;
		rootTransf.localScale = Vector3.one * Mathf.Clamp(rootTransf.localScale.x, minZoom, maxZoom);
		if (Input.GetKey(NewInput.GetKey("Fire1")))
		{
			map.localPosition += new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0f) * speed * (2f / rootTransf.localScale.x);
		}
		if (Input.GetKey(NewInput.GetKey("Zoom")))
		{
			rootTransf.localScale = Vector3.one;
			map.localPosition = Vector3.zero;
		}
	}
}
