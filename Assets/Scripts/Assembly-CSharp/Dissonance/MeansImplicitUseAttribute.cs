using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.GenericParameter)]
	internal sealed class MeansImplicitUseAttribute : Attribute
	{
		[Dissonance.UsedImplicitly]
		public Dissonance.ImplicitUseKindFlags UseKindFlags { get; private set; }

		[Dissonance.UsedImplicitly]
		public Dissonance.ImplicitUseTargetFlags TargetFlags { get; private set; }

		public MeansImplicitUseAttribute()
			: this(Dissonance.ImplicitUseKindFlags.Default, Dissonance.ImplicitUseTargetFlags.Default)
		{
		}

		public MeansImplicitUseAttribute(Dissonance.ImplicitUseKindFlags useKindFlags)
			: this(useKindFlags, Dissonance.ImplicitUseTargetFlags.Default)
		{
		}

		public MeansImplicitUseAttribute(Dissonance.ImplicitUseTargetFlags targetFlags)
			: this(Dissonance.ImplicitUseKindFlags.Default, targetFlags)
		{
		}

		public MeansImplicitUseAttribute(Dissonance.ImplicitUseKindFlags useKindFlags, Dissonance.ImplicitUseTargetFlags targetFlags)
		{
			UseKindFlags = useKindFlags;
			TargetFlags = targetFlags;
		}
	}
}
