using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TextChat : NetworkBehaviour
{
	public int messageDuration;

	private static Transform lply;

	public GameObject textMessagePrefab;

	private Transform attachParent;

	public bool enabledChat;

	private List<GameObject> msgs = new List<GameObject>();

	private void Start()
	{
		if (base.isLocalPlayer)
		{
			lply = base.transform;
		}
	}

	private void Update()
	{
	}

	private void SendChat(string msg, string nick, Vector3 position)
	{
		CmdSendChat(msg, nick, position);
	}

	private void CmdSendChat(string msg, string nick, Vector3 pos)
	{
	}

	private void RpcSendChat(string msg, string nick, Vector3 pos)
	{
	}

	private void AddMsg(string msg, string nick)
	{
		while (msg.Contains("<"))
		{
			msg = msg.Replace("<", "＜");
		}
		while (msg.Contains(">"))
		{
			msg = msg.Replace(">", "＞");
		}
		string text = "<b>" + nick + "</b>: " + msg;
		GameObject gameObject = Object.Instantiate(textMessagePrefab);
		gameObject.transform.SetParent(attachParent);
		msgs.Add(gameObject);
		gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
		gameObject.transform.localScale = Vector3.one;
		gameObject.GetComponent<Text>().text = text;
		gameObject.GetComponent<TextMessage>().remainingLife = messageDuration;
		Object.Destroy(gameObject, messageDuration);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
