using System;

[Serializable]
public class UserGroup
{
	public string BadgeColor;

	public string BadgeText;

	public ulong Permissions;

	public bool Cover;

	public bool HiddenByDefault;

	public UserGroup Clone()
	{
		UserGroup userGroup = new UserGroup();
		userGroup.BadgeColor = BadgeColor;
		userGroup.BadgeText = BadgeText;
		userGroup.Permissions = Permissions;
		userGroup.Cover = Cover;
		userGroup.HiddenByDefault = HiddenByDefault;
		return userGroup;
	}
}
