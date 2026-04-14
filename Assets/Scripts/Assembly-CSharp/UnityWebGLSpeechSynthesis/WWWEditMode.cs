using System;
using System.IO;
using System.Net;

namespace UnityWebGLSpeechSynthesis
{
	public class WWWEditMode : IWWW
	{
		private string _mError;

		private bool _mIsDone;

		private string _mText;

		public WWWEditMode(string url)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.Timeout = 3000;
			httpWebRequest.BeginGetResponse(HandleBeginGetResponse, httpWebRequest);
		}

		private void HandleBeginGetResponse(IAsyncResult asyncResult)
		{
			try
			{
				HttpWebRequest httpWebRequest = (HttpWebRequest)asyncResult.AsyncState;
				HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.EndGetResponse(asyncResult);
				using (Stream stream = httpWebResponse.GetResponseStream())
				{
					using (StreamReader streamReader = new StreamReader(stream))
					{
						_mText = streamReader.ReadToEnd();
						_mIsDone = true;
					}
				}
				httpWebResponse.Close();
			}
			catch (WebException arg)
			{
				_mError = string.Format("Request exception={0}", arg);
			}
		}

		public bool IsDone()
		{
			return _mIsDone;
		}

		public string GetError()
		{
			return _mError;
		}

		public string GetText()
		{
			return _mText;
		}

		public void Dispose()
		{
		}
	}
}
