using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Method)]
	internal sealed class MustUseReturnValueAttribute : Attribute
	{
		[Dissonance.CanBeNull]
		public string Justification { get; private set; }

		public MustUseReturnValueAttribute()
		{
		}

		public MustUseReturnValueAttribute([Dissonance.NotNull] string justification)
		{
			Justification = justification;
		}
	}
}
