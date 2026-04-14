using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("SECTR/Demos/SECTR Character Motor")]
public class SECTR_CharacterMotor : MonoBehaviour
{
	[Serializable]
	public class CharacterMotorMovement
	{
		public float maxForwardSpeed = 3f;

		public float maxSidewaysSpeed = 2f;

		public float maxBackwardsSpeed = 2f;

		public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90f, 1f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));

		public float maxGroundAcceleration = 30f;

		public float maxAirAcceleration = 20f;

		public float gravity = 9.81f;

		public float maxFallSpeed = 20f;

		public float footstepDistance = 1f;

		public float pushPower = 2f;

		[NonSerialized]
		public CollisionFlags collisionFlags;

		[NonSerialized]
		public Vector3 velocity;

		[NonSerialized]
		public Vector3 frameVelocity = Vector3.zero;

		[NonSerialized]
		public Vector3 hitPoint = Vector3.zero;

		[NonSerialized]
		public Vector3 lastHitPoint = new Vector3(float.PositiveInfinity, 0f, 0f);

		[NonSerialized]
		public PhysicMaterial hitMaterial;
	}

	public enum MovementTransferOnJump
	{
		None = 0,
		InitTransfer = 1,
		PermaTransfer = 2,
		PermaLocked = 3
	}

	[Serializable]
	public class CharacterMotorJumping
	{
		public bool enabled = true;

		public float baseHeight = 1f;

		public float extraHeight = 4.1f;

		public float perpAmount;

		public float steepPerpAmount = 0.5f;

		[NonSerialized]
		public bool jumping;

		[NonSerialized]
		public bool holdingJumpButton;

		[NonSerialized]
		public float lastStartTime;

		[NonSerialized]
		public float lastButtonDownTime = -100f;

		[NonSerialized]
		public Vector3 jumpDir = Vector3.up;
	}

	[Serializable]
	public class CharacterMotorMovingPlatform
	{
		public bool enabled = true;

		public MovementTransferOnJump movementTransfer = MovementTransferOnJump.PermaTransfer;

		[NonSerialized]
		public Transform hitPlatform;

		[NonSerialized]
		public Transform activePlatform;

		[NonSerialized]
		public Vector3 activeLocalPoint;

		[NonSerialized]
		public Vector3 activeGlobalPoint;

		[NonSerialized]
		public Quaternion activeLocalRotation;

		[NonSerialized]
		public Quaternion activeGlobalRotation;

		[NonSerialized]
		public Matrix4x4 lastMatrix;

		[NonSerialized]
		public Vector3 platformVelocity;

		[NonSerialized]
		public bool newPlatform;
	}

	[Serializable]
	public class CharacterMotorSliding
	{
		public bool enabled = true;

		public float slidingSpeed = 15f;

		public float sidewaysControl = 1f;

		public float speedControl = 0.4f;
	}

	private bool canControl = true;

	private Vector3 lastGroundNormal = Vector3.zero;

	private Transform cachedTransform;

	private CharacterController cachedController;

	private Vector3 lastFootstepPosition = Vector3.zero;

	private PhysicMaterial defaultHitMaterial;

	[NonSerialized]
	public Vector3 inputMoveDirection = Vector3.zero;

	[NonSerialized]
	public bool inputJump;

	[NonSerialized]
	public bool grounded = true;

	[NonSerialized]
	public Vector3 groundNormal = Vector3.zero;

	[SECTR_ToolTip("Basic movement properties.")]
	public CharacterMotorMovement movement = new CharacterMotorMovement();

	[SECTR_ToolTip("Jump specific movement properties.")]
	public CharacterMotorJumping jumping = new CharacterMotorJumping();

	[SECTR_ToolTip("Platform specific movment properties.")]
	public CharacterMotorMovingPlatform movingPlatform = new CharacterMotorMovingPlatform();

	public CharacterMotorSliding sliding = new CharacterMotorSliding();

	private void Awake()
	{
		cachedController = GetComponent<CharacterController>();
		cachedTransform = base.transform;
		defaultHitMaterial = new PhysicMaterial();
		lastFootstepPosition = cachedTransform.position;
	}

	private void FixedUpdate()
	{
		if (movingPlatform.enabled)
		{
			if (movingPlatform.activePlatform != null)
			{
				if (!movingPlatform.newPlatform)
				{
					movingPlatform.platformVelocity = (movingPlatform.activePlatform.localToWorldMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint) - movingPlatform.lastMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint)) / Time.deltaTime;
				}
				movingPlatform.lastMatrix = movingPlatform.activePlatform.localToWorldMatrix;
				movingPlatform.newPlatform = false;
			}
			else
			{
				movingPlatform.platformVelocity = Vector3.zero;
			}
		}
		Vector3 velocity = movement.velocity;
		velocity = ApplyInputVelocityChange(velocity);
		velocity = ApplyGravityAndJumping(velocity);
		Vector3 zero = Vector3.zero;
		if (MoveWithPlatform())
		{
			Vector3 vector = movingPlatform.activePlatform.TransformPoint(movingPlatform.activeLocalPoint);
			zero = vector - movingPlatform.activeGlobalPoint;
			if (zero != Vector3.zero)
			{
				cachedController.Move(zero);
			}
			Quaternion quaternion = movingPlatform.activePlatform.rotation * movingPlatform.activeLocalRotation;
			float y = (quaternion * Quaternion.Inverse(movingPlatform.activeGlobalRotation)).eulerAngles.y;
			if (y != 0f)
			{
				cachedTransform.Rotate(0f, y, 0f);
			}
		}
		Vector3 position = cachedTransform.position;
		Vector3 motion = velocity * Time.deltaTime;
		float num = Mathf.Max(cachedController.stepOffset, new Vector3(motion.x, 0f, motion.z).magnitude);
		if (grounded)
		{
			motion -= num * Vector3.up;
		}
		movingPlatform.hitPlatform = null;
		groundNormal = Vector3.zero;
		if (cachedController.enabled)
		{
			movement.collisionFlags = cachedController.Move(motion);
		}
		movement.lastHitPoint = movement.hitPoint;
		lastGroundNormal = groundNormal;
		if (movingPlatform.enabled && movingPlatform.activePlatform != movingPlatform.hitPlatform && movingPlatform.hitPlatform != null)
		{
			movingPlatform.activePlatform = movingPlatform.hitPlatform;
			movingPlatform.lastMatrix = movingPlatform.hitPlatform.localToWorldMatrix;
			movingPlatform.newPlatform = true;
		}
		Vector3 vector2 = new Vector3(velocity.x, 0f, velocity.z);
		movement.velocity = (cachedTransform.position - position) / Time.deltaTime;
		Vector3 lhs = new Vector3(movement.velocity.x, 0f, movement.velocity.z);
		if (vector2 == Vector3.zero)
		{
			movement.velocity = new Vector3(0f, movement.velocity.y, 0f);
		}
		else
		{
			float value = Vector3.Dot(lhs, vector2) / vector2.sqrMagnitude;
			movement.velocity = vector2 * Mathf.Clamp01(value) + movement.velocity.y * Vector3.up;
		}
		if ((double)movement.velocity.y < (double)velocity.y - 0.001)
		{
			if (movement.velocity.y < 0f)
			{
				movement.velocity.y = velocity.y;
			}
			else
			{
				jumping.holdingJumpButton = false;
			}
		}
		if (grounded && !IsGroundedTest())
		{
			grounded = false;
			if (movingPlatform.enabled && (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer || movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer))
			{
				movement.frameVelocity = movingPlatform.platformVelocity;
				movement.velocity += movingPlatform.platformVelocity;
			}
			SendMessage("OnFall", (!(movement.hitMaterial != null)) ? defaultHitMaterial : movement.hitMaterial, SendMessageOptions.DontRequireReceiver);
			cachedTransform.position += num * Vector3.up;
		}
		else if (!grounded && IsGroundedTest())
		{
			grounded = true;
			jumping.jumping = false;
			SubtractNewPlatformVelocity();
			SendMessage("OnLand", (!(movement.hitMaterial != null)) ? defaultHitMaterial : movement.hitMaterial, SendMessageOptions.DontRequireReceiver);
		}
		if (MoveWithPlatform())
		{
			movingPlatform.activeGlobalPoint = cachedTransform.position + Vector3.up * (cachedController.center.y - cachedController.height * 0.5f + cachedController.radius);
			movingPlatform.activeLocalPoint = movingPlatform.activePlatform.InverseTransformPoint(movingPlatform.activeGlobalPoint);
			movingPlatform.activeGlobalRotation = cachedTransform.rotation;
			movingPlatform.activeLocalRotation = Quaternion.Inverse(movingPlatform.activePlatform.rotation) * movingPlatform.activeGlobalRotation;
		}
		if (!grounded || TooSteep())
		{
			return;
		}
		if (inputMoveDirection.sqrMagnitude > 0f)
		{
			float num2 = Vector3.SqrMagnitude(position - lastFootstepPosition);
			if (num2 >= movement.footstepDistance * movement.footstepDistance)
			{
				SendMessage("OnFootstep", (!(movement.hitMaterial != null)) ? defaultHitMaterial : movement.hitMaterial, SendMessageOptions.DontRequireReceiver);
				lastFootstepPosition = position;
			}
		}
		else
		{
			lastFootstepPosition = Vector3.zero;
		}
	}

	private Vector3 ApplyInputVelocityChange(Vector3 velocity)
	{
		if (!canControl)
		{
			inputMoveDirection = Vector3.zero;
		}
		Vector3 normalized;
		if (grounded && TooSteep())
		{
			normalized = new Vector3(groundNormal.x, 0f, groundNormal.z).normalized;
			Vector3 vector = Vector3.Project(inputMoveDirection, normalized);
			normalized = normalized + vector * sliding.speedControl + (inputMoveDirection - vector) * sliding.sidewaysControl;
			normalized *= sliding.slidingSpeed;
		}
		else
		{
			normalized = GetDesiredHorizontalVelocity();
		}
		if (movingPlatform.enabled && movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer)
		{
			normalized += movement.frameVelocity;
			normalized.y = 0f;
		}
		if (grounded)
		{
			normalized = AdjustGroundVelocityToNormal(normalized, groundNormal);
		}
		else
		{
			velocity.y = 0f;
		}
		float num = GetMaxAcceleration(grounded) * Time.deltaTime;
		Vector3 vector2 = normalized - velocity;
		if (vector2.sqrMagnitude > num * num)
		{
			vector2 = vector2.normalized * num;
		}
		if (grounded || canControl)
		{
			velocity += vector2;
		}
		if (grounded)
		{
			velocity.y = Mathf.Min(velocity.y, 0f);
		}
		return velocity;
	}

	private Vector3 ApplyGravityAndJumping(Vector3 velocity)
	{
		if (!inputJump || !canControl)
		{
			jumping.holdingJumpButton = false;
			jumping.lastButtonDownTime = -100f;
		}
		if (inputJump && jumping.lastButtonDownTime < 0f && canControl)
		{
			jumping.lastButtonDownTime = Time.time;
		}
		if (grounded)
		{
			velocity.y = Mathf.Min(0f, velocity.y) - movement.gravity * Time.deltaTime;
		}
		else
		{
			velocity.y = movement.velocity.y - movement.gravity * Time.deltaTime;
			if (jumping.jumping && jumping.holdingJumpButton && Time.time < jumping.lastStartTime + jumping.extraHeight / CalculateJumpVerticalSpeed(jumping.baseHeight))
			{
				velocity += jumping.jumpDir * movement.gravity * Time.deltaTime;
			}
			velocity.y = Mathf.Max(velocity.y, 0f - movement.maxFallSpeed);
		}
		if (grounded)
		{
			if (jumping.enabled && canControl && (double)(Time.time - jumping.lastButtonDownTime) < 0.2)
			{
				grounded = false;
				jumping.jumping = true;
				jumping.lastStartTime = Time.time;
				jumping.lastButtonDownTime = -100f;
				jumping.holdingJumpButton = true;
				jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, (!TooSteep()) ? jumping.perpAmount : jumping.steepPerpAmount);
				velocity.y = 0f;
				velocity += jumping.jumpDir * CalculateJumpVerticalSpeed(jumping.baseHeight);
				if (movingPlatform.enabled && (movingPlatform.movementTransfer == MovementTransferOnJump.InitTransfer || movingPlatform.movementTransfer == MovementTransferOnJump.PermaTransfer))
				{
					movement.frameVelocity = movingPlatform.platformVelocity;
					velocity += movingPlatform.platformVelocity;
				}
				SendMessage("OnJump", (!(movement.hitMaterial != null)) ? defaultHitMaterial : movement.hitMaterial, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				jumping.holdingJumpButton = false;
			}
		}
		return velocity;
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (hit.normal.y > 0f && hit.normal.y > groundNormal.y && hit.moveDirection.y < 0f)
		{
			if ((double)(hit.point - movement.lastHitPoint).sqrMagnitude > 0.001 || lastGroundNormal == Vector3.zero)
			{
				groundNormal = hit.normal;
			}
			else
			{
				groundNormal = lastGroundNormal;
			}
			movingPlatform.hitPlatform = hit.collider.transform;
			movement.hitPoint = hit.point;
			movement.hitMaterial = ((!(hit.collider.GetType() == typeof(TerrainCollider))) ? hit.collider.sharedMaterial : ((TerrainCollider)hit.collider).material);
			movement.frameVelocity = Vector3.zero;
		}
		Rigidbody attachedRigidbody = hit.collider.attachedRigidbody;
		if (attachedRigidbody != null && !attachedRigidbody.isKinematic && hit.moveDirection.y >= -0.3f)
		{
			Vector3 vector = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
			attachedRigidbody.velocity = vector * movement.pushPower;
		}
	}

	private IEnumerator SubtractNewPlatformVelocity()
	{
		if (!movingPlatform.enabled || (movingPlatform.movementTransfer != MovementTransferOnJump.InitTransfer && movingPlatform.movementTransfer != MovementTransferOnJump.PermaTransfer))
		{
			yield break;
		}
		if (movingPlatform.newPlatform)
		{
			Transform platform = movingPlatform.activePlatform;
			yield return new WaitForFixedUpdate();
			yield return new WaitForFixedUpdate();
			if (grounded && platform == movingPlatform.activePlatform)
			{
				yield break;
			}
		}
		movement.velocity -= movingPlatform.platformVelocity;
	}

	private bool MoveWithPlatform()
	{
		return movingPlatform.enabled && (grounded || movingPlatform.movementTransfer == MovementTransferOnJump.PermaLocked) && movingPlatform.activePlatform != null;
	}

	private Vector3 GetDesiredHorizontalVelocity()
	{
		Vector3 vector = cachedTransform.InverseTransformDirection(inputMoveDirection);
		float num = MaxSpeedInDirection(vector);
		if (grounded)
		{
			float time = Mathf.Asin(movement.velocity.normalized.y) * 57.29578f;
			num *= movement.slopeSpeedMultiplier.Evaluate(time);
		}
		return cachedTransform.TransformDirection(vector * num);
	}

	private Vector3 AdjustGroundVelocityToNormal(Vector3 hVelocity, Vector3 groundNormal)
	{
		Vector3 lhs = Vector3.Cross(Vector3.up, hVelocity);
		return Vector3.Cross(lhs, groundNormal).normalized * hVelocity.magnitude;
	}

	private bool IsGroundedTest()
	{
		return (double)groundNormal.y > 0.01;
	}

	private float GetMaxAcceleration(bool grounded)
	{
		return (!grounded) ? movement.maxAirAcceleration : movement.maxGroundAcceleration;
	}

	private float CalculateJumpVerticalSpeed(float targetJumpHeight)
	{
		return Mathf.Sqrt(2f * targetJumpHeight * movement.gravity);
	}

	private bool TooSteep()
	{
		return groundNormal.y <= Mathf.Cos(cachedController.slopeLimit * ((float)Math.PI / 180f));
	}

	private float MaxSpeedInDirection(Vector3 desiredMovementDirection)
	{
		if (desiredMovementDirection != Vector3.zero)
		{
			float num = ((!(desiredMovementDirection.z > 0f)) ? movement.maxBackwardsSpeed : movement.maxForwardSpeed) / movement.maxSidewaysSpeed;
			Vector3 normalized = new Vector3(desiredMovementDirection.x, 0f, desiredMovementDirection.z / num).normalized;
			return new Vector3(normalized.x, 0f, normalized.z * num).magnitude * movement.maxSidewaysSpeed;
		}
		return 0f;
	}
}
