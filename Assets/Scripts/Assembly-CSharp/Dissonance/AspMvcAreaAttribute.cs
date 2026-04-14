using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Parameter)]
	internal sealed class AspMvcAreaAttribute : Attribute
	{
		[Dissonance.CanBeNull]
		public string AnonymousProperty { get; private set; }

		public AspMvcAreaAttribute()
		{
		}

		public AspMvcAreaAttribute([Dissonance.NotNull] string anonymousProperty)
		{
			AnonymousProperty = anonymousProperty;
		}
	}
}
