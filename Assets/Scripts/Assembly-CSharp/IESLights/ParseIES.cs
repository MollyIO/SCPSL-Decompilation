using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace IESLights
{
	public static class ParseIES
	{
		private const float SpotlightCutoff = 0.1f;

		public static IESData Parse(string path, NormalizationMode normalizationMode)
		{
			string[] array = File.ReadAllLines(path);
			int lineNumber = 0;
			FindNumberOfAnglesLine(array, ref lineNumber);
			if (lineNumber == array.Length - 1)
			{
				throw new IESParseException("No line containing number of angles found.");
			}
			int numberOfVerticalAngles;
			int numberOfHorizontalAngles;
			PhotometricType photometricType;
			ReadProperties(array, ref lineNumber, out numberOfVerticalAngles, out numberOfHorizontalAngles, out photometricType);
			List<float> verticalAngles = ReadValues(array, numberOfVerticalAngles, ref lineNumber);
			List<float> horizontalAngles = ReadValues(array, numberOfHorizontalAngles, ref lineNumber);
			List<List<float>> list = new List<List<float>>();
			for (int i = 0; i < numberOfHorizontalAngles; i++)
			{
				list.Add(ReadValues(array, numberOfVerticalAngles, ref lineNumber));
			}
			IESData iESData = new IESData();
			iESData.VerticalAngles = verticalAngles;
			iESData.HorizontalAngles = horizontalAngles;
			iESData.CandelaValues = list;
			iESData.PhotometricType = photometricType;
			IESData iESData2 = iESData;
			NormalizeValues(iESData2, normalizationMode == NormalizationMode.Logarithmic);
			if (normalizationMode == NormalizationMode.EqualizeHistogram)
			{
				EqualizeHistogram(iESData2);
			}
			if (photometricType != PhotometricType.TypeA)
			{
				DiscardUnusedVerticalHalf(iESData2);
				SetVerticalAndHorizontalType(iESData2);
				iESData2.HalfSpotlightFov = CalculateHalfSpotFov(iESData2);
			}
			else
			{
				PadToSquare(iESData2);
			}
			return iESData2;
		}

		private static void DiscardUnusedVerticalHalf(IESData iesData)
		{
			if (iesData.VerticalAngles[0] != 0f || iesData.VerticalAngles[iesData.VerticalAngles.Count - 1] != 180f)
			{
				return;
			}
			int i;
			for (i = 0; i < iesData.VerticalAngles.Count && !iesData.NormalizedValues.Any((List<float> slice) => slice[i] > 0.1f); i++)
			{
				if (iesData.VerticalAngles[i] == 90f)
				{
					DiscardBottomHalf(iesData);
					return;
				}
				if (iesData.VerticalAngles[i] > 90f)
				{
					iesData.VerticalAngles[i] = 90f;
					DiscardBottomHalf(iesData);
					return;
				}
			}
			int i2;
			for (i2 = iesData.VerticalAngles.Count - 1; i2 >= 0 && !iesData.NormalizedValues.Any((List<float> slice) => slice[i2] > 0.1f); i2--)
			{
				if (iesData.VerticalAngles[i2] == 90f)
				{
					DiscardTopHalf(iesData);
					break;
				}
				if (iesData.VerticalAngles[i2] < 90f)
				{
					iesData.VerticalAngles[i2] = 90f;
					DiscardTopHalf(iesData);
					break;
				}
			}
		}

		private static void DiscardBottomHalf(IESData iesData)
		{
			int num = 0;
			for (int i = 0; i < iesData.VerticalAngles.Count && iesData.VerticalAngles[i] != 90f; i++)
			{
				num++;
			}
			DiscardHalf(iesData, 0, num);
		}

		private static void DiscardTopHalf(IESData iesData)
		{
			int num = 0;
			for (int i = 0; i < iesData.VerticalAngles.Count; i++)
			{
				if (iesData.VerticalAngles[i] == 90f)
				{
					num = i + 1;
					break;
				}
			}
			int range = iesData.VerticalAngles.Count - num;
			DiscardHalf(iesData, num, range);
		}

		private static void DiscardHalf(IESData iesData, int start, int range)
		{
			iesData.VerticalAngles.RemoveRange(start, range);
			for (int i = 0; i < iesData.CandelaValues.Count; i++)
			{
				iesData.CandelaValues[i].RemoveRange(start, range);
				iesData.NormalizedValues[i].RemoveRange(start, range);
			}
		}

		private static void PadToSquare(IESData iesData)
		{
			if (Mathf.Abs(iesData.HorizontalAngles.Count - iesData.VerticalAngles.Count) > 1)
			{
				int num = Mathf.Max(iesData.HorizontalAngles.Count, iesData.VerticalAngles.Count);
				if (iesData.HorizontalAngles.Count < num)
				{
					PadHorizontal(iesData, num);
				}
				else
				{
					PadVertical(iesData, num);
				}
			}
		}

		private static void PadHorizontal(IESData iesData, int longestSide)
		{
			int num = longestSide - iesData.HorizontalAngles.Count;
			int num2 = num / 2;
			int padBeforeAmount = (iesData.PadAfterAmount = num2);
			iesData.PadBeforeAmount = padBeforeAmount;
			List<float> item = Enumerable.Repeat(0f, iesData.VerticalAngles.Count).ToList();
			for (int i = 0; i < num2; i++)
			{
				iesData.NormalizedValues.Insert(0, item);
			}
			for (int j = 0; j < num - num2; j++)
			{
				iesData.NormalizedValues.Add(item);
			}
		}

		private static void PadVertical(IESData iesData, int longestSide)
		{
			int num = longestSide - iesData.VerticalAngles.Count;
			if (Mathf.Sign(iesData.VerticalAngles[0]) == (float)Math.Sign(iesData.VerticalAngles[iesData.VerticalAngles.Count - 1]))
			{
				int num2 = (iesData.PadBeforeAmount = num / 2);
				iesData.PadAfterAmount = num - num2;
				{
					foreach (List<float> normalizedValue in iesData.NormalizedValues)
					{
						normalizedValue.InsertRange(0, new List<float>(new float[num2]));
						normalizedValue.AddRange(new List<float>(new float[num - num2]));
					}
					return;
				}
			}
			int num4 = longestSide / 2 - iesData.VerticalAngles.Count((float v) => v >= 0f);
			if (iesData.VerticalAngles[0] < 0f)
			{
				iesData.PadBeforeAmount = num - num4;
				iesData.PadAfterAmount = num4;
				{
					foreach (List<float> normalizedValue2 in iesData.NormalizedValues)
					{
						normalizedValue2.InsertRange(0, new List<float>(new float[num - num4]));
						normalizedValue2.AddRange(new List<float>(new float[num4]));
					}
					return;
				}
			}
			iesData.PadBeforeAmount = num4;
			iesData.PadAfterAmount = num - num4;
			foreach (List<float> normalizedValue3 in iesData.NormalizedValues)
			{
				normalizedValue3.InsertRange(0, new List<float>(new float[num4]));
				normalizedValue3.AddRange(new List<float>(new float[num - num4]));
			}
		}

		private static void SetVerticalAndHorizontalType(IESData iesData)
		{
			if ((iesData.VerticalAngles[0] == 0f && iesData.VerticalAngles[iesData.VerticalAngles.Count - 1] == 90f) || (iesData.VerticalAngles[0] == -90f && iesData.VerticalAngles[iesData.VerticalAngles.Count - 1] == 0f))
			{
				iesData.VerticalType = VerticalType.Bottom;
			}
			else if (iesData.VerticalAngles[iesData.VerticalAngles.Count - 1] == 180f && iesData.VerticalAngles[0] == 90f)
			{
				iesData.VerticalType = VerticalType.Top;
			}
			else
			{
				iesData.VerticalType = VerticalType.Full;
			}
			if (iesData.HorizontalAngles.Count == 1)
			{
				iesData.HorizontalType = HorizontalType.None;
				return;
			}
			if (iesData.HorizontalAngles[iesData.HorizontalAngles.Count - 1] - iesData.HorizontalAngles[0] == 90f)
			{
				iesData.HorizontalType = HorizontalType.Quadrant;
				return;
			}
			if (iesData.HorizontalAngles[iesData.HorizontalAngles.Count - 1] - iesData.HorizontalAngles[0] == 180f)
			{
				iesData.HorizontalType = HorizontalType.Half;
				return;
			}
			iesData.HorizontalType = HorizontalType.Full;
			if (iesData.HorizontalAngles[iesData.HorizontalAngles.Count - 1] != 360f)
			{
				StitchHorizontalAssymetry(iesData);
			}
		}

		private static void StitchHorizontalAssymetry(IESData iesData)
		{
			iesData.HorizontalAngles.Add(360f);
			iesData.CandelaValues.Add(iesData.CandelaValues[0]);
			iesData.NormalizedValues.Add(iesData.NormalizedValues[0]);
		}

		private static float CalculateHalfSpotFov(IESData iesData)
		{
			if (iesData.VerticalType == VerticalType.Bottom && iesData.VerticalAngles[0] == 0f)
			{
				return CalculateHalfSpotlightFovForBottomHalf(iesData);
			}
			if (iesData.VerticalType == VerticalType.Top || (iesData.VerticalType == VerticalType.Bottom && iesData.VerticalAngles[0] == -90f))
			{
				return CalculateHalfSpotlightFovForTopHalf(iesData);
			}
			return -1f;
		}

		private static float CalculateHalfSpotlightFovForBottomHalf(IESData iesData)
		{
			for (int num = iesData.VerticalAngles.Count - 1; num >= 0; num--)
			{
				for (int i = 0; i < iesData.NormalizedValues.Count; i++)
				{
					if (iesData.NormalizedValues[i][num] >= 0.1f)
					{
						if (num < iesData.VerticalAngles.Count - 1)
						{
							return iesData.VerticalAngles[num + 1];
						}
						return iesData.VerticalAngles[num];
					}
				}
			}
			return 0f;
		}

		private static float CalculateHalfSpotlightFovForTopHalf(IESData iesData)
		{
			for (int i = 0; i < iesData.VerticalAngles.Count; i++)
			{
				for (int j = 0; j < iesData.NormalizedValues.Count; j++)
				{
					if (!(iesData.NormalizedValues[j][i] >= 0.1f))
					{
						continue;
					}
					if (iesData.VerticalType == VerticalType.Top)
					{
						if (i > 0)
						{
							return 180f - iesData.VerticalAngles[i - 1];
						}
						return 180f - iesData.VerticalAngles[i];
					}
					if (i > 0)
					{
						return 0f - iesData.VerticalAngles[i - 1];
					}
					return 0f - iesData.VerticalAngles[i];
				}
			}
			return 0f;
		}

		private static void NormalizeValues(IESData iesData, bool squashHistogram)
		{
			iesData.NormalizedValues = new List<List<float>>();
			float num = iesData.CandelaValues.SelectMany((List<float> v) => v).Max();
			if (squashHistogram)
			{
				num = Mathf.Log(num);
			}
			foreach (List<float> candelaValue in iesData.CandelaValues)
			{
				List<float> list = new List<float>();
				if (squashHistogram)
				{
					for (int num2 = 0; num2 < candelaValue.Count; num2++)
					{
						list.Add(Mathf.Log(candelaValue[num2]));
					}
				}
				else
				{
					list.AddRange(candelaValue);
				}
				for (int num3 = 0; num3 < candelaValue.Count; num3++)
				{
					list[num3] /= num;
					list[num3] = Mathf.Clamp01(list[num3]);
				}
				iesData.NormalizedValues.Add(list);
			}
		}

		private static void EqualizeHistogram(IESData iesData)
		{
			int num = Mathf.Min((int)iesData.CandelaValues.SelectMany((List<float> v) => v).Max(), 10000);
			float[] array = new float[num];
			float[] array2 = new float[num];
			foreach (List<float> normalizedValue in iesData.NormalizedValues)
			{
				foreach (float item in normalizedValue)
				{
					float num2 = item;
					array[(int)(num2 * (float)(num - 1))] += 1f;
				}
			}
			float num3 = iesData.HorizontalAngles.Count * iesData.VerticalAngles.Count;
			for (int num4 = 0; num4 < array.Length; num4++)
			{
				array[num4] /= num3;
			}
			for (int num5 = 0; num5 < num; num5++)
			{
				array2[num5] = array.Take(num5 + 1).Sum();
			}
			foreach (List<float> normalizedValue2 in iesData.NormalizedValues)
			{
				for (int num6 = 0; num6 < normalizedValue2.Count; num6++)
				{
					int num7 = (int)(normalizedValue2[num6] * (float)(num - 1));
					normalizedValue2[num6] = array2[num7] * (float)(num - 1) / (float)num;
				}
			}
		}

		private static void FindNumberOfAnglesLine(string[] lines, ref int lineNumber)
		{
			int i;
			for (i = 0; i < lines.Length; i++)
			{
				if (lines[i].Trim().StartsWith("TILT"))
				{
					try
					{
						i = ((!(lines[i].Split('=')[1].Trim() != "NONE")) ? (i + 1) : (i + 5));
					}
					catch (ArgumentOutOfRangeException)
					{
						throw new IESParseException("No TILT line present.");
					}
					break;
				}
			}
			lineNumber = i;
		}

		private static void ReadProperties(string[] lines, ref int lineNumber, out int numberOfVerticalAngles, out int numberOfHorizontalAngles, out PhotometricType photometricType)
		{
			List<float> list = ReadValues(lines, 13, ref lineNumber);
			numberOfVerticalAngles = (int)list[3];
			numberOfHorizontalAngles = (int)list[4];
			photometricType = (PhotometricType)list[5];
		}

		private static List<float> ReadValues(string[] lines, int numberOfValuesToFind, ref int lineNumber)
		{
			List<float> list = new List<float>();
			while (list.Count < numberOfValuesToFind)
			{
				if (lineNumber >= lines.Length)
				{
					throw new IESParseException("Reached end of file before the given number of values was read.");
				}
				char[] separator = null;
				if (lines[lineNumber].Contains(","))
				{
					separator = new char[1] { ',' };
				}
				string[] array = lines[lineNumber].Split(separator, StringSplitOptions.RemoveEmptyEntries);
				string[] array2 = array;
				foreach (string s in array2)
				{
					try
					{
						list.Add(float.Parse(s));
					}
					catch (Exception inner)
					{
						throw new IESParseException("Invalid value declaration.", inner);
					}
				}
				lineNumber++;
			}
			return list;
		}
	}
}
