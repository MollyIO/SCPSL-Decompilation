using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Parameter)]
	internal sealed class PathReferenceAttribute : Attribute
	{
		[CanBeNull]
		public string BasePath { get; private set; }

		public PathReferenceAttribute()
		{
		}

		public PathReferenceAttribute([NotNull][PathReference] string basePath)
		{
			BasePath = basePath;
		}
	}
}
