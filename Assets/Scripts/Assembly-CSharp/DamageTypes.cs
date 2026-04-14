public class DamageTypes
{
	public class DamageType
	{
		public readonly string name;

		public readonly bool isWeapon;

		public readonly bool isScp;

		public readonly int weaponId;

		public DamageType(string name, bool weapon = false, bool scp = false, int weaponId = -1)
		{
			this.name = name;
			isWeapon = weapon;
			isScp = scp;
			this.weaponId = weaponId;
		}
	}

	public static DamageType None = new DamageType("NONE");

	public static readonly DamageType Lure = new DamageType("LURE");

	public static readonly DamageType Nuke = new DamageType("NUKE");

	public static readonly DamageType Wall = new DamageType("WALL");

	public static readonly DamageType Decont = new DamageType("DECONT");

	public static readonly DamageType Tesla = new DamageType("TESLA");

	public static readonly DamageType Falldown = new DamageType("FALLDOWN");

	public static readonly DamageType Flying = new DamageType("Flying detection");

	public static readonly DamageType Contain = new DamageType("CONTAIN");

	public static readonly DamageType Pocket = new DamageType("POCKET");

	public static readonly DamageType RagdollLess = new DamageType("RAGDOLL-LESS");

	public static readonly DamageType Com15 = new DamageType("Com15", true, false, 0);

	public static readonly DamageType P90 = new DamageType("P90", true, false, 1);

	public static readonly DamageType E11StandardRifle = new DamageType("E11 Standard Rifle", true, false, 2);

	public static readonly DamageType Mp7 = new DamageType("MP7", true, false, 3);

	public static readonly DamageType Logicer = new DamageType("Logicier", true, false, 4);

	public static readonly DamageType Usp = new DamageType("USP", true, false, 5);

	public static readonly DamageType Grenade = new DamageType("GRENADE");

	public static readonly DamageType Scp049 = new DamageType("SCP-049", false, true);

	public static readonly DamageType Scp0492 = new DamageType("SCP-049-2", false, true);

	public static readonly DamageType Scp096 = new DamageType("SCP-096", false, true);

	public static readonly DamageType Scp106 = new DamageType("SCP-106", false, true);

	public static readonly DamageType Scp173 = new DamageType("SCP-173", false, true);

	public static readonly DamageType Scp939 = new DamageType("SCP-939", false, true);

	private static readonly DamageType[] damageTypes = new DamageType[24]
	{
		None, Lure, Nuke, Wall, Decont, Tesla, Falldown, Flying, Contain, Pocket,
		RagdollLess, Com15, P90, E11StandardRifle, Mp7, Logicer, Usp, Grenade, Scp049, Scp0492,
		Scp096, Scp106, Scp173, Scp939
	};

	public static DamageType FromIndex(int id)
	{
		if (id >= 0 && id < damageTypes.Length)
		{
			return damageTypes[id];
		}
		return None;
	}

	public static int ToIndex(DamageType damageType)
	{
		for (int i = 0; i < damageTypes.Length; i++)
		{
			if (damageTypes[i] == damageType)
			{
				return i;
			}
		}
		return 0;
	}

	public static DamageType FromWeaponId(int weaponId)
	{
		DamageType[] array = damageTypes;
		foreach (DamageType damageType in array)
		{
			if (damageType.isWeapon && damageType.weaponId == weaponId)
			{
				return damageType;
			}
		}
		return None;
	}
}
