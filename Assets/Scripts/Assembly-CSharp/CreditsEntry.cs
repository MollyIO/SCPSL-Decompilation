using System.Linq;

public class CreditsEntry
{
	public string Title;

	public string Name;

	public bool Multi;

	public static CreditsEntry CreateSeparator()
	{
		return new CreditsEntry();
	}

	public static CreditsEntry CreateEntry(string title, string name)
	{
		CreditsEntry creditsEntry = new CreditsEntry();
		creditsEntry.Multi = false;
		creditsEntry.Title = title;
		creditsEntry.Name = name;
		return creditsEntry;
	}

	public static CreditsEntry CreateEntry(string name)
	{
		CreditsEntry creditsEntry = new CreditsEntry();
		creditsEntry.Multi = false;
		creditsEntry.Title = string.Empty;
		creditsEntry.Name = name;
		return creditsEntry;
	}

	public static CreditsEntry CreateEntry(string[] names)
	{
		string name = names.Aggregate(string.Empty, (string current, string n) => current + n + "\n");
		CreditsEntry creditsEntry = new CreditsEntry();
		creditsEntry.Multi = true;
		creditsEntry.Title = string.Empty;
		creditsEntry.Name = name;
		return creditsEntry;
	}
}
