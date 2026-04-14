using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Method)]
	internal sealed class NotifyPropertyChangedInvocatorAttribute : Attribute
	{
		[Dissonance.CanBeNull]
		public string ParameterName { get; private set; }

		public NotifyPropertyChangedInvocatorAttribute()
		{
		}

		public NotifyPropertyChangedInvocatorAttribute([Dissonance.NotNull] string parameterName)
		{
			ParameterName = parameterName;
		}
	}
}
