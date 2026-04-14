using System;
using System.Collections.Generic;
using Dissonance.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace Dissonance.Demo
{
	public class ChatLogController : MonoBehaviour
	{
		private class ChatLogEntry
		{
			private readonly Text _txt;

			private readonly RectTransform _transform;

			private float _transitionProgress;

			[NotNull]
			public RectTransform Transform
			{
				get
				{
					return _transform;
				}
			}

			[NotNull]
			public GameObject Object
			{
				get
				{
					return _txt.gameObject;
				}
			}

			public bool IsTransitioningOut { get; private set; }

			public bool IsTransitionComplete { get; private set; }

			public ChatLogEntry([NotNull] Text txt)
			{
				_txt = txt;
				_transform = txt.rectTransform;
			}

			public void FadeOut()
			{
				IsTransitioningOut = true;
			}

			public void Update()
			{
				if (IsTransitioningOut)
				{
					Color color = _txt.color;
					_transitionProgress = Mathf.Clamp(_transitionProgress + Time.deltaTime, 0f, 1f);
					_txt.color = Color.Lerp(new Color(color.r, color.g, color.b, 1f), new Color(color.r, color.g, color.b, 0f), _transitionProgress);
					if (_transitionProgress >= 1f)
					{
						IsTransitionComplete = true;
					}
				}
			}
		}

		public DissonanceComms Comms;

		private GameObject _textPrototype;

		private CanvasGroup _canvas;

		private float _heightLimit;

		private readonly Queue<ChatLogEntry> _entries = new Queue<ChatLogEntry>();

		private DateTime _fadeOutStartTime;

		public bool ForceShow { get; set; }

		public void Start()
		{
			Comms = Comms ?? UnityEngine.Object.FindObjectOfType<DissonanceComms>();
			_textPrototype = Resources.Load<GameObject>("LogTextPrototype");
			_canvas = GetComponent<CanvasGroup>();
			_heightLimit = base.gameObject.GetComponent<RectTransform>().rect.height - 20f;
			if (Comms != null)
			{
				Comms.Text.MessageReceived += OnMessageReceived;
			}
		}

		private void OnMessageReceived(TextMessage message)
		{
			if (!(Comms != null) || !(message.Sender == Comms.LocalPlayerName))
			{
				string message2 = string.Format("{0} ({1}): {2}", message.Sender.Substring(0, Math.Min(8, message.Sender.Length)), (message.RecipientType != ChannelType.Room) ? "Whisper" : message.Recipient, message.Message);
				AddMessage(message2, new Color(0.19f, 0.19f, 0.19f));
			}
		}

		public void AddMessage(string message, Color color)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_textPrototype, base.gameObject.transform);
			Text component = gameObject.GetComponent<Text>();
			component.text = message;
			component.color = color;
			component.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, component.preferredHeight);
			component.GetComponent<RectTransform>().anchoredPosition += new Vector2(0f, 3f);
			_entries.Enqueue(new ChatLogEntry(component));
			foreach (ChatLogEntry entry in _entries)
			{
				entry.Transform.anchoredPosition += new Vector2(0f, component.preferredHeight);
				if (entry.Transform.anchoredPosition.y > _heightLimit && !entry.IsTransitioningOut)
				{
					entry.FadeOut();
				}
			}
			ShowFor(TimeSpan.FromSeconds(3.0));
		}

		public void Update()
		{
			if (ForceShow)
			{
				_canvas.alpha = 1f;
				_fadeOutStartTime = DateTime.UtcNow + TimeSpan.FromSeconds(4.0);
			}
			else if (_fadeOutStartTime < DateTime.UtcNow)
			{
				float num = (float)(DateTime.UtcNow - _fadeOutStartTime).TotalSeconds / 2f;
				if (num > 0f)
				{
					float alpha = (2f - Mathf.Clamp(num, 0f, 2f)) / 2f;
					_canvas.alpha = alpha;
				}
			}
			else if (_fadeOutStartTime > DateTime.UtcNow)
			{
				_canvas.alpha = 1f;
			}
			while (_entries.Count > 0 && _entries.Peek().IsTransitionComplete)
			{
				UnityEngine.Object.Destroy(_entries.Dequeue().Object);
			}
			foreach (ChatLogEntry entry in _entries)
			{
				if (entry.IsTransitioningOut)
				{
					entry.Update();
					continue;
				}
				break;
			}
		}

		public void ShowFor(TimeSpan time)
		{
			_fadeOutStartTime = DateTime.UtcNow + time;
		}
	}
}
