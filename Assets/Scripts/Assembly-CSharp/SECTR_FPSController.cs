using UnityEngine;

[RequireComponent(typeof(SECTR_CharacterMotor))]
[AddComponentMenu("SECTR/Demos/SECTR Character Controller")]
public class SECTR_FPSController : SECTR_FPController
{
	private SECTR_CharacterMotor cachedMotor;

	private void Awake()
	{
		cachedMotor = GetComponent<SECTR_CharacterMotor>();
	}

	protected override void Update()
	{
		base.Update();
		Vector3 vector = ((!Input.multiTouchEnabled || Application.isEditor) ? new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f) : ((Vector3)GetScreenJoystick(false)));
		if (vector != Vector3.zero)
		{
			float magnitude = vector.magnitude;
			vector /= magnitude;
			magnitude = Mathf.Min(1f, magnitude);
			magnitude *= magnitude;
			vector *= magnitude;
		}
		vector = base.transform.rotation * vector;
		Quaternion quaternion = Quaternion.FromToRotation(-base.transform.forward, base.transform.up);
		vector = quaternion * vector;
		cachedMotor.inputMoveDirection = vector;
		cachedMotor.inputJump = Input.GetKey(NewInput.GetKey("Jump"));
	}

	private Vector3 ProjectOntoPlane(Vector3 v, Vector3 normal)
	{
		return v - Vector3.Project(v, normal);
	}

	private Vector3 ConstantSlerp(Vector3 from, Vector3 to, float angle)
	{
		float t = Mathf.Min(1f, angle / Vector3.Angle(from, to));
		return Vector3.Slerp(from, to, t);
	}
}
