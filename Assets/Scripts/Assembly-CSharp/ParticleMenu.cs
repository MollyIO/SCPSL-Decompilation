using UnityEngine;
using UnityEngine.UI;

public class ParticleMenu : MonoBehaviour
{
	public ParticleExamples[] particleSystems;

	public GameObject gunGameObject;

	private int currentIndex;

	private GameObject currentGO;

	public Transform spawnLocation;

	public Text title;

	public Text description;

	public Text navigationDetails;

	private void Start()
	{
		Navigate(0);
		currentIndex = 0;
	}

	public void Navigate(int i)
	{
		currentIndex = (particleSystems.Length + currentIndex + i) % particleSystems.Length;
		if (currentGO != null)
		{
			Object.Destroy(currentGO);
		}
		currentGO = Object.Instantiate(particleSystems[currentIndex].particleSystemGO, spawnLocation.position + particleSystems[currentIndex].particlePosition, Quaternion.Euler(particleSystems[currentIndex].particleRotation));
		gunGameObject.SetActive(particleSystems[currentIndex].isWeaponEffect);
		title.text = particleSystems[currentIndex].title;
		description.text = particleSystems[currentIndex].description;
		navigationDetails.text = string.Empty + (currentIndex + 1) + " out of " + particleSystems.Length.ToString();
	}
}
