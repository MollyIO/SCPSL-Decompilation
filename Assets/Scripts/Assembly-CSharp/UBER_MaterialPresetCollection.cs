using UnityEngine;

public class UBER_MaterialPresetCollection : ScriptableObject
{
	[SerializeField]
	[HideInInspector]
	public string currentPresetName;

	[SerializeField]
	[HideInInspector]
	public UBER_PresetParamSection whatToRestore;

	[HideInInspector]
	[SerializeField]
	public UBER_MaterialPreset[] matPresets;

	[SerializeField]
	[HideInInspector]
	public string[] names;
}
