using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
	internal sealed class HtmlAttributeValueAttribute : Attribute
	{
		[Dissonance.NotNull]
		public string Name { get; private set; }

		public HtmlAttributeValueAttribute([Dissonance.NotNull] string name)
		{
			Name = name;
		}
	}
}
