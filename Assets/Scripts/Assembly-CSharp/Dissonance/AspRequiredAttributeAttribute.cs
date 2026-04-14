using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	internal sealed class AspRequiredAttributeAttribute : Attribute
	{
		[Dissonance.NotNull]
		public string Attribute { get; private set; }

		public AspRequiredAttributeAttribute([Dissonance.NotNull] string attribute)
		{
			Attribute = attribute;
		}
	}
}
