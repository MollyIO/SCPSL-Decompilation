using UnityEngine;

public class HandPart : MonoBehaviour
{
	public GameObject part;

	public int id;

	public Animator anim;

	private Inventory inv;

	private void Start()
	{
		if (anim == null)
		{
			anim = GetComponentsInParent<Animator>()[0];
		}
		if (inv == null)
		{
			inv = GetComponentInParent<Inventory>();
		}
	}

	public void UpdateItem()
	{
		part.SetActive(inv.curItem == id);
	}
}
