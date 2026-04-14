using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	internal sealed class RazorInjectionAttribute : Attribute
	{
		[NotNull]
		public string Type { get; private set; }

		[NotNull]
		public string FieldName { get; private set; }

		public RazorInjectionAttribute([NotNull] string type, [NotNull] string fieldName)
		{
			Type = type;
			FieldName = fieldName;
		}
	}
}
