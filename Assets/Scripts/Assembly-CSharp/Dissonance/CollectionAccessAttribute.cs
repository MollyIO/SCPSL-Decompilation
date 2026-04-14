using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property)]
	internal sealed class CollectionAccessAttribute : Attribute
	{
		public Dissonance.CollectionAccessType CollectionAccessType { get; private set; }

		public CollectionAccessAttribute(Dissonance.CollectionAccessType collectionAccessType)
		{
			CollectionAccessType = collectionAccessType;
		}
	}
}
