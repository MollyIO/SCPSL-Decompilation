using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class LocalCurrentRoomEffects : NetworkBehaviour
{
	private Color curColor;

	public Color normalColor;

	public Color vhighColor;

	public static bool isVhigh;

	public float deltatimeScale = 3f;

	private CharacterClassManager ccm;

	private bool isInFlickerableRoom;

	[SyncVar(hook = "SetFlicker")]
	public bool syncFlicker;

	public bool NetworksyncFlicker
	{
		get
		{
			return syncFlicker;
		}
		[param: In]
		set
		{
			LocalCurrentRoomEffects localCurrentRoomEffects = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetFlicker(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref localCurrentRoomEffects.syncFlicker, 1u);
		}
	}

	private void Start()
	{
		ccm = GetComponent<CharacterClassManager>();
	}

	private void SetFlicker(bool b)
	{
		NetworksyncFlicker = b;
	}

	private void Update()
	{
		if (!ccm.isLocalPlayer && !NetworkServer.active)
		{
			return;
		}
		GameObject gameObject = null;
		RaycastHit hitInfo;
		if (Physics.Raycast(new Ray(base.transform.position, Vector3.down), out hitInfo, 100f, Interface079.singleton.roomDetectionMask))
		{
			Transform parent = hitInfo.transform;
			while (parent != null && !parent.transform.name.ToUpper().Contains("ROOT"))
			{
				parent = parent.transform.parent;
			}
			if (parent != null)
			{
				gameObject = parent.gameObject;
			}
		}
		if (NetworkServer.active)
		{
			if (gameObject != null)
			{
				FlickerableLight componentInChildren = gameObject.GetComponentInChildren<FlickerableLight>();
				isInFlickerableRoom = componentInChildren != null && componentInChildren.IsDisabled();
			}
			else
			{
				isInFlickerableRoom = false;
			}
			if (syncFlicker != isInFlickerableRoom)
			{
				SetFlicker(isInFlickerableRoom);
			}
		}
		if (ccm.isLocalPlayer)
		{
			bool flag;
			if (gameObject != null)
			{
				FlickerableLight componentInChildren2 = gameObject.GetComponentInChildren<FlickerableLight>();
				flag = ((!(componentInChildren2 != null) || !componentInChildren2.IsDisabled()) ? true : false);
			}
			else
			{
				flag = true;
			}
			RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, flag ? ((!isVhigh) ? normalColor : vhighColor) : ((ccm.klasy[ccm.curClass].team != Team.SCP) ? Color.black : vhighColor), Time.deltaTime * deltatimeScale);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(syncFlicker);
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
			writer.Write(syncFlicker);
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
			syncFlicker = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetFlicker(reader.ReadBoolean());
		}
	}
}
