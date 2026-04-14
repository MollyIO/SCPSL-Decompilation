using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	internal sealed class AspRequiredAttributeAttribute : Attribute
	{
		[NotNull]
		public string Attribute { get; private set; }

		public AspRequiredAttributeAttribute([NotNull] string attribute)
		{
			Attribute = attribute;
		}
	}
}
