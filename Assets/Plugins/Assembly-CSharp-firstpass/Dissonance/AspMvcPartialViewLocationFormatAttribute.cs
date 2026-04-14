using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	internal sealed class AspMvcPartialViewLocationFormatAttribute : Attribute
	{
		[NotNull]
		public string Format { get; private set; }

		public AspMvcPartialViewLocationFormatAttribute([NotNull] string format)
		{
			Format = format;
		}
	}
}
