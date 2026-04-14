using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Parameter)]
	internal sealed class AssertionConditionAttribute : Attribute
	{
		public Dissonance.AssertionConditionType ConditionType { get; private set; }

		public AssertionConditionAttribute(Dissonance.AssertionConditionType conditionType)
		{
			ConditionType = conditionType;
		}
	}
}
