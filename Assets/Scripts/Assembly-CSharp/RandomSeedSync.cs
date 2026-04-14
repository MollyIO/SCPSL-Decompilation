using System.Runtime.InteropServices;
using GameConsole;
using UnityEngine;
using UnityEngine.Networking;

public class RandomSeedSync : NetworkBehaviour
{
	[SyncVar(hook = "SetSeed")]
	public int seed = -1;

	private static int staticSeed;

	public static bool generated;

	public int Networkseed
	{
		get
		{
			return seed;
		}
		[param: In]
		set
		{
			RandomSeedSync randomSeedSync = this;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetSeed(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref randomSeedSync.seed, 1u);
		}
	}

	private void Start()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		if (NetworkServer.active)
		{
			WorkStation[] array = Object.FindObjectsOfType<WorkStation>();
			foreach (WorkStation workStation in array)
			{
				workStation.SetPosition(new Offset
				{
					position = workStation.transform.localPosition,
					rotation = workStation.transform.localRotation.eulerAngles,
					scale = Vector3.one
				});
			}
		}
		generated = false;
		Networkseed = ConfigFile.ServerConfig.GetInt("map_seed", -1);
		while (NetworkServer.active && seed == -1)
		{
			Networkseed = Random.Range(-999999999, 999999999);
		}
	}

	private void Update()
	{
		if (!generated && base.name == "Host" && seed != -1)
		{
			staticSeed = seed;
			generated = true;
			GenerateLevel();
			Invoke("RefreshBounds", 10f);
		}
	}

	private void SetSeed(int i)
	{
		Networkseed = i;
	}

	public static void GenerateLevel()
	{
		if (!NonFacilityCompatibility.currentSceneSettings.enableWorldGeneration)
		{
			return;
		}
		Console console = Object.FindObjectOfType<Console>();
		console.AddLog("Initializing generator...", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
		ImageGenerator imageGenerator = null;
		ImageGenerator imageGenerator2 = null;
		ImageGenerator imageGenerator3 = null;
		ImageGenerator[] array = Object.FindObjectsOfType<ImageGenerator>();
		foreach (ImageGenerator imageGenerator4 in array)
		{
			switch (imageGenerator4.height)
			{
			case 0:
				imageGenerator = imageGenerator4;
				break;
			case -1000:
				imageGenerator2 = imageGenerator4;
				break;
			case -1001:
				imageGenerator3 = imageGenerator4;
				break;
			}
		}
		if (!TutorialManager.status)
		{
			if (imageGenerator != null)
			{
				imageGenerator.GenerateMap(staticSeed);
			}
			if (imageGenerator2 != null)
			{
				imageGenerator2.GenerateMap(staticSeed + 1);
			}
			if (imageGenerator3 != null)
			{
				imageGenerator3.GenerateMap(staticSeed + 2);
			}
			Door[] array2 = Object.FindObjectsOfType<Door>();
			foreach (Door door in array2)
			{
				door.UpdatePos();
			}
		}
		GameObject[] array3 = GameObject.FindGameObjectsWithTag("DoorButton");
		foreach (GameObject gameObject in array3)
		{
			try
			{
				gameObject.GetComponent<ButtonWallAdjuster>().Adjust();
				ButtonWallAdjuster[] componentsInChildren = gameObject.GetComponentsInChildren<ButtonWallAdjuster>();
				foreach (ButtonWallAdjuster buttonWallAdjuster in componentsInChildren)
				{
					buttonWallAdjuster.Invoke("Adjust", 4f);
				}
			}
			catch
			{
			}
		}
		Lift[] array4 = Object.FindObjectsOfType<Lift>();
		foreach (Lift lift in array4)
		{
			Lift.Elevator[] elevators = lift.elevators;
			foreach (Lift.Elevator elevator in elevators)
			{
				elevator.SetPosition();
			}
		}
		console.AddLog("Spawning items...", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
		Door[] array5 = Object.FindObjectsOfType<Door>();
		foreach (Door door2 in array5)
		{
			if (door2.destroyed)
			{
				door2.DestroyDoor(true);
			}
			else
			{
				door2.SetActiveStatus(1);
				door2.SetActiveStatus(0);
			}
			door2.SetState(door2.isOpen);
		}
		if (NetworkServer.active)
		{
			PlayerManager.localPlayer.GetComponent<HostItemSpawner>().Spawn(staticSeed);
		}
		SECTR_Member[] array6 = Object.FindObjectsOfType<SECTR_Member>();
		foreach (SECTR_Member sECTR_Member in array6)
		{
			sECTR_Member.UpdateViaScript();
		}
		Pickup[] array7 = Object.FindObjectsOfType<Pickup>();
		foreach (Pickup pickup in array7)
		{
			pickup.transform.position = pickup.info.position;
			pickup.transform.rotation = pickup.info.rotation;
		}
		Object.FindObjectOfType<LCZ_LabelManager>().RefreshLabels();
		console.AddLog("The scene is ready! Good luck!", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
		if (NetworkServer.active)
		{
			Locker[] array8 = Object.FindObjectsOfType<Locker>();
			foreach (Locker locker in array8)
			{
				locker.GetReady();
			}
		}
		Scp079Interactable[] array9 = Object.FindObjectsOfType<Scp079Interactable>();
		foreach (Scp079Interactable scp079Interactable in array9)
		{
			scp079Interactable.OnMapGenerate();
		}
		Interface079.singleton.RefreshInteractables();
		if (!NetworkServer.active)
		{
			return;
		}
		GameObject[] array10 = GameObject.FindGameObjectsWithTag("GeneratorSpawn");
		foreach (Generator079 generator in Generator079.generators)
		{
			int num6;
			GameObject gameObject2;
			do
			{
				num6 = Random.Range(0, array10.Length);
				gameObject2 = array10[num6];
			}
			while (gameObject2 == null);
			array10[num6] = null;
			generator.SetOffset(new Offset
			{
				position = gameObject2.transform.position,
				rotation = gameObject2.transform.rotation.eulerAngles
			});
		}
	}

	private void RefreshBounds()
	{
		SECTR_Member[] array = Object.FindObjectsOfType<SECTR_Member>();
		foreach (SECTR_Member sECTR_Member in array)
		{
			sECTR_Member.UpdateViaScript();
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)seed);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)seed);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			seed = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			SetSeed((int)reader.ReadPackedUInt32());
		}
	}
}
