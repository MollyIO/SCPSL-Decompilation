using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class AlphaWarheadNukesitePanel : NetworkBehaviour
{
	public Transform lever;

	public BlastDoor blastDoor;

	public Door outsideDoor;

	public Material led_blastdoors;

	public Material led_outsidedoor;

	public Material led_detonationinprogress;

	public Material led_cancel;

	public Material[] onOffMaterial;

	private float _leverStatus;

	[SyncVar(hook = "SetEnabled")]
	public new bool enabled;

	public bool Networkenabled
	{
		get
		{
			return enabled;
		}
		[param: In]
		set
		{
			AlphaWarheadNukesitePanel alphaWarheadNukesitePanel = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetEnabled(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref alphaWarheadNukesitePanel.enabled, 1u);
		}
	}

	private void Awake()
	{
		AlphaWarheadOutsitePanel.nukeside = this;
	}

	private void FixedUpdate()
	{
		UpdateLeverStatus();
	}

	public bool AllowChangeLevelState()
	{
		return _leverStatus == 0f || _leverStatus == 1f;
	}

	private void UpdateLeverStatus()
	{
		if (!(AlphaWarheadController.host == null))
		{
			Color color = new Color(0.2f, 0.3f, 0.5f);
			led_detonationinprogress.SetColor("_EmissionColor", (!AlphaWarheadController.host.inProgress) ? Color.black : color);
			led_outsidedoor.SetColor("_EmissionColor", (!outsideDoor.isOpen) ? Color.black : color);
			led_blastdoors.SetColor("_EmissionColor", (!blastDoor.isClosed) ? Color.black : color);
			led_cancel.SetColor("_EmissionColor", (!(AlphaWarheadController.host.timeToDetonation > 10f) || !AlphaWarheadController.host.inProgress) ? Color.black : Color.red);
			_leverStatus += ((!enabled) ? (-0.04f) : 0.04f);
			_leverStatus = Mathf.Clamp01(_leverStatus);
			for (int i = 0; i < 2; i++)
			{
				onOffMaterial[i].SetColor("_EmissionColor", (i != Mathf.RoundToInt(_leverStatus)) ? Color.black : new Color(1.2f, 1.2f, 1.2f, 1f));
			}
			lever.localRotation = Quaternion.Euler(new Vector3(Mathf.Lerp(10f, -170f, _leverStatus), -90f, 90f));
		}
	}

	public void SetEnabled(bool b)
	{
		Networkenabled = b;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(enabled);
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
			writer.Write(enabled);
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
			enabled = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetEnabled(reader.ReadBoolean());
		}
	}
}
