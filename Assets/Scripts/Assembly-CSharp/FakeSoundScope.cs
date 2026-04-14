using UnityEngine;

public class FakeSoundScope : MonoBehaviour
{
	public AnimationCurve highOverVolume;

	public int numOfPos;

	private LineRenderer line;

	public float maxH;

	private void Awake()
	{
		line = GetComponent<LineRenderer>();
	}

	private void LateUpdate()
	{
		Vector3[] array = new Vector3[numOfPos];
		float value = Random.value;
		float num = ((!MyMicrophoneIndicator.isTransmitting) ? 0f : highOverVolume.Evaluate(MyMicrophoneIndicator.curVolume));
		for (int i = 0; i < numOfPos; i++)
		{
			float num2 = (float)i / (float)numOfPos;
			float num3 = Mathf.Abs(1f - Mathf.Abs(num2 - 0.5f) * 2f);
			array[i][0] = num2 * 100f;
			array[i][2] = Mathf.Sin((float)(i * 7) * value) * num3 * maxH * (Mathf.Sin(i) / 3f) * num;
		}
		line.SetPositions(array);
	}
}
