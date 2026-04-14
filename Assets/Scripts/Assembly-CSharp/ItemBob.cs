using UnityEngine;

public class ItemBob : MonoBehaviour
{
	private FirstPersonController fpc;

	private Animator anim;

	public float speedScale = 1f;

	private float lerp;

	private void Start()
	{
		anim = GetComponent<Animator>();
		fpc = GetComponentInParent<FirstPersonController>();
	}

	private void Update()
	{
		lerp = (fpc.m_MoveDir + Vector3.up * 10f).magnitude * speedScale;
		anim.SetFloat("speed", lerp);
	}
}
