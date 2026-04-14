using System;

namespace Dissonance
{
	[Dissonance.BaseTypeRequired(typeof(Attribute))]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	internal sealed class BaseTypeRequiredAttribute : Attribute
	{
		[Dissonance.NotNull]
		public Type BaseType { get; private set; }

		public BaseTypeRequiredAttribute([Dissonance.NotNull] Type baseType)
		{
			BaseType = baseType;
		}
	}
}
