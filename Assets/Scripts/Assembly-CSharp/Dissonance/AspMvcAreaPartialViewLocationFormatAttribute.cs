using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	internal sealed class AspMvcAreaPartialViewLocationFormatAttribute : Attribute
	{
		[Dissonance.NotNull]
		public string Format { get; private set; }

		public AspMvcAreaPartialViewLocationFormatAttribute([Dissonance.NotNull] string format)
		{
			Format = format;
		}
	}
}
