using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	internal sealed class RazorImportNamespaceAttribute : Attribute
	{
		[Dissonance.NotNull]
		public string Name { get; private set; }

		public RazorImportNamespaceAttribute([Dissonance.NotNull] string name)
		{
			Name = name;
		}
	}
}
