using UnityEngine;

public class SkinColorChanger : MonoBehaviour
{
	public Material ci;

	public Material mtf;

	public Material classd;

	public Material scientist;

	public Material guard;

	private int lastClass = -1;

	private void OnEnable()
	{
		Renderer component = GetComponent<SkinnedMeshRenderer>();
		CharacterClassManager componentInParent = GetComponentInParent<CharacterClassManager>();
		if (lastClass != componentInParent.curClass)
		{
			lastClass = componentInParent.curClass;
			switch (componentInParent.klasy[componentInParent.curClass].team)
			{
			case Team.MTF:
				component.sharedMaterial = ((componentInParent.curClass != 15) ? mtf : guard);
				break;
			case Team.CHI:
				component.sharedMaterial = ci;
				break;
			case Team.RSC:
				component.sharedMaterial = scientist;
				break;
			default:
				component.sharedMaterial = classd;
				break;
			}
		}
	}
}
