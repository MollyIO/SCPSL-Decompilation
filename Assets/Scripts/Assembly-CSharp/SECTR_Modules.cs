using System;

public static class SECTR_Modules
{
	public static bool AUDIO;

	public static bool VIS;

	public static bool STREAM;

	public static bool DEV;

	public static string VERSION;

	static SECTR_Modules()
	{
		VERSION = "1.3.6";
		AUDIO = Type.GetType("SECTR_AudioSystem") != null;
		VIS = Type.GetType("SECTR_CullingCamera") != null;
		STREAM = Type.GetType("SECTR_Chunk") != null;
		DEV = Type.GetType("SECTR_Tests") != null;
	}

	public static bool HasPro()
	{
		return true;
	}

	public static bool HasComplete()
	{
		return AUDIO && VIS && STREAM;
	}
}
