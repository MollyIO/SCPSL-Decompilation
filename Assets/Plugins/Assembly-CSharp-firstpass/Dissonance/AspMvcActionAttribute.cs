using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
	internal sealed class AspMvcActionAttribute : Attribute
	{
		[CanBeNull]
		public string AnonymousProperty { get; private set; }

		public AspMvcActionAttribute()
		{
		}

		public AspMvcActionAttribute([NotNull] string anonymousProperty)
		{
			AnonymousProperty = anonymousProperty;
		}
	}
}
