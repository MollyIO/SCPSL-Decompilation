using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
	internal sealed class CannotApplyEqualityOperatorAttribute : Attribute
	{
	}
}
