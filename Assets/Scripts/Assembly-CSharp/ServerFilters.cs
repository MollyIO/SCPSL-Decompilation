using UnityEngine;

public class ServerFilters : MonoBehaviour
{
	private ServerListManager list;

	public string nameFilter;

	public bool AllowToSpawn(string server_name)
	{
		if (nameFilter.Length == 0)
		{
			return true;
		}
		nameFilter = nameFilter.ToUpper();
		int num = 0;
		int num2 = 0;
		string text = nameFilter;
		foreach (char c in text)
		{
			for (int j = num2; j < server_name.Length; j++)
			{
				if (server_name.ToUpper()[j] == c)
				{
					num2 = j;
					num++;
					break;
				}
			}
		}
		return num == nameFilter.Length;
	}

	private void Start()
	{
		list = GetComponent<ServerListManager>();
	}
}
