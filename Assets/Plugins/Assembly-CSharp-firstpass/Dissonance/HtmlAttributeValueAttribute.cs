using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
	internal sealed class HtmlAttributeValueAttribute : Attribute
	{
		[NotNull]
		public string Name { get; private set; }

		public HtmlAttributeValueAttribute([NotNull] string name)
		{
			Name = name;
		}
	}
}
