using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true)]
	internal sealed class MacroAttribute : Attribute
	{
		[CanBeNull]
		public string Expression { get; set; }

		public int Editable { get; set; }

		[CanBeNull]
		public string Target { get; set; }
	}
}
