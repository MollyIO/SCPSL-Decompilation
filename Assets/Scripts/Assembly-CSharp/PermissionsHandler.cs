using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using RemoteAdmin;

public class PermissionsHandler
{
	private readonly string _overridePassword;

	private readonly string _overrideRole;

	private readonly Dictionary<string, UserGroup> _groups;

	private readonly Dictionary<string, string> _members;

	private readonly Dictionary<string, ulong> _permissions;

	private readonly HashSet<ulong> _raPermissions;

	private readonly YamlConfig _config;

	private ulong _lastPerm;

	private readonly bool _managerAccess;

	private readonly bool _banTeamAccess;

	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly bool _003CStaffAccess_003Ek__BackingField;

	public UserGroup OverrideGroup
	{
		get
		{
			if (!OverrideEnabled)
			{
				return null;
			}
			return _groups.ContainsKey(_overrideRole) ? _groups[_overrideRole] : null;
		}
	}

	public bool OverrideEnabled
	{
		get
		{
			if (string.IsNullOrEmpty(_overridePassword) || _overridePassword == "none")
			{
				return false;
			}
			if (!IsVerified)
			{
				return true;
			}
			if (_overridePassword.Length < 8)
			{
				ServerConsole.AddLog("Override password refused, because it's too short (requirement for verified servers only).");
				return false;
			}
			if (_overridePassword.ToLower() == _overridePassword || _overridePassword.ToUpper() == _overridePassword)
			{
				ServerConsole.AddLog("Override password refused, because it must contain mixed case chars (requirement for verified servers only).");
				return false;
			}
			if (_overridePassword.Any((char c) => !char.IsLetter(c)))
			{
				return true;
			}
			ServerConsole.AddLog("Override password refused, because it must contain digit or special symbol (requirement for verified servers only).");
			return false;
		}
	}

	public bool IsVerified { get; private set; }

	public ulong FullPerm { get; private set; }

	public bool StaffAccess
	{
		[CompilerGenerated]
		get
		{
			return _003CStaffAccess_003Ek__BackingField;
		}
	}

	public bool ManagersAccess
	{
		[CompilerGenerated]
		get
		{
			return _managerAccess || StaffAccess || IsVerified;
		}
	}

	public bool BanningTeamAccess
	{
		[CompilerGenerated]
		get
		{
			return _banTeamAccess || StaffAccess || IsVerified;
		}
	}

	public PermissionsHandler(ref YamlConfig configuration)
	{
		_config = configuration;
		_overridePassword = configuration.GetString("override_password", "none");
		_overrideRole = configuration.GetString("override_password_role", "owner");
		_003CStaffAccess_003Ek__BackingField = configuration.GetBool("enable_staff_access");
		_managerAccess = configuration.GetBool("enable_manager_access", true);
		_banTeamAccess = configuration.GetBool("enable_banteam_access", true);
		_groups = new Dictionary<string, UserGroup>();
		_raPermissions = new HashSet<ulong>();
		List<string> stringList = configuration.GetStringList("Roles");
		foreach (string item in stringList)
		{
			string text = configuration.GetString(item + "_badge", string.Empty);
			string text2 = configuration.GetString(item + "_color", string.Empty);
			bool cover = configuration.GetBool(item + "_cover", true);
			bool hiddenByDefault = configuration.GetBool(item + "_hidden");
			if (!(text == string.Empty) && !(text2 == string.Empty))
			{
				_groups.Add(item, new UserGroup
				{
					BadgeColor = text2,
					BadgeText = text,
					Permissions = 0uL,
					Cover = cover,
					HiddenByDefault = hiddenByDefault
				});
			}
		}
		_members = configuration.GetStringDictionary("Members");
		_lastPerm = 1uL;
		foreach (KeyValuePair<string, string> member in _members)
		{
			if (!_groups.ContainsKey(member.Value))
			{
				_members.Remove(member.Key);
			}
		}
		_permissions = new Dictionary<string, ulong>();
		string[] names = Enum.GetNames(typeof(PlayerPermissions));
		foreach (string text3 in names)
		{
			ulong num = (ulong)Enum.Parse(typeof(PlayerPermissions), text3);
			FullPerm |= num;
			_permissions.Add(text3, num);
			if (num != 4096 && num != 131072)
			{
				_raPermissions.Add(num);
			}
			if (num > _lastPerm)
			{
				_lastPerm = num;
			}
		}
		RefreshPermissions();
	}

