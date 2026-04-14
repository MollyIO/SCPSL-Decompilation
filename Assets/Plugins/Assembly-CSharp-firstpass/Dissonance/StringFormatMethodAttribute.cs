using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Delegate)]
	internal sealed class StringFormatMethodAttribute : Attribute
	{
		[NotNull]
		public string FormatParameterName { get; private set; }

		public StringFormatMethodAttribute([NotNull] string formatParameterName)
		{
			FormatParameterName = formatParameterName;
		}
	}
}
