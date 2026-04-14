using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	internal sealed class ContractAnnotationAttribute : Attribute
	{
		[Dissonance.NotNull]
		public string Contract { get; private set; }

		public bool ForceFullStates { get; private set; }

		public ContractAnnotationAttribute([Dissonance.NotNull] string contract)
			: this(contract, false)
		{
		}

		public ContractAnnotationAttribute([Dissonance.NotNull] string contract, bool forceFullStates)
		{
			Contract = contract;
			ForceFullStates = forceFullStates;
		}
	}
}
