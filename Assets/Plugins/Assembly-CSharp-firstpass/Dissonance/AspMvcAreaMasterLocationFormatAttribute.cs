using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	internal sealed class AspMvcAreaMasterLocationFormatAttribute : Attribute
	{
		[NotNull]
		public string Format { get; private set; }

		public AspMvcAreaMasterLocationFormatAttribute([NotNull] string format)
		{
			Format = format;
		}
	}
}
