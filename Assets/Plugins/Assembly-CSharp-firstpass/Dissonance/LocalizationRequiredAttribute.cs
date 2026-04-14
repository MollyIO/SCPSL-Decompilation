using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.All)]
	internal sealed class LocalizationRequiredAttribute : Attribute
	{
		public bool Required { get; private set; }

		public LocalizationRequiredAttribute()
			: this(true)
		{
		}

		public LocalizationRequiredAttribute(bool required)
		{
			Required = required;
		}
	}
}
