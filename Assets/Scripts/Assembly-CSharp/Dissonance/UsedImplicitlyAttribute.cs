using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.All)]
	internal sealed class UsedImplicitlyAttribute : Attribute
	{
		public Dissonance.ImplicitUseKindFlags UseKindFlags { get; private set; }

		public Dissonance.ImplicitUseTargetFlags TargetFlags { get; private set; }

		public UsedImplicitlyAttribute()
			: this(Dissonance.ImplicitUseKindFlags.Default, Dissonance.ImplicitUseTargetFlags.Default)
		{
		}

		public UsedImplicitlyAttribute(Dissonance.ImplicitUseKindFlags useKindFlags)
			: this(useKindFlags, Dissonance.ImplicitUseTargetFlags.Default)
		{
		}

		public UsedImplicitlyAttribute(Dissonance.ImplicitUseTargetFlags targetFlags)
			: this(Dissonance.ImplicitUseKindFlags.Default, targetFlags)
		{
		}

		public UsedImplicitlyAttribute(Dissonance.ImplicitUseKindFlags useKindFlags, Dissonance.ImplicitUseTargetFlags targetFlags)
		{
			UseKindFlags = useKindFlags;
			TargetFlags = targetFlags;
		}
	}
}
