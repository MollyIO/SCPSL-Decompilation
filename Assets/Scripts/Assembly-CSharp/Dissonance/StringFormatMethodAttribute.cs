using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Delegate)]
	internal sealed class StringFormatMethodAttribute : Attribute
	{
		[Dissonance.NotNull]
		public string FormatParameterName { get; private set; }

		public StringFormatMethodAttribute([Dissonance.NotNull] string formatParameterName)
		{
			FormatParameterName = formatParameterName;
		}
	}
}
