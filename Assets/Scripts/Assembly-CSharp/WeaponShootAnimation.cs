using UnityEngine;

public class WeaponShootAnimation : MonoBehaviour
{
	public float curPosition;

	public Vector3 maxRecoilPos;

	public Vector3 maxRecoilRot;

	public float backSpeed;

	public float backY_Speed;

	private float yOverride;

	private float curY;

	private void LateUpdate()
	{
		if ((double)curPosition > 0.03)
		{
			curPosition = Mathf.Lerp(curPosition, 0f, Time.deltaTime * backSpeed * curPosition);
		}
		else
		{
			curPosition -= Time.deltaTime * 0.1f;
		}
		if (curPosition < 0f)
		{
			curPosition = 0f;
		}
		yOverride = Mathf.Lerp(0f, yOverride, curPosition);
		curY = Mathf.Lerp(curY, yOverride, Time.deltaTime * backY_Speed * curPosition);
		base.transform.localPosition = Vector3.Lerp(Vector3.zero, maxRecoilPos, curPosition);
		base.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.Euler(maxRecoilRot + Vector3.up * curY), curPosition);
	}

	public void Recoil(float f)
	{
		curPosition = Mathf.Clamp01(curPosition + f);
		yOverride = Random.Range(-10f, 10f) * f;
	}
}
