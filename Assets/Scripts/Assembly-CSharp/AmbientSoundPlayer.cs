using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AmbientSoundPlayer : NetworkBehaviour
{
	[Serializable]
	public class AmbientClip
	{
		public AudioClip clip;

		public bool repeatable = true;

		public bool is3D = true;

		public bool played;

		public int index;
	}

	public GameObject audioPrefab;

	public int minTime = 30;

	public int maxTime = 60;

	public AmbientClip[] clips;

	private static int kRpcRpcPlaySound;

	private void Start()
	{
		if (base.isLocalPlayer && base.isServer)
		{
			for (int i = 0; i < clips.Length; i++)
			{
				clips[i].index = i;
			}
			Invoke("GenerateRandom", 10f);
		}
	}

	private void PlaySound(int clipID)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(audioPrefab);
		Vector2 vector = new Vector2(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1));
		Vector3 vector2 = new Vector3(vector.x, 0f, vector.y).normalized * 200f;
		gameObject.transform.position = vector2 + base.transform.position;
		gameObject.GetComponent<AudioSource>().clip = clips[clipID].clip;
		gameObject.GetComponent<AudioSource>().spatialBlend = (clips[clipID].is3D ? 1 : 0);
		gameObject.GetComponent<AudioSource>().Play();
		UnityEngine.Object.Destroy(gameObject, 10f);
	}

	private void GenerateRandom()
	{
		int num = 0;
		List<AmbientClip> list = new List<AmbientClip>();
		AmbientClip[] array = clips;
		foreach (AmbientClip ambientClip in array)
		{
			if (!ambientClip.played)
			{
				list.Add(ambientClip);
			}
		}
		num = UnityEngine.Random.Range(0, list.Count);
		int index = list[num].index;
		if (!clips[index].repeatable)
		{
			clips[index].played = true;
		}
		CallRpcPlaySound(index);
		Invoke("GenerateRandom", UnityEngine.Random.Range(minTime, maxTime));
	}

	[ClientRpc(channel = 1)]
	private void RpcPlaySound(int id)
	{
		PlaySound(id);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcPlaySound(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaySound called on server.");
		}
		else
		{
			((AmbientSoundPlayer)obj).RpcPlaySound((int)reader.ReadPackedUInt32());
		}
	}

	public void CallRpcPlaySound(int id)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPlaySound called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPlaySound);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)id);
		SendRPCInternal(networkWriter, 1, "RpcPlaySound");
	}

	static AmbientSoundPlayer()
	{
		kRpcRpcPlaySound = -1539903091;
		NetworkBehaviour.RegisterRpcDelegate(typeof(AmbientSoundPlayer), kRpcRpcPlaySound, InvokeRpcRpcPlaySound);
		NetworkCRC.RegisterBehaviour("AmbientSoundPlayer", 0);
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
