using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	internal sealed class AspMvcViewLocationFormatAttribute : Attribute
	{
		[Dissonance.NotNull]
		public string Format { get; private set; }

		public AspMvcViewLocationFormatAttribute([Dissonance.NotNull] string format)
		{
			Format = format;
		}
	}
}
