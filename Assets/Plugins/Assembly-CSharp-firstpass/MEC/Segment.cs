namespace MEC
{
	public enum Segment
	{
		Invalid = -1,
		Update = 0,
		FixedUpdate = 1,
		LateUpdate = 2,
		SlowUpdate = 3,
		RealtimeUpdate = 4,
		EditorUpdate = 5,
		EditorSlowUpdate = 6,
		EndOfFrame = 7,
		ManualTimeframe = 8
	}
}
