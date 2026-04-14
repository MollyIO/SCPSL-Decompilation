using UnityEngine;

public class DetectorBlink : MonoBehaviour
{
	public Material mat;

	private bool state;

	private void Start()
	{
		Blink();
	}

	private void Blink()
	{
		state = !state;
		int num = (state ? 2 : 0);
		mat.SetColor("_EmissionColor", new Color(num, num, num));
		Invoke("Blink", (!state) ? 1.3f : 0.2f);
	}
}
