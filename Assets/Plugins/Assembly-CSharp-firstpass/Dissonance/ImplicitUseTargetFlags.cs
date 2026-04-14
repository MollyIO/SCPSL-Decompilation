using System;

namespace Dissonance
{
	[Flags]
	internal enum ImplicitUseTargetFlags
	{
		Default = 1,
		Itself = 1,
		Members = 2,
		WithMembers = 3
	}
}
