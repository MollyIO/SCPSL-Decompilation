using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	internal sealed class RazorDirectiveAttribute : Attribute
	{
		[NotNull]
		public string Directive { get; private set; }

		public RazorDirectiveAttribute([NotNull] string directive)
		{
			Directive = directive;
		}
	}
}
