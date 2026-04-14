using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	internal sealed class AspMvcSuppressViewErrorAttribute : Attribute
	{
	}
}
