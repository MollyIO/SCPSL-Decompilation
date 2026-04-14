using System;

namespace Dissonance
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	internal sealed class ContractAnnotationAttribute : Attribute
	{
		[NotNull]
		public string Contract { get; private set; }

		public bool ForceFullStates { get; private set; }

		public ContractAnnotationAttribute([NotNull] string contract)
			: this(contract, false)
		{
		}

		public ContractAnnotationAttribute([NotNull] string contract, bool forceFullStates)
		{
			Contract = contract;
			ForceFullStates = forceFullStates;
		}
	}
}
