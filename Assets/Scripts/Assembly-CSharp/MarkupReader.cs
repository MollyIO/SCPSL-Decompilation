using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MEC;
using UnityEngine;

public class MarkupReader : MonoBehaviour
{
	[Serializable]
	public class TagStyleRelation
	{
		public string tag;

		public MarkupStyle style;

		public string sourceURL;
	}

	public List<TagStyleRelation> relations = new List<TagStyleRelation>();

	public static MarkupReader singleton;

	[CompilerGenerated]
	private static Dictionary<string, int> _003C_003Ef__switch_0024map7;

	private void Awake()
	{
		singleton = this;
	}

	public bool AddStyleFromURL(string url)
	{
		if (VerifyURL(url))
		{
			Timing.RunCoroutine(_DownloadStyle(url));
			return true;
		}
		return false;
	}

	private IEnumerator<float> _DownloadStyle(string url)
	{
		using (WWW www = new WWW(url))
		{
			yield return Timing.WaitUntilDone(www);
			if (string.IsNullOrEmpty(www.error))
			{
				string text = www.text;
				LoadStyle(text, url);
			}
			else
			{
				Debug.LogError("Error on downloading the style: " + www.error);
			}
		}
	}

	private void LoadStyle(string _style, string _url)
	{
		string text = _style;
		for (int i = 0; i < 1000; i++)
		{
			if (!text.Contains("<"))
			{
				break;
			}
			if (!text.Contains(">"))
			{
				break;
			}
			string text2 = text;
			text2 = text2.Remove(0, text2.IndexOf('<') + 1);
			string text3 = text2.Remove(text2.IndexOf('>'));
			text2 = text2.Remove(0, text2.IndexOf('{') + 1);
			text2 = text2.Remove(text2.IndexOf('}'));
			string[] array = text2.Split(';');
			foreach (string text4 in array)
			{
				if (!text4.Contains(":"))
				{
					continue;
				}
				string text5 = string.Empty;
				string text6 = string.Empty;
				bool flag = false;
				for (int k = 0; k < text4.Length; k++)
				{
					if (text4[k] != ' ' && (text4[k] != ':' || flag))
					{
						if (flag)
						{
							text6 += text4[k];
						}
						else
						{
							text5 += text4[k];
						}
					}
					if (text4[k] == ':')
					{
						flag = true;
					}
				}
				if (text5.Contains(Environment.NewLine))
				{
					text5 = text5.Remove(text5.IndexOf(Environment.NewLine), 1);
				}
				if (text6.Contains(Environment.NewLine))
				{
					text6 = text5.Remove(text5.IndexOf(Environment.NewLine), 1);
				}
				OverrideTagStyle(text3.ToLower(), text5.ToLower(), text6, _url);
			}
			text = text.Remove(0, text.IndexOf('}') + 1);
		}
	}

	private void OverrideTagStyle(string _tag, string _var, string _value, string _url)
	{
		int num = -1;
		for (int i = 0; i < relations.Count; i++)
		{
			if (relations[i].tag == _tag)
			{
				num = i;
			}
		}
		if (num == -1)
		{
			relations.Add(new TagStyleRelation
			{
				tag = _tag,
				style = new MarkupStyle(),
				sourceURL = string.Empty
			});
			num = relations.Count - 1;
		}
		relations[num].sourceURL = _url;
		while (!Regex.IsMatch(_var[0].ToString(), "^[a-zA-Z]+$"))
		{
			_var = _var.Remove(0, 1);
		}
		if (_var == null)
		{
			return;
		}
		if (_003C_003Ef__switch_0024map7 == null)
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>(13);
			dictionary.Add("background-color", 0);
			dictionary.Add("outline-color", 1);
			dictionary.Add("text-color", 2);
			dictionary.Add("text-outline-color", 3);
			dictionary.Add("image-color", 4);
			dictionary.Add("image-url", 5);
			dictionary.Add("outline-size", 6);
			dictionary.Add("text-outline-size", 7);
			dictionary.Add("text-font-family", 8);
			dictionary.Add("text-font-size", 9);
			dictionary.Add("text-content", 10);
			dictionary.Add("text-align", 11);
			dictionary.Add("raycast-target", 12);
			_003C_003Ef__switch_0024map7 = dictionary;
		}
		int value;
		if (!_003C_003Ef__switch_0024map7.TryGetValue(_var, out value))
		{
			return;
		}
		switch (value)
		{
		case 0:
		{
			Color color2;
			if (ColorUtility.TryParseHtmlString(_value, out color2))
			{
				relations[num].style.mainColor = color2;
			}
			break;
		}
		case 1:
		{
			Color color4;
			if (ColorUtility.TryParseHtmlString(_value, out color4))
			{
				relations[num].style.outlineColor = color4;
			}
			break;
		}
		case 2:
		{
			Color color5;
			if (ColorUtility.TryParseHtmlString(_value, out color5))
			{
				relations[num].style.textColor = color5;
			}
			break;
		}
		case 3:
		{
			Color color3;
			if (ColorUtility.TryParseHtmlString(_value, out color3))
			{
				relations[num].style.textOutlineColor = color3;
			}
			break;
		}
		case 4:
		{
			Color color;
			if (ColorUtility.TryParseHtmlString(_value, out color))
			{
				relations[num].style.imageColor = color;
			}
			break;
		}
		case 5:
			relations[num].style.imageUrl = _value;
			break;
		case 6:
		{
			float result4;
			if (float.TryParse(_value, out result4))
			{
				relations[num].style.outlineSize = result4;
			}
			break;
		}
		case 7:
		{
			float result3;
			if (float.TryParse(_value, out result3))
			{
				relations[num].style.textOutlineSize = result3;
			}
			break;
		}
		case 8:
		{
			int result2;
			if (int.TryParse(_value, out result2))
			{
				relations[num].style.fontID = result2;
			}
			break;
		}
		case 9:
		{
			int result;
			if (int.TryParse(_value, out result))
			{
				relations[num].style.fontSize = result;
			}
			break;
		}
		case 10:
			relations[num].style.textContent = _value;
			break;
		case 11:
		{
			string[] names = Enum.GetNames(typeof(TextAnchor));
			foreach (string text in names)
			{
				if (text.ToLower() == _value.ToLower())
				{
					relations[num].style.textAlignment = (TextAnchor)Enum.Parse(typeof(TextAnchor), _value, true);
				}
			}
			break;
		}
		case 12:
			relations[num].style.raycast = _value.ToLower() == "true";
			break;
		}
	}

	public bool VerifyURL(string url)
	{
		if (url.StartsWith("https://"))
		{
			url = url.Remove(0, 8);
		}
		if (url.StartsWith("http://"))
		{
			url = url.Remove(0, 7);
		}
		if (url.Contains("/"))
		{
			url = url.Remove(0, url.IndexOf("/"));
			int num = 0;
			string text = url;
			foreach (char c in text)
			{
				if (c == '.')
				{
					num++;
				}
			}
			if (num == 1 && url.ToLower().EndsWith(".txt"))
			{
				return true;
			}
			return false;
		}
		return false;
	}
}
