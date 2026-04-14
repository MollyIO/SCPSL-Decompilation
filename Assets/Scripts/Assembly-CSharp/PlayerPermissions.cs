public enum PlayerPermissions : ulong
{
	KickingAndShortTermBanning = 1uL,
	BanningUpToDay = 2uL,
	LongTermBanning = 4uL,
	ForceclassSelf = 8uL,
	ForceclassToSpectator = 0x10uL,
	ForceclassWithoutRestrictions = 0x20uL,
	GivingItems = 0x40uL,
	WarheadEvents = 0x80uL,
	RespawnEvents = 0x100uL,
	RoundEvents = 0x200uL,
	SetGroup = 0x400uL,
	GameplayData = 0x800uL,
	Overwatch = 0x1000uL,
	FacilityManagement = 0x2000uL,
	PlayersManagement = 0x4000uL,
	PermissionsManagement = 0x8000uL,
	ServerConsoleCommands = 0x10000uL,
	ViewHiddenBadges = 0x20000uL,
	ServerConfigs = 0x40000uL
}
