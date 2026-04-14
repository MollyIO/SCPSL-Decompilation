using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Parameter)]
	internal sealed class PathReferenceAttribute : Attribute
	{
		[Dissonance.CanBeNull]
		public string BasePath { get; private set; }

		public PathReferenceAttribute()
		{
		}

		public PathReferenceAttribute([Dissonance.PathReference][Dissonance.NotNull] string basePath)
		{
			BasePath = basePath;
		}
	}
}
