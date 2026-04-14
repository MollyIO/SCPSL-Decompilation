using System;
using UnityEngine;
using UnityEngine.PostProcessing;

[Serializable]
public class Class
{
	public string fullName = "Chaos Insurgency";

	public Color classColor = Color.white;

	[Multiline]
	public string description;

	public Team team;

	public PostProcessingProfile postprocessingProfile;

	[Space]
	public int[] ammoTypes = new int[3] { 100, 100, 100 };

	public int maxHP = 100;

	public float walkSpeed = 5f;

	public float runSpeed = 7f;

	public float jumpSpeed = 7f;

	public GameObject model_player;

	public Offset model_offset;

	public GameObject model_ragdoll;

	public Offset ragdoll_offset;

	public int[] startItems;

	public float classRecoil = 1f;

	[Space]
	public AudioClip[] stepClips;

	public bool banClass;

	public float iconHeightOffset;

	public int forcedCrosshair = -1;

	public bool useHeadBob;

	public int bloodType;
}
