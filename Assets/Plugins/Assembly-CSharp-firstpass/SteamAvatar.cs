using Facepunch.Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class SteamAvatar : MonoBehaviour
{
	public ulong SteamId;

	public Friends.AvatarSize Size;

	public Texture FallbackTexture;

	private void Start()
	{
		Fetch();
	}

	public void Fetch()
	{
		if (SteamId != 0)
		{
			if (Client.Instance == null)
			{
				ApplyTexture(FallbackTexture);
			}
			else
			{
				Client.Instance.Friends.GetAvatar(Size, SteamId, OnImage);
			}
		}
	}

	private void OnImage(Facepunch.Steamworks.Image image)
	{
		if (image == null)
		{
			ApplyTexture(FallbackTexture);
			return;
		}
		Texture2D texture2D = new Texture2D(image.Width, image.Height);
		for (int i = 0; i < image.Width; i++)
		{
			for (int j = 0; j < image.Height; j++)
			{
				Facepunch.Steamworks.Color pixel = image.GetPixel(i, j);
				texture2D.SetPixel(i, image.Height - j, new UnityEngine.Color((float)(int)pixel.r / 255f, (float)(int)pixel.g / 255f, (float)(int)pixel.b / 255f, (float)(int)pixel.a / 255f));
			}
		}
		texture2D.Apply();
		ApplyTexture(texture2D);
	}

	private void ApplyTexture(Texture texture)
	{
		RawImage component = GetComponent<RawImage>();
		if (component != null)
		{
			component.texture = texture;
		}
	}
}
