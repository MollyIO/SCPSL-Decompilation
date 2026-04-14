using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Method)]
	internal sealed class MustUseReturnValueAttribute : Attribute
	{
		[CanBeNull]
		public string Justification { get; private set; }

		public MustUseReturnValueAttribute()
		{
		}

		public MustUseReturnValueAttribute([NotNull] string justification)
		{
			Justification = justification;
		}
	}
}
