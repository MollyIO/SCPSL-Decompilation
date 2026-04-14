using UnityEngine;
using UnityEngine.UI;

public class RadioDisplay : MonoBehaviour
{
	public Text label_t;

	public Text power_t;

	public Text battery_t;

	public static string label;

	public static string power;

	public static string battery;

	public GameObject normal;

	public GameObject nobattery;

	public Texture batt1;

	public Texture batt2;

	public RawImage img;

	private void Start()
	{
		InvokeRepeating("ChangeImg", 1f, 0.5f);
	}

	private void Update()
	{
		normal.SetActive(battery != "0");
		nobattery.SetActive(battery == "0");
		label_t.text = label;
		power_t.text = power;
		battery_t.text = "BAT. " + battery + "%";
	}

	private void ChangeImg()
	{
		img.texture = ((!(img.texture == batt1)) ? batt1 : batt2);
	}
}
