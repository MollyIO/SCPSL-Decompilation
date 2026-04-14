using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NineTailedFoxAnnouncer : MonoBehaviour
{
	[Serializable]
	public class VoiceLine
	{
		public string apiName;

		public AudioClip clip;

		public float length;

		public string GetName()
		{
			return apiName;
		}
	}

	private class ItemEqualityComparer : IEqualityComparer<VoiceLine>
	{
		public bool Equals(VoiceLine x, VoiceLine y)
		{
			return x != null && x.clip != null && x.clip == y.clip;
		}

		public int GetHashCode(VoiceLine obj)
		{
			return obj.clip.GetHashCode();
		}
	}

	public VoiceLine[] voiceLines;

	public AudioClip[] backgroundLines;

	public AudioClip suffixPluralStandard;

	public AudioClip suffixPluralException;

	public AudioClip suffixPastStandard;

	public AudioClip suffixPastException;

	public List<VoiceLine> queue = new List<VoiceLine>();

	public AudioSource speakerSource;

	public AudioSource backgroundSource;

	public bool isFree = true;

	public static NineTailedFoxAnnouncer singleton;

	public void AnnounceNtfEntrance(int _scpsLeft, int _mtfNumber, char _mtfLetter)
	{
		string empty = string.Empty;
		string[] array = new string[2]
		{
			_mtfNumber.ToString("00")[0].ToString(),
			_mtfNumber.ToString("00")[1].ToString()
		};
		empty += "MTFUNIT EPSILON 11 DESIGNATED ";
		string text = empty;
		empty = text + "NATO_" + _mtfLetter + " ";
		empty = empty + array[0] + array[1] + " ";
		empty += "ENTERED ALLREMAINING ";
		empty += ((_scpsLeft > 0) ? ("AWATINGRECONTAINMENT " + _scpsLeft + ((_scpsLeft != 1) ? " SCPSUBJECTS" : " SCPSUBJECT")) : "NOSCPSLEFT");
		AddPhraseToQueue(empty);
	}

	public string ConvertNumber(int num)
	{
		if (num == 0)
		{
			return " 0 ";
		}
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		while ((float)num / 1000f >= 1f)
		{
			num2++;
			num -= 1000;
		}
		while ((float)num / 100f >= 1f)
		{
			num3++;
			num -= 100;
		}
		if (num >= 20)
		{
			while ((float)num / 10f >= 1f)
			{
				num4++;
				num -= 10;
			}
		}
		string text = string.Empty;
		if (num2 > 0)
		{
			text = text + ConvertNumber(num2) + " thousand ";
		}
		if (num3 > 0)
		{
			text = text + num3 + " hundred ";
		}
		if (num3 + num2 > 0 && (num > 0 || num4 > 0))
		{
			text += " and ";
		}
		if (num4 > 0)
		{
			text = text + num4 + "0 ";
		}
		if (num > 0)
		{
			text = text + num + " ";
		}
		return text;
	}

	public void AnnounceScpKill(string scpNumber, CharacterClassManager executioner)
	{
		string text = string.Empty;
		try
		{
			text += "SCP ";
			if (scpNumber.Contains("-"))
			{
				string text2 = scpNumber.Split('-')[1];
				foreach (char c in text2)
				{
					text = text + c + " ";
				}
			}
			text += "CONTAINEDSUCCESSFULLY CONTAINMENTUNIT ";
			if (executioner == null || executioner.curClass < 0 || executioner.klasy[executioner.curClass].team != Team.MTF)
			{
				text += "UNKNOWN";
			}
			else
			{
				string text3 = NineTailedFoxUnits.host.list[executioner.ntfUnit];
				char c2 = text3[0];
				string text4 = int.Parse(text3.Split('-')[1]).ToString("00");
				string text5 = text;
				text = text5 + "NATO_" + c2 + " " + text4[0] + string.Empty + text4[1];
			}
		}
		catch
		{
			Debug.Log("Error: " + text);
		}
		AddPhraseToQueue(text);
	}

	private float CalculateDuration(string tts, bool rawNumber = false)
	{
		float num = 0f;
		float num2 = 1f;
		string[] array = tts.Split(' ');
		string[] array2 = array;
		foreach (string text in array2)
		{
			float result;
			if (text.ToUpper().StartsWith("PITCH_") && float.TryParse(text.Remove(0, 6), out result))
			{
				num2 = result;
				continue;
			}
			if (float.TryParse(text, out result) && !rawNumber)
			{
				num += CalculateDuration(ConvertNumber((int)result), true);
				continue;
			}
			bool flag = false;
			VoiceLine[] array3 = voiceLines;
			foreach (VoiceLine voiceLine in array3)
			{
				if (text.ToUpper() == voiceLine.apiName.ToUpper())
				{
					flag = true;
					num += voiceLine.length / num2;
				}
			}
			if (flag || text.Length <= 3)
			{
				continue;
			}
			for (int k = 1; k < 3; k++)
			{
				VoiceLine[] array4 = voiceLines;
				foreach (VoiceLine voiceLine2 in array4)
				{
					if (text.ToUpper().Remove(text.Length - k) == voiceLine2.apiName.ToUpper())
					{
						num += voiceLine2.length / num2;
					}
				}
			}
		}
		return num;
	}

	public void AddPhraseToQueue(string tts, bool rawNumber = false, bool makeHold = false)
	{
		string[] array = tts.Split(' ');
		if (!rawNumber)
		{
			float num = CalculateDuration(tts);
			int num2 = 0;
			for (int i = 0; i < backgroundLines.Length - 1; i++)
			{
				if ((float)i < num)
				{
					num2 = i + 1;
				}
			}
			queue.Add(new VoiceLine
			{
				apiName = "BG_BACKGROUND",
				clip = backgroundLines[num2],
				length = 2.5f
			});
		}
		float num3 = 1f;
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text.ToUpper().StartsWith("PITCH_"))
			{
				queue.Add(new VoiceLine
				{
					apiName = text.ToUpper()
				});
				continue;
			}
			float result;
			if (!rawNumber && float.TryParse(text, out result))
			{
				AddPhraseToQueue(ConvertNumber((int)result), true);
				continue;
			}
			bool flag = false;
			VoiceLine[] array3 = voiceLines;
			foreach (VoiceLine voiceLine in array3)
			{
				if (text.ToUpper() == voiceLine.apiName.ToUpper())
				{
					queue.Add(new VoiceLine
					{
						apiName = voiceLine.apiName,
						clip = voiceLine.clip,
						length = voiceLine.length / num3
					});
					flag = true;
				}
			}
			if (flag || text.Length <= 3)
			{
				continue;
			}
			VoiceLine voiceLine2 = null;
			for (int l = 1; l < 3; l++)
			{
				VoiceLine[] array4 = voiceLines;
				foreach (VoiceLine voiceLine3 in array4)
				{
					if (text.ToUpper().Remove(text.Length - l) == voiceLine3.apiName.ToUpper())
					{
						VoiceLine voiceLine4 = new VoiceLine();
						voiceLine4.apiName = voiceLine3.apiName;
						voiceLine4.clip = voiceLine3.clip;
						voiceLine4.length = voiceLine3.length / num3;
						voiceLine2 = voiceLine4;
					}
				}
			}
			if (voiceLine2 != null)
			{
				AudioClip audioClip = ((!text.ToUpper().EndsWith("TED") && !text.ToUpper().EndsWith("DED")) ? (text.ToUpper().EndsWith("D") ? suffixPastStandard : ((!voiceLine2.apiName.EndsWith("S") && !voiceLine2.apiName.EndsWith("SH") && !voiceLine2.apiName.EndsWith("CH") && !voiceLine2.apiName.EndsWith("X") && !voiceLine2.apiName.EndsWith("Z")) ? suffixPluralStandard : suffixPluralException)) : suffixPastException);
				queue.Add(new VoiceLine
				{
					apiName = voiceLine2.apiName,
					clip = voiceLine2.clip,
					length = (voiceLine2.length - 0.06f) / num3
				});
				queue.Add(new VoiceLine
				{
					apiName = "SUFFIX_" + audioClip.name,
					clip = audioClip,
					length = audioClip.length / num3
				});
			}
		}
		if (!rawNumber)
		{
			for (int n = 0; n < ((!makeHold) ? 1 : 3); n++)
			{
				queue.Add(new VoiceLine
				{
					apiName = "END_OF_MESSAGE"
				});
			}
		}
	}

	private IEnumerator Start()
	{
		singleton = this;
		float speed = 1f;
		while (this != null)
		{
			if (queue.Count == 0)
			{
				speed = 1f;
				yield return new WaitForEndOfFrame();
				isFree = true;
				continue;
			}
			isFree = false;
			if (queue[0].apiName == "END_OF_MESSAGE")
			{
				speakerSource.pitch = 1f;
				yield return new WaitForSeconds(4f);
				queue.RemoveAt(0);
				continue;
			}
			bool isBackground = queue[0].apiName.StartsWith("BG_");
			bool isSuffix = queue[0].apiName.StartsWith("SUFFIX_");
			float num;
			if (queue[0].clip != null)
			{
				if (isBackground)
				{
					backgroundSource.PlayOneShot(queue[0].clip);
				}
				else
				{
					if (isSuffix)
					{
						speakerSource.Stop();
					}
					speakerSource.PlayOneShot(queue[0].clip);
				}
			}
			else if (queue[0].apiName.ToUpper().StartsWith("PITCH_") && float.TryParse(queue[0].apiName.Remove(0, 6), out num))
			{
				speed = num;
				speakerSource.pitch = speed;
			}
			yield return new WaitForSeconds(queue[0].length / speed);
			queue.RemoveAt(0);
		}
	}
}
