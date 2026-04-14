using UnityEngine;
using UnityEngine.UI;

public class BroadcastAssigner : MonoBehaviour
{
	public static bool Displaying;

	public static bool MessageDisplayed;

	public static float TimeLeft;

	public Text Display;

	public Font Normal;

	public Font MonoSpaced;

	private void Start()
	{
		Displaying = false;
		MessageDisplayed = false;
		TimeLeft = 0f;
		Display.enabled = false;
		Display.text = string.Empty;
	}

	private void Update()
	{
		if (!Displaying)
		{
			return;
		}
		if (MessageDisplayed)
		{
			TimeLeft -= Time.deltaTime;
			if (!(TimeLeft <= 0f))
			{
				return;
			}
			MessageDisplayed = false;
		}
		if (!MessageDisplayed && Broadcast.Messages.Count == 0)
		{
			Displaying = false;
			Display.enabled = false;
			Display.text = string.Empty;
			TimeLeft = 0f;
		}
		else
		{
			BroadcastMessage broadcastMessage = Broadcast.Messages.Dequeue();
			MessageDisplayed = true;
			TimeLeft = broadcastMessage.Time;
			Display.text = broadcastMessage.Text;
			Display.font = ((!broadcastMessage.MonoSpaced) ? Normal : MonoSpaced);
			Display.enabled = true;
		}
	}
}
