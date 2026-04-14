using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	internal sealed class RazorInjectionAttribute : Attribute
	{
		[Dissonance.NotNull]
		public string Type { get; private set; }

		[Dissonance.NotNull]
		public string FieldName { get; private set; }

		public RazorInjectionAttribute([Dissonance.NotNull] string type, [Dissonance.NotNull] string fieldName)
		{
			Type = type;
			FieldName = fieldName;
		}
	}
}
