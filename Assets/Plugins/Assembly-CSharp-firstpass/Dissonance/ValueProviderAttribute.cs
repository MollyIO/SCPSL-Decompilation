using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
	internal sealed class ValueProviderAttribute : Attribute
	{
		[NotNull]
		public string Name { get; private set; }

		public ValueProviderAttribute([NotNull] string name)
		{
			Name = name;
		}
	}
}
