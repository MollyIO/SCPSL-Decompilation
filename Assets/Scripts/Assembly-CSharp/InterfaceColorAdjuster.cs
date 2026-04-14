using UnityEngine;
using UnityEngine.UI;

public class InterfaceColorAdjuster : MonoBehaviour
{
	public InventoryDisplay inventoryDisplay;

	public MyMicrophoneIndicator microphoneIndicator;

	public Graphic[] graphicsToChange;

	private void Awake()
	{
		PlayerList.ica = this;
	}

	public void ChangeColor(Color color)
	{
		microphoneIndicator.SetColor(color);
		inventoryDisplay.highlightColor = Color.Lerp(color, new Color(1f, 1f, 1f, 0f), 0.6f);
		inventoryDisplay.selectedColor = new Color(color.r, color.g, color.b, 0.5f);
		ItemDescriptionValue[] itemDescriptions = inventoryDisplay.itemDescriptions;
		foreach (ItemDescriptionValue itemDescriptionValue in itemDescriptions)
		{
			itemDescriptionValue.title.color = color;
		}
		Graphic[] array = graphicsToChange;
		foreach (Graphic graphic in array)
		{
			if (graphic != null)
			{
				Color color2 = new Color(color.r, color.g, color.b, graphic.color.a);
				graphic.color = color2;
			}
		}
		PlayerList.UpdateColors();
	}
}
