using UnityEngine;

namespace UnityWebGLSpeechSynthesis
{
	public class WWWPlayMode : IWWW
	{
		private WWW _mWWW;

		public WWWPlayMode(string url)
		{
			_mWWW = new WWW(url);
		}

		public bool IsDone()
		{
			return _mWWW.isDone;
		}

		public string GetError()
		{
			return _mWWW.error;
		}

		public string GetText()
		{
			return _mWWW.text;
		}

		public void Dispose()
		{
			_mWWW.Dispose();
		}
	}
}
