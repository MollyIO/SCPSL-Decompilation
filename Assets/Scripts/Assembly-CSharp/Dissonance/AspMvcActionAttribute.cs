using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
	internal sealed class AspMvcActionAttribute : Attribute
	{
		[Dissonance.CanBeNull]
		public string AnonymousProperty { get; private set; }

		public AspMvcActionAttribute()
		{
		}

		public AspMvcActionAttribute([Dissonance.NotNull] string anonymousProperty)
		{
			AnonymousProperty = anonymousProperty;
		}
	}
}