	public ulong RegisterPermission(string name, bool remoteAdmin, bool refresh = true)
	{
		_lastPerm = (ulong)Math.Pow(2.0, Math.Log(_lastPerm, 2.0) + 1.0);
		FullPerm |= _lastPerm;
		_permissions.Add(name, _lastPerm);
		if (remoteAdmin)
		{
			_raPermissions.Add(_lastPerm);
		}
		if (refresh)
		{
			RefreshPermissions();
		}
		return _lastPerm;
	}

	public void RefreshPermissions()
	{
		foreach (KeyValuePair<string, UserGroup> group in _groups)
		{
			group.Value.Permissions = 0uL;
		}
		Dictionary<string, string> stringDictionary = _config.GetStringDictionary("Permissions");
		foreach (string key2 in _permissions.Keys)
		{
			ulong num = _permissions[key2];
			if (!stringDictionary.ContainsKey(key2))
			{
				continue;
			}
			string[] array = YamlConfig.ParseCommaSeparatedString(stringDictionary[key2]);
			if (array == null)
			{
				ServerConsole.AddLog("Failed to process group permissions in remote admin config! Make sure there is no typo.");
				break;
			}
			string[] array2 = array;
			foreach (string key in array2)
			{
				if (_groups.ContainsKey(key))
				{
					_groups[key].Permissions |= num;
				}
			}
		}
	}

	public bool IsRaPermitted(ulong permissions)
	{
		foreach (ulong raPermission in _raPermissions)
		{
			if (IsPermitted(permissions, raPermission))
			{
				return true;
			}
		}
		return false;
	}

	public UserGroup GetGroup(string name)
	{
		return _groups.ContainsKey(name) ? _groups[name].Clone() : null;
	}

	public List<string> GetAllGroupsNames()
	{
		return _groups.Keys.ToList();
	}

	public Dictionary<string, UserGroup> GetAllGroups()
	{
		Dictionary<string, UserGroup> dictionary = new Dictionary<string, UserGroup>();
		foreach (string key in _groups.Keys)
		{
			dictionary.Add(key, _groups[key]);
		}
		return dictionary;
	}

	public string GetPermissionName(ulong value)
	{
		return _permissions.FirstOrDefault((KeyValuePair<string, ulong> x) => x.Value == value).Key;
	}

	public ulong GetPermissionValue(string name)
	{
		return _permissions.FirstOrDefault((KeyValuePair<string, ulong> x) => x.Key == name).Value;
	}

	public List<string> GetAllPermissions()
	{
		return _permissions.Keys.ToList();
	}

	public void SetServerAsVerified()
	{
		IsVerified = true;
	}

	public bool IsPermitted(ulong permissions, PlayerPermissions check)
	{
		return IsPermitted(permissions, Convert.ToUInt64(check));
	}

	public bool IsPermitted(ulong permissions, PlayerPermissions[] check)
	{
		if (check.Length == 0)
		{
			return true;
		}
		ulong num = 0uL;
		num = check.Aggregate(0uL, (ulong current, PlayerPermissions c) => current | Convert.ToUInt64(c));
		return IsPermitted(permissions, num);
	}

	public bool IsPermitted(ulong permissions, string check)
	{
		return _permissions.ContainsKey(check) && IsPermitted(permissions, _permissions[check]);
	}

	public bool IsPermitted(ulong permissions, string[] check)
	{
		if (check.Length == 0)
		{
			return true;
		}
		ulong check2 = check.Where((string c) => _permissions.ContainsKey(c)).Aggregate(0uL, (ulong current, string c) => current | _permissions[c]);
		return IsPermitted(permissions, check2);
	}

	public bool IsPermitted(ulong permissions, ulong check)
	{
		return (permissions & check) != 0;
	}

	public byte[] DerivePassword(byte[] serverSalt, byte[] clientSalt)
	{
		return QueryProcessor.DerivePassword(_overridePassword, serverSalt, clientSalt);
	}

	public UserGroup GetUserGroup(string steamId)
	{
		return _members.ContainsKey(steamId) ? _groups[_members[steamId]] : null;
	}
}
