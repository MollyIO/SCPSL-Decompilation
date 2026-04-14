using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AlphaWarheadOutsitePanel : NetworkBehaviour
{
	public Animator panelButtonCoverAnim;

	public static AlphaWarheadNukesitePanel nukeside;

	private static AlphaWarheadController _host;

	public Text[] display;

	public GameObject[] inevitable;

	[SyncVar(hook = "SetKeycardState")]
	public bool keycardEntered;

	public bool NetworkkeycardEntered
	{
		get
		{
			return keycardEntered;
		}
		[param: In]
		set
		{
			AlphaWarheadOutsitePanel alphaWarheadOutsitePanel = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetKeycardState(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref alphaWarheadOutsitePanel.keycardEntered, 1u);
		}
	}

	private void Update()
	{
		if (_host == null)
		{
			_host = AlphaWarheadController.host;
			return;
		}
		base.transform.localPosition = new Vector3(0f, 0f, 9f);
		Text[] array = display;
		foreach (Text text in array)
		{
			text.text = GetTimeString();
		}
		GameObject[] array2 = inevitable;
		foreach (GameObject gameObject in array2)
		{
			gameObject.SetActive(_host.timeToDetonation <= 10f && _host.timeToDetonation > 0f);
		}
		panelButtonCoverAnim.SetBool("enabled", keycardEntered);
	}

	public void SetKeycardState(bool b)
	{
		NetworkkeycardEntered = b;
	}

	public static string GetTimeString()
	{
		if (!nukeside.enabled && !_host.inProgress)
		{
			return "<size=180><color=red>DISABLED</color></size>";
		}
		if (!_host.inProgress)
		{
			return (!(_host.timeToDetonation > AlphaWarheadController.host.RealDetonationTime())) ? "<color=lime><size=180>READY</size></color>" : "<color=red><size=200>PLEASE WAIT</size></color>";
		}
		if (_host.timeToDetonation == 0f)
		{
			return ((int)(Time.realtimeSinceStartup * 4f) % 2 != 0) ? "<color=orange><size=270>00:00:00</size></color>" : string.Empty;
		}
		float num = (AlphaWarheadController.host.RealDetonationTime() - AlphaWarheadController.alarmSource.time) * 100f;
		num *= 1f + 2.5f / AlphaWarheadController.host.RealDetonationTime();
		if (num < 0f)
		{
			num = 0f;
		}
		int num2 = 0;
		int num3 = 0;
		while (num >= 100f)
		{
			num -= 100f;
			num2++;
		}
		while (num2 >= 60)
		{
			num2 -= 60;
			num3++;
		}
		return "<color=orange><size=270>" + num3.ToString("00").Substring(0, 2) + ":" + num2.ToString("00").Substring(0, 2) + ":" + num.ToString("00").Substring(0, 2) + "</size></color>";
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(keycardEntered);
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
			writer.Write(keycardEntered);
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
			keycardEntered = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetKeycardState(reader.ReadBoolean());
		}
	}
}
