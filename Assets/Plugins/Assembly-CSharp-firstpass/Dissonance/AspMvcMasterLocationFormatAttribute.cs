using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	internal sealed class AspMvcMasterLocationFormatAttribute : Attribute
	{
		[NotNull]
		public string Format { get; private set; }

		public AspMvcMasterLocationFormatAttribute([NotNull] string format)
		{
			Format = format;
		}
	}
}
