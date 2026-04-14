using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
	internal sealed class HtmlElementAttributesAttribute : Attribute
	{
		[CanBeNull]
		public string Name { get; private set; }

		public HtmlElementAttributesAttribute()
		{
		}

		public HtmlElementAttributesAttribute([NotNull] string name)
		{
			Name = name;
		}
	}
}
