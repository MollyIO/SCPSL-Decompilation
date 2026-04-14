using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class NineTailedFoxUnits : NetworkBehaviour
{
	public string[] names;

	[SyncVar(hook = "SetList")]
	public SyncListString list;

	private CharacterClassManager ccm;

	private TextMeshProUGUI txtlist;

	public static NineTailedFoxUnits host;

	private static int kListlist;

	public NineTailedFoxUnits()
	{
		list = new SyncListString();
	}

	private void SetList(SyncListString l)
	{
		list = l;
	}

	private void AddUnit(string unit)
	{
		list.Add(unit);
	}

	private string GenerateName()
	{
		return names[Random.Range(0, names.Length)] + "-" + Random.Range(1, 20).ToString("00");
	}

	private void Start()
	{
		ccm = GetComponent<CharacterClassManager>();
		txtlist = GameObject.Find("NTFlist").GetComponent<TextMeshProUGUI>();
		if (base.isLocalPlayer)
		{
			if (NetworkServer.active)
			{
				NewName();
				host = this;
			}
			else
			{
				host = null;
			}
		}
	}

	private void Update()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		if (host == null)
		{
			GameObject gameObject = GameObject.Find("Host");
			if (gameObject != null)
			{
				host = gameObject.GetComponent<NineTailedFoxUnits>();
			}
			return;
		}
		txtlist.text = string.Empty;
		if (ccm.curClass <= 0 || ccm.klasy[ccm.curClass].team != Team.MTF)
		{
			return;
		}
		for (int i = 0; i < host.list.Count; i++)
		{
			if (i == ccm.ntfUnit)
			{
				TextMeshProUGUI textMeshProUGUI = txtlist;
				textMeshProUGUI.text = textMeshProUGUI.text + "<u>" + host.GetNameById(i) + "</u>";
			}
			else
			{
				txtlist.text += host.GetNameById(i);
			}
			txtlist.text += "\n";
		}
	}

	public int NewName(out int number, out char letter)
	{
		int num = 0;
		string text = GenerateName();
		while (list.Contains(text) && num < 100)
		{
			num++;
			text = GenerateName();
		}
		letter = text.ToUpper()[0];
		number = int.Parse(text.Split('-')[1]);
		AddUnit(text);
		return list.Count - 1;
	}

	public int NewName()
	{
		int number;
		char letter;
		return NewName(out number, out letter);
	}

	public string GetNameById(int id)
	{
		return list[id];
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncListlist(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("SyncList list called on server.");
		}
		else
		{
			((NineTailedFoxUnits)obj).list.HandleMsg(reader);
		}
	}

	static NineTailedFoxUnits()
	{
		kListlist = -376129279;
		NetworkBehaviour.RegisterSyncListDelegate(typeof(NineTailedFoxUnits), kListlist, InvokeSyncListlist);
		NetworkCRC.RegisterBehaviour("NineTailedFoxUnits", 0);
	}

	private void Awake()
	{
		list.InitializeBehaviour(this, kListlist);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			SyncListString.WriteInstance(writer, list);
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
			SyncListString.WriteInstance(writer, list);
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
			SyncListString.ReadReference(reader, list);
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SyncListString.ReadReference(reader, list);
		}
	}
}
