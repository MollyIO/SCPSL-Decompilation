using UnityEngine;

[AddComponentMenu("SECTR/Demos/SECTR Ghost Controller")]
public class SECTR_GhostController : SECTR_FPController
{
	[SECTR_ToolTip("The speed at which to fly through the world.")]
	public float FlySpeed = 0.5f;

	[SECTR_ToolTip("The translation acceleration amount applied by keyboard input.")]
	public float AccelerationRatio = 1f;

	[SECTR_ToolTip("The amount by which holding down Ctrl slows you down.")]
	public float SlowDownRatio = 0.5f;

	protected override void Update()
	{
		base.Update();
		if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
		{
			FlySpeed *= AccelerationRatio * Time.deltaTime;
		}
		if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
		{
			FlySpeed /= AccelerationRatio * Time.deltaTime;
		}
		if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
		{
			FlySpeed *= SlowDownRatio * Time.deltaTime;
		}
		if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
		{
			FlySpeed /= SlowDownRatio * Time.deltaTime;
		}
		Vector2 vector = ((!Input.multiTouchEnabled || Application.isEditor) ? new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) : GetScreenJoystick(false));
		base.transform.position += base.transform.forward * FlySpeed * Time.deltaTime * vector.y + base.transform.right * FlySpeed * Time.deltaTime * vector.x;
		if (Input.GetKey(KeyCode.E))
		{
			base.transform.position += base.transform.up * FlySpeed * Time.deltaTime * 0.5f;
		}
		else if (Input.GetKey(KeyCode.Q))
		{
			base.transform.position -= base.transform.right * FlySpeed * Time.deltaTime * 0.5f;
		}
	}
}
