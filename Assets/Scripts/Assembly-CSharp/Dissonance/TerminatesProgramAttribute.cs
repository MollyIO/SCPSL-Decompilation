using System;

namespace Dissonance
{
	[Obsolete("Use [ContractAnnotation('=> halt')] instead")]
	[AttributeUsage(AttributeTargets.Method)]
	internal sealed class TerminatesProgramAttribute : Attribute
	{
	}
}
