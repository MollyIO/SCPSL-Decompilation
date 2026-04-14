using UnityEngine;

public class SpawnpointManager : MonoBehaviour
{
	public GameObject GetRandomPosition(int classID)
	{
		GameObject result = null;
		Class obj = GameObject.Find("Host").GetComponent<CharacterClassManager>().klasy[classID];
		if (obj.team == Team.CDP || obj.team == Team.TUT)
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag("SP_CDP");
			int num = Random.Range(0, array.Length);
			result = array[num];
		}
		if (classID == 10)
		{
			return null;
		}
		switch (obj.team)
		{
		case Team.SCP:
			switch (classID)
			{
			case 3:
			{
				GameObject[] array8 = GameObject.FindGameObjectsWithTag("SP_106");
				int num8 = Random.Range(0, array8.Length);
				result = array8[num8];
				break;
			}
			case 5:
			{
				GameObject[] array7 = GameObject.FindGameObjectsWithTag("SP_049");
				int num7 = Random.Range(0, array7.Length);
				result = array7[num7];
				break;
			}
			case 7:
			{
				GameObject[] array10 = GameObject.FindGameObjectsWithTag("SP_079");
				int num10 = Random.Range(0, array10.Length);
				result = array10[num10];
				break;
			}
			case 9:
			{
				GameObject[] array9 = GameObject.FindGameObjectsWithTag("SCP_096");
				int num9 = Random.Range(0, array9.Length);
				result = array9[num9];
				break;
			}
			default:
				if (obj.fullName.Contains("SCP-939"))
				{
					GameObject[] array5 = GameObject.FindGameObjectsWithTag("SCP_939");
					int num5 = Random.Range(0, array5.Length);
					result = array5[num5];
				}
				else
				{
					GameObject[] array6 = GameObject.FindGameObjectsWithTag("SP_173");
					int num6 = Random.Range(0, array6.Length);
					result = array6[num6];
				}
				break;
			}
			break;
		case Team.MTF:
		{
			GameObject[] array4 = GameObject.FindGameObjectsWithTag((classID != 15) ? "SP_MTF" : "SP_GUARD");
			int num4 = Random.Range(0, array4.Length);
			result = array4[num4];
			break;
		}
		case Team.RSC:
		{
			GameObject[] array3 = GameObject.FindGameObjectsWithTag("SP_RSC");
			int num3 = Random.Range(0, array3.Length);
			result = array3[num3];
			break;
		}
		case Team.CHI:
		{
			GameObject[] array2 = GameObject.FindGameObjectsWithTag("SP_CI");
			int num2 = Random.Range(0, array2.Length);
			result = array2[num2];
			break;
		}
		}
		return result;
	}
}
