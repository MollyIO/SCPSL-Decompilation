using System.Collections;
using UnityEngine;

namespace Dissonance.Demo
{
	public class SpeakerIndicator : MonoBehaviour
	{
		private GameObject _indicator;

		private Light _light;

		private Transform _transform;

		private float _intensity;

		private IDissonancePlayer _player;

		private VoicePlayerState _state;

		private bool IsSpeaking
		{
			get
			{
				return _player.Type == NetworkPlayerType.Remote && _state != null && _state.IsSpeaking;
			}
		}

		private void Start()
		{
			_indicator = Object.Instantiate(Resources.Load<GameObject>("SpeechIndicator"));
			_indicator.transform.SetParent(base.transform);
			_indicator.transform.localPosition = new Vector3(0f, 3f, 0f);
			_light = _indicator.GetComponent<Light>();
			_transform = _indicator.GetComponent<Transform>();
			_player = GetComponent<IDissonancePlayer>();
			StartCoroutine(FindPlayerState());
		}

		private IEnumerator FindPlayerState()
		{
			while (!_player.IsTracking)
			{
				yield return null;
			}
			while (_state == null)
			{
				_state = Object.FindObjectOfType<DissonanceComms>().FindPlayer(_player.PlayerId);
				yield return null;
			}
		}

		private void Update()
		{
			if (IsSpeaking)
			{
				_intensity = Mathf.Max(Mathf.Clamp(Mathf.Pow(_state.Amplitude, 0.175f), 0.25f, 1f), _intensity - Time.deltaTime);
				_indicator.SetActive(true);
			}
			else
			{
				_intensity -= Time.deltaTime * 2f;
				if (_intensity <= 0f)
				{
					_indicator.SetActive(false);
				}
			}
			UpdateLight(_light, _intensity);
			UpdateChildTransform(_transform, _intensity);
		}

		private static void UpdateChildTransform([NotNull] Transform transform, float intensity)
		{
			transform.localScale = new Vector3(intensity, intensity, intensity);
		}

		private static void UpdateLight([NotNull] Light light, float intensity)
		{
			light.intensity = intensity;
		}
	}
}
