using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true)]
	internal sealed class MacroAttribute : Attribute
	{
		[Dissonance.CanBeNull]
		public string Expression { get; set; }

		public int Editable { get; set; }

		[Dissonance.CanBeNull]
		public string Target { get; set; }
	}
}
