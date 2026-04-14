using RemoteAdmin;
using UnityEngine;

public struct PlayerPositionData
{
	public Vector3 position;

	public float rotation;

	public int playerID;

	public PlayerPositionData(Vector3 _pos, float _rotY, int _id)
	{
		position = _pos;
		rotation = _rotY;
		playerID = _id;
	}

	public PlayerPositionData(GameObject _player)
	{
		playerID = _player.GetComponent<QueryProcessor>().PlayerId;
		Scp079PlayerScript component = _player.GetComponent<Scp079PlayerScript>();
		if (component.iAm079)
		{
			try
			{
				position = ((!string.IsNullOrEmpty(component.Speaker)) ? GameObject.Find(component.Speaker).transform.position : (Vector3.up * 7979f));
			}
			catch
			{
				position = Vector3.up * 7970f;
			}
			rotation = 0f;
		}
		else
		{
			PlyMovementSync component2 = _player.GetComponent<PlyMovementSync>();
			position = ((component2.ccm.curClass != 2) ? component2.position : (Vector3.up * 6000f));
			rotation = component2.rotation;
		}
	}
}
