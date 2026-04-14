using System;

namespace Dissonance
{
	[Flags]
	internal enum CollectionAccessType
	{
		None = 0,
		Read = 1,
		ModifyExistingContent = 2,
		UpdatedContent = 6
	}
}
