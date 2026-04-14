using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

public class FileManager
{
	private static string _appfolder = string.Empty;

	public static string GetAppFolder(bool shared = false, bool addseparator = true, bool addport = false, bool addconfigs = false)
	{
		if (shared)
		{
			if (ConfigFile.HosterPolicy != null && ConfigFile.HosterPolicy.GetBool("gamedir_for_configs"))
			{
				return "AppData" + ((!addseparator) ? string.Empty : GetPathSeparator().ToString());
			}
			return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + GetPathSeparator() + "SCP Secret Laboratory" + ((!addseparator) ? string.Empty : GetPathSeparator().ToString());
		}
		if (ConfigFile.HosterPolicy != null && ConfigFile.HosterPolicy.GetBool("gamedir_for_configs"))
		{
			return "AppData" + ((!addport) ? string.Empty : (GetPathSeparator() + NetworkManager.singleton.networkPort.ToString())) + ((!addconfigs) ? string.Empty : (GetPathSeparator() + "configs")) + ((!addseparator) ? string.Empty : GetPathSeparator().ToString());
		}
		if (!string.IsNullOrEmpty(_appfolder))
		{
			return _appfolder + ((!addport) ? string.Empty : (GetPathSeparator() + NetworkManager.singleton.networkPort.ToString())) + ((!addconfigs) ? string.Empty : (GetPathSeparator() + "configs")) + ((!addseparator) ? string.Empty : GetPathSeparator().ToString());
		}
		return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + GetPathSeparator() + "SCP Secret Laboratory" + ((!addport) ? string.Empty : (GetPathSeparator() + NetworkManager.singleton.networkPort.ToString())) + ((!addconfigs) ? string.Empty : (GetPathSeparator() + "configs")) + ((!addseparator) ? string.Empty : GetPathSeparator().ToString());
	}

	public static void SetAppFolder(string path)
	{
		if (Directory.Exists(path))
		{
			while (path.EndsWith("\\") || path.EndsWith("/") || path.EndsWith(GetPathSeparator().ToString()))
			{
				path = path.Remove(_appfolder.Length - 2);
			}
			_appfolder = path;
		}
	}

	public static string ReplacePathSeparators(string path)
	{
		return path.Replace('/', GetPathSeparator()).Replace('\\', GetPathSeparator());
	}

	public static char GetPathSeparator()
	{
		return Path.DirectorySeparatorChar;
	}

	public static bool FileExists(string path)
	{
		return File.Exists(path);
	}

	public static bool DictionaryExists(string path)
	{
		return Directory.Exists(path);
	}

	public static string[] ReadAllLines(string path)
	{
		return File.ReadAllLines(path, Encoding.UTF8);
	}

	public static void WriteToFile(IEnumerable<string> data, string path, bool removeempty = false)
	{
		File.WriteAllLines(path, (!removeempty) ? data.ToArray() : data.Where((string line) => !string.IsNullOrEmpty(line.Replace(Environment.NewLine, string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty)
			.Replace(" ", string.Empty))).ToArray(), Encoding.UTF8);
	}

	public static void WriteStringToFile(string data, string path)
	{
		File.WriteAllText(path, data, Encoding.UTF8);
	}

	public static void AppendFile(string data, string path, bool newLine = true)
	{
		string[] array = ReadAllLines(path);
		if (!newLine || array.Length == 0 || array[array.Length - 1].EndsWith(Environment.NewLine) || array[array.Length - 1].EndsWith("\n"))
		{
			File.AppendAllText(path, data, Encoding.UTF8);
		}
		else
		{
			File.AppendAllText(path, Environment.NewLine + data, Encoding.UTF8);
		}
	}

	public static void RenameFile(string path, string newpath)
	{
		File.Move(path, newpath);
	}

	public static void DeleteFile(string path)
	{
		File.Delete(path);
	}

	public static void ReplaceLine(int line, string text, string path)
	{
		string[] array = ReadAllLines(path);
		array[line] = text;
		WriteToFile(array, path);
	}

	public static void RemoveEmptyLines(string path)
	{
		string[] array = (from s in ReadAllLines(path)
			where !string.IsNullOrEmpty(s.Replace(Environment.NewLine, string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty)
				.Replace(" ", string.Empty))
			select s).ToArray();
		if (ReadAllLines(path) != array)
		{
			WriteToFile(array, path);
		}
	}

	private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(sourceDirName);
		if (!directoryInfo.Exists)
		{
			throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
		}
		DirectoryInfo[] directories = directoryInfo.GetDirectories();
		if (Directory.Exists(destDirName))
		{
			Directory.Delete(destDirName, true);
		}
		Directory.CreateDirectory(destDirName);
		FileInfo[] files = directoryInfo.GetFiles();
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			string destFileName = Path.Combine(destDirName, fileInfo.Name);
			fileInfo.CopyTo(destFileName, true);
		}
		if (copySubDirs)
		{
			DirectoryInfo[] array2 = directories;
			foreach (DirectoryInfo directoryInfo2 in array2)
			{
				string destDirName2 = Path.Combine(destDirName, directoryInfo2.Name);
				DirectoryCopy(directoryInfo2.FullName, destDirName2);
			}
		}
	}
}
