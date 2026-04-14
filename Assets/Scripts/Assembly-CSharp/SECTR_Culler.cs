using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SECTR_Member))]
[AddComponentMenu("")]
public class SECTR_Culler : MonoBehaviour
{
	private SECTR_Member cachedMember;

	[SECTR_ToolTip("Overrides the culling information on Member.")]
	public bool CullEachChild;

	private void OnEnable()
	{
		cachedMember = GetComponent<SECTR_Member>();
		cachedMember.ChildCulling = ((!CullEachChild) ? SECTR_Member.ChildCullModes.Group : SECTR_Member.ChildCullModes.Individual);
	}

	private void OnDisable()
	{
	}
}
