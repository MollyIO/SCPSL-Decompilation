using System.Collections;
using System.Collections.Generic;

namespace UnityWebGLSpeechSynthesis
{
	public class EditorProxySpeechSynthesisPlugin : ProxySpeechSynthesisPlugin
	{
		private const string KEY_SPEECH_SYNTHESIS_ENABLED = "ProxySpeechSynthesisEnabled";

		private bool _mHasEditorUpdates;

		private List<IEnumerator> _mPendingRoutines = new List<IEnumerator>();

		protected override void Start()
		{
		}

		protected override void SafeStartCoroutine(string routineName, IEnumerator routine)
		{
			_mPendingRoutines.Add(routine);
		}

		protected override IWWW CreateWWW(string url)
		{
			return new WWWEditMode(url);
		}

		public static bool IsEnabled()
		{
			return false;
		}

		public static void SetEnabled(bool toggle)
		{
		}

		public new static EditorProxySpeechSynthesisPlugin GetInstance()
		{
			return null;
		}

		public void EditorUpdate()
		{
		}

		public void StartEditorUpdates()
		{
			if (!_mHasEditorUpdates)
			{
				_mHasEditorUpdates = true;
				_mPendingRoutines.Clear();
				SafeStartCoroutine("Init", Init());
			}
		}

		public void StopEditorUpdates()
		{
			if (_mHasEditorUpdates)
			{
				_mHasEditorUpdates = false;
				_mPendingRoutines.Clear();
			}
		}
	}
}
