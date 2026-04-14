using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarkupImageRequest : MonoBehaviour
{
	[Serializable]
	public class CachedImage
	{
		public Texture texture;

		public string url;
	}

	public static List<CachedImage> cachedImages = new List<CachedImage>();

	public string[] allowedExtensions;

	public int maxSizeInBytes;

	public Texture errorTexture;

	public Image progressImage;

	public Image dimmerImage;

	private RawImage targetImage;

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
			if (num == 1)
			{
				string[] array = allowedExtensions;
				foreach (string text2 in array)
				{
					if (url.ToLower().EndsWith("." + text2.ToLower()))
					{
						return true;
					}
				}
			}
			return false;
		}
		return false;
	}

	public void DownloadImage(string url, Color c)
	{
		targetImage = GetComponent<RawImage>();
		foreach (CachedImage cachedImage in cachedImages)
		{
			if (cachedImage.url == url)
			{
				targetImage.color = c;
				targetImage.texture = cachedImage.texture;
				dimmerImage.enabled = false;
				progressImage.enabled = false;
				return;
			}
		}
		if (VerifyURL(url))
		{
			StopAllCoroutines();
			StartCoroutine(RequestImage(url, c));
			return;
		}
		targetImage.texture = errorTexture;
		dimmerImage.color = Color.clear;
		progressImage.enabled = false;
		Debug.Log("Verification failed");
	}

	private IEnumerator RequestImage(string url, Color col)
	{
		using (WWW www = new WWW(url))
		{
			while (!www.isDone)
			{
				yield return new WaitForEndOfFrame();
				progressImage.fillAmount = www.progress;
				progressImage.transform.Rotate(0f, 0f, Time.deltaTime * -360f);
				if (www.bytesDownloaded > maxSizeInBytes || (www.progress != 0f && (float)www.bytesDownloaded / www.progress > (float)maxSizeInBytes))
				{
					progressImage.enabled = false;
					targetImage.texture = errorTexture;
					dimmerImage.color = Color.clear;
					progressImage.enabled = false;
					Debug.Log("Out of max size");
					yield break;
				}
			}
			if (string.IsNullOrEmpty(www.error))
			{
				progressImage.fillAmount = 1f;
				yield return new WaitForEndOfFrame();
				targetImage.color = col;
				targetImage.texture = www.texture;
				cachedImages.Add(new CachedImage
				{
					texture = targetImage.texture,
					url = www.url
				});
			}
			else
			{
				targetImage.texture = errorTexture;
				dimmerImage.color = Color.clear;
				progressImage.enabled = false;
				Debug.Log("Error: " + www.error);
			}
		}
		for (float i = 0f; i <= 30f; i += 1f)
		{
			dimmerImage.color = Color.Lerp(Color.black, Color.clear, i / 30f);
			progressImage.color = Color.Lerp(Color.white, Color.clear, i / 30f);
			yield return new WaitForFixedUpdate();
		}
		dimmerImage.enabled = false;
		progressImage.enabled = false;
	}
}
