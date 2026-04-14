using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.Utility;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FirstPersonController : MonoBehaviour
{
	[SerializeField]
	private bool m_IsWalking;

	[SerializeField]
	public float m_WalkSpeed;

	[SerializeField]
	public float m_RunSpeed;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_RunstepLenghten;

	[SerializeField]
	public float m_JumpSpeed;

	[SerializeField]
	private float m_StickToGroundForce;

	[SerializeField]
	private float m_GravityMultiplier;

	public MouseLook m_MouseLook;

	[SerializeField]
	private bool m_UseFovKick;

	[SerializeField]
	private FOVKick m_FovKick = new FOVKick();

	[SerializeField]
	public bool m_UseHeadBob;

	[SerializeField]
	private CurveControlledBob m_HeadBob = new CurveControlledBob();

	[SerializeField]
	private LerpControlledBob m_JumpBob = new LerpControlledBob();

	[SerializeField]
	private float m_StepInterval;

	[SerializeField]
	public AudioClip m_LandSound;

	public Camera m_Camera;

	private bool m_Jump;

	private float m_YRotation;

	private Vector2 m_Input;

	public Vector3 m_MoveDir = Vector3.zero;

	public Vector2 plySpeed;

	public CharacterController m_CharacterController;

	private CollisionFlags m_CollisionFlags;

	private bool m_PreviouslyGrounded;

	private Vector3 m_OriginalCameraPosition;

	private float m_StepCycle;

	private float m_NextStep;

	private bool m_Jumping;

	private AudioSource m_AudioSource;

	public float zoomSlowdown = 1f;

	public bool sneaking;

	public bool lookingAtMe;

	public bool tutstop;

	public bool isInfected;

	public bool isSearching;

	public bool usingConsole;

	public bool usingTurret;

	public bool isPaused;

	public bool noclip;

	public static float speedMultiplier939;

	public bool isSCP;

	public static bool usingRemoteAdmin;

	public bool lockMovement;

	public bool rangeSpeed;

	public int animationID;

	public float blinkAddition;

	private KeyCode m_fwd;

	private KeyCode m_bwd;

	private KeyCode m_lft;

	private KeyCode m_rgt;

	private KeyCode m_sneak;

	public static bool disableJumping;

	private Vector3 previousPosition;

	private float horizontal;

	private float vertical;

	public float smoothSize = 0.5f;

	private void Start()
	{
		m_fwd = NewInput.GetKey("Move Forward");
		m_bwd = NewInput.GetKey("Move Backward");
		m_lft = NewInput.GetKey("Move Left");
		m_rgt = NewInput.GetKey("Move Right");
		m_sneak = NewInput.GetKey("Sneak");
		m_CharacterController = GetComponent<CharacterController>();
		m_OriginalCameraPosition = m_Camera.transform.localPosition;
		m_FovKick.Setup(m_Camera);
		m_HeadBob.Setup(m_Camera, m_StepInterval);
		m_StepCycle = 0f;
		m_NextStep = m_StepCycle / 2f;
		m_Jumping = false;
		m_AudioSource = GetComponent<AudioSource>();
		m_MouseLook.Init(base.transform, m_Camera.transform);
	}

	private void Update()
	{
		RotateView();
		lockMovement = isSearching | usingConsole | noclip | isPaused;
		if (!m_Jump && m_CharacterController.isGrounded)
		{
			m_Jump = Input.GetKeyDown(NewInput.GetKey("Jump")) && !disableJumping;
		}
		if (lockMovement || lookingAtMe || isInfected || tutstop || usingRemoteAdmin)
		{
			m_Jump = false;
		}
		if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
		{
			StartCoroutine(m_JumpBob.DoBobCycle());
			PlayLandingSound();
			m_MoveDir.y = 0f;
			m_Jumping = false;
		}
		if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
		{
			m_MoveDir.y = 0f;
		}
		m_PreviouslyGrounded = m_CharacterController.isGrounded;
	}

	private void PlayLandingSound()
	{
		m_AudioSource.clip = m_LandSound;
		m_AudioSource.Play();
		m_NextStep = m_StepCycle + 0.5f;
		base.gameObject.SendMessage((!Input.GetKey(NewInput.GetKey("Run"))) ? "SyncWalk" : "SyncRun");
	}

	public void MotorPlayer()
	{
		float speed;
		GetInput(out speed);
		Vector3 vector = base.transform.forward * m_Input.y + base.transform.right * m_Input.x;
		RaycastHit hitInfo;
		Physics.SphereCast(base.transform.position, m_CharacterController.radius, Vector3.down, out hitInfo, m_CharacterController.height / 2f, -1, QueryTriggerInteraction.Ignore);
		vector = Vector3.ProjectOnPlane(vector, hitInfo.normal).normalized;
		m_MoveDir.x = vector.x * speed;
		m_MoveDir.z = vector.z * speed;
		if (m_CharacterController.isGrounded)
		{
			m_MoveDir.y = 0f - m_StickToGroundForce;
			if (m_Jump)
			{
				m_MoveDir.y = m_JumpSpeed;
				m_Jump = false;
				m_Jumping = true;
			}
		}
		else
		{
			m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
		}
		if (noclip)
		{
			m_MoveDir = Vector3.zero;
		}
		m_CollisionFlags = ((blinkAddition != 0f) ? m_CharacterController.Move(m_MoveDir * blinkAddition / 10f) : m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime));
		ProgressStepCycle(speed);
		UpdateCameraPosition(speed);
	}

	private void FixedUpdate()
	{
		MotorPlayer();
		Vector3 vector = (base.transform.position - previousPosition) / Time.fixedDeltaTime;
		vector = Quaternion.Euler(0f, 0f - base.transform.rotation.eulerAngles.y, 0f) * vector;
		plySpeed = new Vector2(Mathf.Round(vector.z), Mathf.Round(vector.x));
		previousPosition = base.transform.position;
	}

	private void ProgressStepCycle(float speed)
	{
		if (m_CharacterController.velocity.sqrMagnitude > 0f && (m_Input.x != 0f || m_Input.y != 0f))
		{
			m_StepCycle += (m_CharacterController.velocity.magnitude + speed * ((!m_IsWalking) ? m_RunstepLenghten : 1f)) * Time.fixedDeltaTime;
		}
		if (m_StepCycle > m_NextStep)
		{
			m_NextStep = m_StepCycle + m_StepInterval;
			PlayFootStepAudio();
		}
	}

	private void PlayFootStepAudio()
	{
		if (m_CharacterController.isGrounded && !(zoomSlowdown <= 0.4f) && !sneaking && GetComponent<NetworkIdentity>().isLocalPlayer)
		{
			base.gameObject.SendMessage((!Input.GetKey(NewInput.GetKey("Run"))) ? "SyncWalk" : "SyncRun");
		}
	}

	private void UpdateCameraPosition(float speed)
	{
		if (m_UseHeadBob)
		{
			Vector3 localPosition;
			if (m_CharacterController.velocity.magnitude > 0f && m_CharacterController.isGrounded)
			{
				m_Camera.transform.localPosition = m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude + speed * ((!m_IsWalking) ? m_RunstepLenghten : 1f));
				localPosition = m_Camera.transform.localPosition;
				localPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
			}
			else
			{
				localPosition = m_Camera.transform.localPosition;
				localPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
			}
			m_Camera.transform.localPosition = localPosition;
		}
	}

	private void GetInput(out float speed)
	{
		int num = 0;
		if (Input.GetKey(m_fwd))
		{
			num++;
		}
		if (Input.GetKey(m_bwd))
		{
			num--;
		}
		int num2 = 0;
		if (Input.GetKey(m_rgt))
		{
			num2++;
		}
		if (Input.GetKey(m_lft))
		{
			num2--;
		}
		horizontal = num2;
		vertical = num;
		bool isWalking = m_IsWalking;
		sneaking = Input.GetKey(m_sneak) && !isSCP;
		m_IsWalking = !Input.GetKey(NewInput.GetKey("Run")) || zoomSlowdown != 1f || sneaking;
		speed = ((zoomSlowdown < ((!sneaking) ? 1f : 0.4f)) ? zoomSlowdown : ((!sneaking) ? 1f : 0.4f)) * (((!m_IsWalking) ? m_RunSpeed : m_WalkSpeed) * speedMultiplier939 * (float)((!(lockMovement | lookingAtMe | isInfected | tutstop | usingRemoteAdmin)) ? 1 : 0) * (float)((!(rangeSpeed & !m_IsWalking)) ? 1 : 15) + blinkAddition) * m_Input.sqrMagnitude;
		m_Input = Vector2.Lerp(m_Input, new Vector2(horizontal, vertical), smoothSize);
		if (m_Input.sqrMagnitude > 1f)
		{
			m_Input.Normalize();
		}
		if (m_IsWalking != isWalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0f)
		{
			StopAllCoroutines();
			StartCoroutine(m_IsWalking ? m_FovKick.FOVKickDown() : m_FovKick.FOVKickUp());
		}
	}

	private void RotateView()
	{
		if (!lockMovement || noclip)
		{
			m_MouseLook.LookRotation(base.transform, m_Camera.transform);
		}
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Rigidbody attachedRigidbody = hit.collider.attachedRigidbody;
		if (m_CollisionFlags != CollisionFlags.Below && !(attachedRigidbody == null) && !attachedRigidbody.isKinematic)
		{
			attachedRigidbody.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
		}
	}

	private void LateUpdate()
	{
		animationID = (m_Jumping ? 2 : ((Input.GetKey(NewInput.GetKey("Run")) && zoomSlowdown == 1f && !sneaking) ? 1 : 0));
	}
}
