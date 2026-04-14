using UnityEngine;

public class GunShoot : MonoBehaviour
{
	public float fireRate = 0.25f;

	public float weaponRange = 20f;

	public Transform gunEnd;

	public ParticleSystem muzzleFlash;

	public ParticleSystem cartridgeEjection;

	public GameObject metalHitEffect;

	public GameObject sandHitEffect;

	public GameObject stoneHitEffect;

	public GameObject waterLeakEffect;

	public GameObject waterLeakExtinguishEffect;

	public GameObject[] fleshHitEffects;

	public GameObject woodHitEffect;

	private float nextFire;

	private Animator anim;

	private GunAim gunAim;

	private void Start()
	{
		anim = GetComponent<Animator>();
		gunAim = GetComponentInParent<GunAim>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(NewInput.GetKey("Fire1")) && Time.time > nextFire && !gunAim.GetIsOutOfBounds())
		{
			nextFire = Time.time + fireRate;
			muzzleFlash.Play();
			cartridgeEjection.Play();
			anim.SetTrigger("Fire");
			Vector3 position = gunEnd.position;
			RaycastHit hitInfo;
			if (Physics.Raycast(position, gunEnd.forward, out hitInfo, weaponRange))
			{
				HandleHit(hitInfo);
			}
		}
	}

	private void HandleHit(RaycastHit hit)
	{
		if (hit.collider.sharedMaterial != null)
		{
			switch (hit.collider.sharedMaterial.name)
			{
			case "Metal":
				SpawnDecal(hit, metalHitEffect);
				break;
			case "Sand":
				SpawnDecal(hit, sandHitEffect);
				break;
			case "Stone":
				SpawnDecal(hit, stoneHitEffect);
				break;
			case "WaterFilled":
				SpawnDecal(hit, waterLeakEffect);
				SpawnDecal(hit, metalHitEffect);
				break;
			case "Wood":
				SpawnDecal(hit, woodHitEffect);
				break;
			case "Meat":
				SpawnDecal(hit, fleshHitEffects[Random.Range(0, fleshHitEffects.Length)]);
				break;
			case "Character":
				SpawnDecal(hit, fleshHitEffects[Random.Range(0, fleshHitEffects.Length)]);
				break;
			case "WaterFilledExtinguish":
				SpawnDecal(hit, waterLeakExtinguishEffect);
				SpawnDecal(hit, metalHitEffect);
				break;
			}
		}
	}

	private void SpawnDecal(RaycastHit hit, GameObject prefab)
	{
		GameObject gameObject = Object.Instantiate(prefab, hit.point, Quaternion.LookRotation(hit.normal));
		gameObject.transform.SetParent(hit.collider.transform);
	}
}
