using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	internal sealed class AspChildControlTypeAttribute : Attribute
	{
		[Dissonance.NotNull]
		public string TagName { get; private set; }

		[Dissonance.NotNull]
		public Type ControlType { get; private set; }

		public AspChildControlTypeAttribute([Dissonance.NotNull] string tagName, [Dissonance.NotNull] Type controlType)
		{
			TagName = tagName;
			ControlType = controlType;
		}
	}
}
