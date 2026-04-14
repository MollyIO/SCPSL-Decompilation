using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Networking;

namespace AntiFaker
{
	public class AntiFakeCommands : NetworkBehaviour
	{
		private Scp173PlayerScript scp173;

		private Scp096PlayerScript scp096;

		private PlyMovementSync pms;

		private CharacterClassManager ccm;

		private float distanceTraveled;

		private Vector3 prevPos = Vector3.zero;

		private float maxDistance;

		[Header("Noclip Protection")]
		internal bool NoclipProtection = true;

		internal bool NoclipProtectionOutput;

		internal bool SpeedhackProtectionOutput;

		public LayerMask mask;

		private void Start()
		{
			NoclipProtection = !ConfigFile.ServerConfig.GetBool("noclip_v3_protection_disabled");
			NoclipProtectionOutput = ConfigFile.ServerConfig.GetBool("noclip_protection_output");
			SpeedhackProtectionOutput = ConfigFile.ServerConfig.GetBool("speedhack_protection_output");
			scp173 = GetComponent<Scp173PlayerScript>();
			scp096 = GetComponent<Scp096PlayerScript>();
			if (!TutorialManager.status)
			{
				ccm = GetComponent<CharacterClassManager>();
				pms = GetComponent<PlyMovementSync>();
				Timing.RunCoroutine(_AntiSpeedhack(), Segment.Update);
				if (NetworkServer.active)
				{
					Timing.RunCoroutine(CheckPosition(), Segment.Update);
				}
			}
		}

		private IEnumerator<float> CheckPosition()
		{
			if (!XZVectorCompare(base.transform.position, prevPos) && !CheckMovement(base.transform.position))
			{
				pms.SetPosition(prevPos);
			}
			yield return Timing.WaitForSeconds(0.5f);
		}

		public bool CheckMovement(Vector3 pos)
		{
			if (TutorialManager.status || ccm.curClass < 0 || ccm.curClass == 2)
			{
				prevPos = pos;
				return true;
			}
			distanceTraveled += Vector2.Distance(new Vector2(prevPos.x, prevPos.z), new Vector2(pos.x, pos.z));
			if (ccm.curClass == 0)
			{
				maxDistance = ((!scp173.CanMove()) ? 3f : (scp173.boost_teleportDistance.Evaluate(GetComponent<PlayerStats>().GetHealthPercent()) * 2f));
			}
			else if (ccm.curClass > 0)
			{
				maxDistance = ccm.klasy[ccm.curClass].runSpeed;
			}
			if (ccm.curClass == 9 && scp096.enraged == Scp096PlayerScript.RageState.Enraged)
			{
				maxDistance *= 4.9f;
			}
			if (ccm.curClass == 7)
			{
				maxDistance = 1f;
			}
			if (distanceTraveled < maxDistance * 1.3f)
			{
				List<RaycastHit> hitInfo;
				if (NoclipProtection && MoreCast.BeamCast(prevPos, pos, new Vector3(0.1f, 0f, 0.1f), 0.1f, out hitInfo, mask, false))
				{
					bool flag = true;
					foreach (RaycastHit item in hitInfo)
					{
						if (item.collider.isTrigger)
						{
							flag = false;
							continue;
						}
						Door componentInParent = item.collider.GetComponentInParent<Door>();
						if (!(componentInParent == null))
						{
							if (ccm.curClass == 3 && (!componentInParent.locked || componentInParent.destroyed))
							{
								flag = false;
							}
							else if (componentInParent.curCooldown > 0.7f)
							{
								flag = false;
							}
							else if (ccm.curClass == 9 && componentInParent.destroyedPrefab != null && GetComponent<Scp096PlayerScript>().enraged == Scp096PlayerScript.RageState.Enraged)
							{
								flag = false;
							}
						}
					}
					if (flag)
					{
						if (NoclipProtectionOutput)
						{
							ServerConsole.AddLog("Player " + GetComponent<NicknameSync>().myNick + " (" + GetComponent<CharacterClassManager>().SteamId + ") tried to noclip (code 3.1).");
						}
						ccm.TargetConsolePrint(base.connectionToClient, "You have tried to noclip (code: 3.1).", "gray");
						return false;
					}
				}
				prevPos = pos;
				return true;
			}
			if (SpeedhackProtectionOutput)
			{
				ServerConsole.AddLog("Player " + GetComponent<NicknameSync>().myNick + " (" + GetComponent<CharacterClassManager>().SteamId + ") tried to speedhack (code: 3.2).");
			}
			return false;
		}

		private IEnumerator<float> _AntiSpeedhack()
		{
			while (this != null)
			{
				distanceTraveled = 0f;
				yield return Timing.WaitForSeconds(1f);
			}
		}

		public void SetPosition(Vector3 pos)
		{
			prevPos = pos;
			pms.groundedY = pos.y;
			pms.FlyTime = 0f;
			distanceTraveled = 0f;
		}

		public bool XZVectorCompare(Vector3 a, Vector3 b)
		{
			return Mathf.Abs(a.x - b.x) < 0.05f && Math.Abs(b.z - b.z) < 0.05f;
		}

		private void UNetVersion()
		{
		}

		public override bool OnSerialize(NetworkWriter writer, bool forceAll)
		{
			bool result = default(bool);
			return result;
		}

		public override void OnDeserialize(NetworkReader reader, bool initialState)
		{
		}
	}
}
