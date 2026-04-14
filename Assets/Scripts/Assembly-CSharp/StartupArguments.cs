using System;
using System.Linq;

public class StartupArguments
{
	public static bool IsSetShort(string param)
	{
		return Environment.GetCommandLineArgs().Any((string x) => x.StartsWith("-") && !x.StartsWith("--") && x.Contains(param));
	}

	public static bool IsSetBool(string param, string alias = "")
	{
		return Environment.GetCommandLineArgs().Contains("--" + param) || (!string.IsNullOrEmpty(alias) && IsSetShort(alias));
	}

	public static string GetArgument(string param, string alias = "", string def = "")
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		bool flag = false;
		string[] array = commandLineArgs;
		foreach (string text in array)
		{
			if (flag && !text.StartsWith("-"))
			{
				return text;
			}
			flag = text == "--" + param || (!string.IsNullOrEmpty(alias) && text.StartsWith("-") && !text.StartsWith("--") && text.EndsWith(alias));
		}
		return def;
	}
}
