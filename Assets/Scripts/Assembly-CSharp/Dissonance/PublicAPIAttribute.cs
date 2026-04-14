using System;

namespace Dissonance
{
	[Dissonance.MeansImplicitUse(Dissonance.ImplicitUseTargetFlags.WithMembers)]
	internal sealed class PublicAPIAttribute : Attribute
	{
		[Dissonance.CanBeNull]
		public string Comment { get; private set; }

		public PublicAPIAttribute()
		{
		}

		public PublicAPIAttribute([Dissonance.NotNull] string comment)
		{
			Comment = comment;
		}
	}
}
