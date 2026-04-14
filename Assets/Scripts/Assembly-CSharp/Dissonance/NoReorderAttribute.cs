using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface)]
	internal sealed class NoReorderAttribute : Attribute
	{
	}
}
