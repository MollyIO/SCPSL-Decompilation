using System.IO;
using System.Net;
using System.Text;

public class HttpQuery
{
	public static string Get(string url)
	{
		WebRequest webRequest = WebRequest.Create(url);
		ServicePointManager.Expect100Continue = true;
		((HttpWebRequest)webRequest).UserAgent = "SCP SL";
		webRequest.Method = "GET";
		webRequest.ContentType = "application/x-www-form-urlencoded";
		WebResponse response = webRequest.GetResponse();
		Stream responseStream = response.GetResponseStream();
		StreamReader streamReader = new StreamReader(responseStream);
		string result = streamReader.ReadToEnd();
		streamReader.Close();
		responseStream.Close();
		response.Close();
		return result;
	}

	public static string Post(string url, string data)
	{
		byte[] bytes = new UTF8Encoding().GetBytes(data);
		WebRequest webRequest = WebRequest.Create(url);
		ServicePointManager.Expect100Continue = true;
		((HttpWebRequest)webRequest).UserAgent = "SCP SL";
		webRequest.Method = "POST";
		webRequest.ContentType = "application/x-www-form-urlencoded";
		webRequest.ContentLength = bytes.Length;
		Stream requestStream = webRequest.GetRequestStream();
		requestStream.Write(bytes, 0, bytes.Length);
		requestStream.Close();
		WebResponse response = webRequest.GetResponse();
		requestStream = response.GetResponseStream();
		StreamReader streamReader = new StreamReader(requestStream);
		string result = streamReader.ReadToEnd();
		streamReader.Close();
		requestStream.Close();
		response.Close();
		return result;
	}
}
