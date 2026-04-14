using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = true)]
	internal sealed class ValueProviderAttribute : Attribute
	{
		[Dissonance.NotNull]
		public string Name { get; private set; }

		public ValueProviderAttribute([Dissonance.NotNull] string name)
		{
			Name = name;
		}
	}
}
