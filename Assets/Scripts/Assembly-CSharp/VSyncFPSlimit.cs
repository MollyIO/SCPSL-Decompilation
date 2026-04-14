using UnityEngine;
using UnityEngine.UI;

public class VSyncFPSlimit : MonoBehaviour
{
	public void Check()
	{
		if (base.gameObject.GetComponent<Slider>().value == 0f)
		{
			int num = PlayerPrefs.GetInt("MaxFramerate", 969);
			if (num == 969)
			{
				Application.targetFrameRate = -1;
			}
			else
			{
				Application.targetFrameRate = num;
			}
		}
	}
}
