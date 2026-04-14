using System;
using System.Collections.Generic;
using System.Text;
using MEC;
using UnityEngine;
using UnityEngine.UI;

public class ServerListManager : MonoBehaviour
{
	public RectTransform contentParent;

	public RectTransform element;

	public Text loadingText;

	public static ServerListManager singleton;

	private ServerFilters filters;

	private List<GameObject> spawns = new List<GameObject>();

	private void Awake()
	{
		filters = GetComponent<ServerFilters>();
		singleton = this;
	}

	public GameObject AddRecord()
	{
		RectTransform rectTransform = UnityEngine.Object.Instantiate(element);
		rectTransform.SetParent(contentParent);
		rectTransform.localScale = Vector3.one;
		rectTransform.localPosition = Vector3.zero;
		spawns.Add(rectTransform.gameObject);
		contentParent.sizeDelta = Vector2.up * 150f * spawns.Count;
		return rectTransform.gameObject;
	}

	private void OnEnable()
	{
		Refresh();
	}

	public void ApplyNameFilter(string nameFilter)
	{
		filters.nameFilter = nameFilter;
		Refresh();
	}

	public void Refresh()
	{
		Timing.RunCoroutine(_DownloadList(), Segment.FixedUpdate);
	}

	public IEnumerator<float> _DownloadList()
	{
		while (true)
		{
			loadingText.text = TranslationReader.Get("MainMenu", 53);
			foreach (GameObject spawn in spawns)
			{
				UnityEngine.Object.Destroy(spawn);
			}
			spawns.Clear();
			using (WWW www = new WWW(CentralServer.StandardUrl + "lobbylist.php"))
			{
				yield return Timing.WaitUntilDone(www);
				Text text = loadingText;
				string text2 = TranslationReader.Get("MainMenu", 54);
				loadingText.text = text2;
				text.text = text2;
				if (string.IsNullOrEmpty(www.error))
				{
					string[] versions = CustomNetworkManager.CompatibleVersions;
					if (!www.text.Contains("<br>"))
					{
						break;
					}
					string[] results = www.text.Split(new string[1] { "<br>" }, StringSplitOptions.None);
					string[] array = results;
					foreach (string server in array)
					{
						if (!server.Contains(";"))
						{
							continue;
						}
						string[] info = server.Split(';');
						if (info.Length != 5)
						{
							continue;
						}
						try
						{
							string[] array2 = versions;
							foreach (string text3 in array2)
							{
								if (!(Base64Decode(info[2]).Split(new string[1] { ":[:BREAK:]:" }, StringSplitOptions.None)[2] != text3))
								{
									string text4 = Base64Decode(info[2]).Split(new string[1] { ":[:BREAK:]:" }, StringSplitOptions.None)[0];
									if (filters.AllowToSpawn(text4))
									{
										loadingText.text = string.Empty;
										PlayButton component = AddRecord().GetComponent<PlayButton>();
										component.Ip = info[0];
										component.Port = info[1];
										component.Motd.text = text4;
										component.InfoType = Base64Decode(info[2]).Split(new string[1] { ":[:BREAK:]:" }, StringSplitOptions.None)[1];
										component.Players.text = info[3];
									}
								}
							}
						}
						catch
						{
						}
						if (loadingText.text == string.Empty)
						{
							yield return 0f;
						}
					}
					break;
				}
				if (CentralServer.ChangeCentralServer(true))
				{
					continue;
				}
				loadingText.text = www.error + "\nChecked all servers.";
				break;
			}
		}
	}

	private string Base64Decode(string t)
	{
		byte[] bytes = Convert.FromBase64String(t);
		return Encoding.UTF8.GetString(bytes);
	}
}
