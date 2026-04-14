using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
	internal sealed class AspMvcControllerAttribute : Attribute
	{
		[Dissonance.CanBeNull]
		public string AnonymousProperty { get; private set; }

		public AspMvcControllerAttribute()
		{
		}

		public AspMvcControllerAttribute([Dissonance.NotNull] string anonymousProperty)
		{
			AnonymousProperty = anonymousProperty;
		}
	}
}
