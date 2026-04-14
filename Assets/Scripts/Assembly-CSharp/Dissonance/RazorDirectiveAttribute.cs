using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	internal sealed class RazorDirectiveAttribute : Attribute
	{
		[Dissonance.NotNull]
		public string Directive { get; private set; }

		public RazorDirectiveAttribute([Dissonance.NotNull] string directive)
		{
			Directive = directive;
		}
	}
}
