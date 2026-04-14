using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class LureSubjectContainer : NetworkBehaviour
{
	private Vector3 position = new Vector3(-1471f, 160.5f, -3426.9f);

	private Vector3 rotation = new Vector3(0f, 180f, 0f);

	public float range;

	[SyncVar(hook = "SetState")]
	public bool allowContain;

	private CharacterClassManager ccm;

	[Space(10f)]
	public Transform hatch;

	public Vector3 closedPos;

	public Vector3 openPosition;

	private GameObject localplayer;

	public bool NetworkallowContain
	{
		get
		{
			return allowContain;
		}
		[param: In]
		set
		{
			LureSubjectContainer lureSubjectContainer = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetState(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref lureSubjectContainer.allowContain, 1u);
		}
	}

	public void SetState(bool b)
	{
		NetworkallowContain = b;
		if (b)
		{
			hatch.GetComponent<AudioSource>().Play();
		}
	}

	private void Start()
	{
		base.transform.localPosition = position;
		base.transform.localRotation = Quaternion.Euler(rotation);
	}

	private void Update()
	{
		CheckForLure();
		hatch.localPosition = Vector3.Slerp(hatch.localPosition, (!allowContain) ? openPosition : closedPos, Time.deltaTime * 3f);
	}

	private void CheckForLure()
	{
		if (ccm == null)
		{
			localplayer = PlayerManager.localPlayer;
			if (localplayer != null)
			{
				ccm = localplayer.GetComponent<CharacterClassManager>();
			}
		}
		else if (ccm.curClass >= 0)
		{
			Team team = ccm.klasy[ccm.curClass].team;
			GetComponent<BoxCollider>().enabled = team == Team.SCP || ccm.GodMode;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.position, range);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(allowContain);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(allowContain);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			allowContain = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetState(reader.ReadBoolean());
		}
	}
}
