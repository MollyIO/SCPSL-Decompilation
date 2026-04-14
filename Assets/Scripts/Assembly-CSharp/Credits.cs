using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Credits : MonoBehaviour
{
	public TextMeshProUGUI Header;

	public TextMeshProUGUI[] RecordTitles;

	public TextMeshProUGUI[] Records;

	public static readonly string Patreons = "All The Time Gaming\r\nArztDerpy\r\nAzmith BlackHeart\r\nBlobbygobster\r\nDj Peatz\r\nFlexedTrack\r\nFlight\r\nGONE\r\nhiromiti\r\nHotomi Mitzugashi\r\nInfamous1\r\nKozak\r\nMagilou\r\nMe_just_in/rofldile\r\nMiiers\r\nNynamo\r\nNynamo\r\nQuartz563\r\nractii7\r\nsanyae2439\r\nSCP-2662 “Cthulhu”\r\nSingularity\r\nsoup\r\nSpacepl\r\nSushiloid\r\nSyinn\r\nVoidus";

	public static CreditsCategory[] CreditsData = new CreditsCategory[12]
	{
		new CreditsCategory
		{
			Header = "STUDIO DIRECTORS",
			DisplayTime = 3000,
			Records = new CreditsEntry[6]
			{
				CreditsEntry.CreateEntry("CHIEF EXECUTIVE OFFICER", "Hubert Moszka"),
				CreditsEntry.CreateEntry("CHIEF OPERATIONS OFFICER", "welknair"),
				CreditsEntry.CreateEntry("GAME DIRECTOR", "Emperor"),
				CreditsEntry.CreateEntry("CHIEF INFORMATION OFFICER", "zabszk"),
				CreditsEntry.CreateEntry("CHIEF COMMUNICATIONS OFFICER", "SkullOG"),
				CreditsEntry.CreateEntry("Development Administrator", "Adve")
			}
		},
		new CreditsCategory
		{
			Header = "SCP - CONTAINMENT BREACH DEVS",
			Records = new CreditsEntry[8]
			{
				CreditsEntry.CreateEntry("Undertow Games", "Joonas Rikkonen"),
				CreditsEntry.CreateEntry("Third Subvision Studio", "Vane Brain"),
				CreditsEntry.CreateSeparator(),
				CreditsEntry.CreateSeparator(),
				CreditsEntry.CreateEntry(new string[3] { "gmodfan11", "Catnipbuddy", "philip45" }),
				CreditsEntry.CreateEntry(new string[3] { "FragdaddyXXL", "FireFox", "Walach" }),
				CreditsEntry.CreateEntry(new string[3] { "Reilly Grindle", "GamerEntitlement", "Brandon Smith" }),
				CreditsEntry.CreateEntry(new string[3] { "Ryan Stillings", "GamerEntitlement", "Hakkla" })
			}
		},
		new CreditsCategory
		{
			Header = "$50+ Patreon Supporters",
			Records = ParsePatreons().ToArray()
		},
		new CreditsCategory
		{
			Header = "PROGRAMMERS",
			Records = new CreditsEntry[7]
			{
				CreditsEntry.CreateEntry("Lead Gameplay Programmer", "James"),
				CreditsEntry.CreateEntry("Citrio"),
				CreditsEntry.CreateEntry("Griffin"),
				CreditsEntry.CreateEntry("Khaos"),
				CreditsEntry.CreateEntry("Laserman"),
				CreditsEntry.CreateEntry("REALM"),
				CreditsEntry.CreateEntry("Senpai")
			}
		},
		new CreditsCategory
		{
			Header = "GAME DESIGNERS",
			Records = new CreditsEntry[3]
			{
				CreditsEntry.CreateEntry("Lead Game Designer", "Wavepoole"),
				CreditsEntry.CreateEntry("Elfenstein"),
				CreditsEntry.CreateEntry("Javier")
			}
		},
		new CreditsCategory
		{
			Header = "ARTISTS",
			Records = new CreditsEntry[14]
			{
				CreditsEntry.CreateEntry("Lead Artist", "PurpleGoop"),
				CreditsEntry.CreateEntry("Buce"),
				CreditsEntry.CreateEntry("crazycheese"),
				CreditsEntry.CreateEntry("DisgustedWater"),
				CreditsEntry.CreateEntry("Dovafox"),
				CreditsEntry.CreateEntry("H0plite"),
				CreditsEntry.CreateEntry("k0vac \ud83c\uddf7\ud83c\uddf8"),
				CreditsEntry.CreateEntry("Kanf"),
				CreditsEntry.CreateEntry("Luna"),
				CreditsEntry.CreateEntry("Moranatol"),
				CreditsEntry.CreateEntry("Pierre Martinon"),
				CreditsEntry.CreateEntry("Raio Boss"),
				CreditsEntry.CreateEntry("tammy"),
				CreditsEntry.CreateEntry("Vess")
			}
		},
		new CreditsCategory
		{
			Header = "VOICE ACTORS & SOUND DESIGNERS",
			Records = new CreditsEntry[6]
			{
				CreditsEntry.CreateEntry("C.A.S.S.I.E. voice", "Allen \"Narlor\" Johnson"),
				CreditsEntry.CreateEntry("TUTORIAL INSTRUCTOR", "TheVolgun"),
				CreditsEntry.CreateEntry("CHARACTER DIALOGUES", "Doggdemon"),
				CreditsEntry.CreateEntry("CHARACTER DIALOGUES", "Wizz Da Burrito Boy"),
				CreditsEntry.CreateEntry("SOUND DESIGNER", "Eternium"),
				CreditsEntry.CreateEntry("SOUNDTRACK", "Jacek \"Fragik\" Rogal")
			}
		},
		new CreditsCategory
		{
			Header = "SECURITY",
			Records = new CreditsEntry[23]
			{
				CreditsEntry.CreateEntry("System Administrator", "Blizzard"),
				CreditsEntry.CreateEntry("System Administrator", "Faety"),
				CreditsEntry.CreateEntry("System Administrator", "Rin"),
				CreditsEntry.CreateEntry("Global Moderation Manager", "Humerzazer"),
				CreditsEntry.CreateEntry("Global Moderation Advisor", "TheeRider"),
				CreditsEntry.CreateEntry("Global Moderator", "Desteris"),
				CreditsEntry.CreateEntry("Global Moderator", "Killer1001"),
				CreditsEntry.CreateEntry("Global Moderator", "Nicku"),
				CreditsEntry.CreateEntry("Global Moderator", "Romlyn"),
				CreditsEntry.CreateEntry("Global Moderator", "Squishy"),
				CreditsEntry.CreateEntry("Global Moderator", "Adapt"),
				CreditsEntry.CreateEntry("Global Moderator", "Buszmen"),
				CreditsEntry.CreateEntry("Global Moderator", "Ender"),
				CreditsEntry.CreateEntry("Global Moderator", "GunRunner"),
				CreditsEntry.CreateEntry("Global Moderator", "MrVideoFreak"),
				CreditsEntry.CreateEntry("Backend Programmer", "Dankrushen"),
				CreditsEntry.CreateEntry("Backend Programmer", "Evan"),
				CreditsEntry.CreateEntry("Backend Programmer", "KernelError"),
				CreditsEntry.CreateEntry("Backend Programmer", "Kigen"),
				CreditsEntry.CreateEntry("Backend Programmer", "Petris"),
				CreditsEntry.CreateEntry("Backend Programmer", "ShingekiNoRex"),
				CreditsEntry.CreateEntry("Backend Programmer", "Vectro"),
				CreditsEntry.CreateEntry("Verification Specialist", "Sauron")
			}
		},
		new CreditsCategory
		{
			Header = "PUBLIC RELATIONS",
			Records = new CreditsEntry[43]
			{
				CreditsEntry.CreateEntry("Strategic Communication Officer", "Jamsu"),
				CreditsEntry.CreateEntry("Enforcement Communications Officer", "Desteris"),
				CreditsEntry.CreateEntry("Marketing Platform Manager", "Maverick"),
				CreditsEntry.CreateEntry("Tech Support Manager", "Papa Skibbles"),
				CreditsEntry.CreateEntry("Discord Manager", "Reshiram"),
				CreditsEntry.CreateEntry("Steam Manager", "Ðoge"),
				CreditsEntry.CreateEntry("Discord Overseer", "FloranOtten"),
				CreditsEntry.CreateEntry("Discord Overseer", "Humerzazer"),
				CreditsEntry.CreateEntry("Discord Overseer", "x3j50"),
				CreditsEntry.CreateEntry("Steam Supervisor", "Tonton John"),
				CreditsEntry.CreateEntry("Bot Developer", "Faety"),
				CreditsEntry.CreateEntry("Arxela"),
				CreditsEntry.CreateEntry("Codsworthless"),
				CreditsEntry.CreateEntry("TitanSix"),
				CreditsEntry.CreateEntry("Dj_Nathan_"),
				CreditsEntry.CreateEntry("Doggdemon"),
				CreditsEntry.CreateEntry("Dr.Hayward"),
				CreditsEntry.CreateEntry("Ender"),
				CreditsEntry.CreateEntry("GunRunner"),
				CreditsEntry.CreateEntry("GuyFawkes"),
				CreditsEntry.CreateEntry("iD4NG3R`"),
				CreditsEntry.CreateEntry("Javier"),
				CreditsEntry.CreateEntry("k0vac \ud83c\uddf7\ud83c\uddf8"),
				CreditsEntry.CreateEntry("Khaos"),
				CreditsEntry.CreateEntry("Killer1001"),
				CreditsEntry.CreateEntry("lanco786"),
				CreditsEntry.CreateEntry("MrVideoFreak"),
				CreditsEntry.CreateEntry("Nicku"),
				CreditsEntry.CreateEntry("NotADev"),
				CreditsEntry.CreateEntry("Philip"),
				CreditsEntry.CreateEntry("Phoenix--Project"),
				CreditsEntry.CreateEntry("Romlyn"),
				CreditsEntry.CreateEntry("Sauron"),
				CreditsEntry.CreateEntry("Seems Legit"),
				CreditsEntry.CreateEntry("T0ta11y Ch1ck3n"),
				CreditsEntry.CreateEntry("TheeRider"),
				CreditsEntry.CreateEntry("Tobias Sammet"),
				CreditsEntry.CreateEntry("._."),
				CreditsEntry.CreateEntry("Wafel"),
				CreditsEntry.CreateEntry("cross-conception"),
				CreditsEntry.CreateEntry("Blue"),
				CreditsEntry.CreateEntry("Ðoge"),
				CreditsEntry.CreateEntry("Th3B0r3dD3v3l0p3r")
			}
		},
		new CreditsCategory
		{
			Header = "BETA TESTERS",
			Records = new CreditsEntry[26]
			{
				CreditsEntry.CreateEntry("Adve"),
				CreditsEntry.CreateEntry("barwa"),
				CreditsEntry.CreateEntry("Buszmen"),
				CreditsEntry.CreateEntry("Chudy"),
				CreditsEntry.CreateEntry("eybi"),
				CreditsEntry.CreateEntry("Kinro"),
				CreditsEntry.CreateEntry("Gabor"),
				CreditsEntry.CreateEntry("Haksan"),
				CreditsEntry.CreateEntry("Kamiloza"),
				CreditsEntry.CreateEntry("KebabKuba"),
				CreditsEntry.CreateEntry("kewinfun"),
				CreditsEntry.CreateEntry("koweq"),
				CreditsEntry.CreateEntry("Kryxos"),
				CreditsEntry.CreateEntry("krzechix"),
				CreditsEntry.CreateEntry("Masteruś"),
				CreditsEntry.CreateEntry("Morizet"),
				CreditsEntry.CreateEntry("NarVo"),
				CreditsEntry.CreateEntry("Nasaan"),
				CreditsEntry.CreateEntry("Nasto"),
				CreditsEntry.CreateEntry("Przemysław"),
				CreditsEntry.CreateEntry("Remu"),
				CreditsEntry.CreateEntry("RGPlay"),
				CreditsEntry.CreateEntry("Rowell"),
				CreditsEntry.CreateEntry("Shibo"),
				CreditsEntry.CreateEntry("Teluś"),
				CreditsEntry.CreateEntry("Zytter")
			}
		},
		new CreditsCategory
		{
			Header = "SPECIAL THANKS",
			Records = new CreditsEntry[8]
			{
				CreditsEntry.CreateEntry("REMOTE ADMIN IDEAS", "Even \"Rnen\" Berg"),
				CreditsEntry.CreateEntry("NETWORKING SUPPORT", "VANKO"),
				CreditsEntry.CreateEntry("GENERAL ENGINE SUPPORT", "stopierwszy"),
				CreditsEntry.CreateEntry("SERVER SYSTEM SUPPORT", "JonaseQ"),
				CreditsEntry.CreateEntry("SUBSTANTIVE SUPPORT", "wanna-amigo"),
				CreditsEntry.CreateEntry("SUBSTANTIVE SUPPORT", "Miś"),
				CreditsEntry.CreateEntry("SUBSTANTIVE SUPPORT", "Ozyr"),
				CreditsEntry.CreateEntry("CHARACTER DESIGN", "Chucken")
			}
		},
		new CreditsCategory
		{
			Header = "EXTERNAL ASSETS",
			Records = new CreditsEntry[4]
			{
				CreditsEntry.CreateEntry("Camera glitch effect", "Keijiro Takahashi - KinoGlitch"),
				CreditsEntry.CreateEntry("GUNS SFX", "Mike Koenig"),
				CreditsEntry.CreateEntry("GUNS SFX", "bizniss"),
				CreditsEntry.CreateEntry("ALPHA WARHEAD THEME", "AJOURA")
			}
		}
	};

	private static bool _skip;

	private static int _timer;

	private void OnEnable()
	{
		StopAllCoroutines();
		StartCoroutine(Play());
	}

	public void Skip()
	{
		_skip = true;
	}

	private IEnumerator Play()
	{
		CreditsCategory[] creditsData = CreditsData;
		foreach (CreditsCategory data in creditsData)
		{
			Header.text = data.Header;
			Clear();
			for (int j = 0; j < data.Records.Length; j++)
			{
				RecordTitles[j % 8].text = data.Records[j].Title;
				Records[j % 8].text = data.Records[j].Name;
				if (j % 8 == 7 && j != data.Records.Length - 1)
				{
					_timer = 0;
					_skip = false;
					while (!_skip && _timer < data.DisplayTime)
					{
						_timer += 20;
						yield return new WaitForSeconds(0.02f);
					}
					Clear();
				}
			}
			_timer = 0;
			_skip = false;
			while (!_skip && _timer < data.DisplayTime)
			{
				_timer += 10;
				yield return new WaitForSeconds(0.02f);
			}
		}
		GetComponentInParent<MainMenuScript>().ChangeMenu(0);
	}

	private void Clear()
	{
		TextMeshProUGUI[] records = Records;
		foreach (TextMeshProUGUI textMeshProUGUI in records)
		{
			textMeshProUGUI.text = string.Empty;
		}
		TextMeshProUGUI[] recordTitles = RecordTitles;
		foreach (TextMeshProUGUI textMeshProUGUI2 in recordTitles)
		{
			textMeshProUGUI2.text = string.Empty;
		}
	}

	private static IEnumerable<CreditsEntry> ParsePatreons()
	{
		string[] split = Patreons.Replace("\r", string.Empty).Split('\n');
		string[] array = split;
		foreach (string p in array)
		{
			yield return CreditsEntry.CreateEntry(p);
		}
	}
}
