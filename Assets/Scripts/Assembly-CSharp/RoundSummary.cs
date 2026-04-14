using System;
using System.Collections.Generic;
using GameConsole;
using MEC;
using TMPro;
using Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RoundSummary : NetworkBehaviour
{
	public enum LeadingTeam
	{
		FacilityForces = 0,
		ChaosInsurgency = 1,
		Anomalies = 2,
		Draw = 3
	}

	[Serializable]
	public struct SumInfo_ClassList
	{
		public int class_ds;

		public int scientists;

		public int chaos_insurgents;

		public int mtf_and_guards;

		public int scps_except_zombies;

		public int zombies;

		public int warhead_kills;

		public int time;
	}

	private bool roundEnded;

	private SumInfo_ClassList classlistStart;

	public Image fadeOutImage;

	public GameObject ui_root;

	public TextMeshProUGUI ui_text_who_won;

	public TextMeshProUGUI ui_text_info;

	public static RoundSummary singleton;

	public static int roundTime;

	public static int Damages;

	public static int Kills;

	public static int escaped_ds;

	public static int escaped_scientists;

	public static int kills_by_scp;

	public static int changed_into_zombies;

	private static int kRpcRpcShowRoundSummary;

	private static int kRpcRpcDimScreen;

	private void Start()
	{
		if (NetworkServer.active)
		{
			roundTime = 0;
			singleton = this;
			Timing.RunCoroutine(_ProcessServerSideCode(), Segment.Update);
			kills_by_scp = 0;
			escaped_ds = 0;
			escaped_scientists = 0;
			changed_into_zombies = 0;
			Damages = 0;
			Kills = 0;
		}
	}

	public void SetStartClassList(SumInfo_ClassList info)
	{
		classlistStart = info;
	}

	private IEnumerator<float> _ProcessServerSideCode()
	{
		while (this != null)
		{
			while (!RoundInProgress() || PlayerManager.singleton.players.Length < 2)
			{
				yield return 0f;
			}
			SumInfo_ClassList newList = default(SumInfo_ClassList);
			GameObject[] players = PlayerManager.singleton.players;
			foreach (GameObject ply in players)
			{
				if (!(ply != null))
				{
					continue;
				}
				CharacterClassManager ccm = ply.GetComponent<CharacterClassManager>();
				if (ccm.curClass >= 0)
				{
					switch (ccm.klasy[ccm.curClass].team)
					{
					case Team.CDP:
						newList.class_ds++;
						break;
					case Team.CHI:
						newList.chaos_insurgents++;
						break;
					case Team.MTF:
						newList.mtf_and_guards++;
						break;
					case Team.RSC:
						newList.scientists++;
						break;
					case Team.SCP:
						if (ccm.curClass == 10)
						{
							newList.zombies++;
						}
						else
						{
							newList.scps_except_zombies++;
						}
						break;
					}
				}
				yield return 0f;
			}
			newList.warhead_kills = ((!AlphaWarheadController.host.detonated) ? (-1) : AlphaWarheadController.host.warheadKills);
			yield return 0f;
			newList.time = (int)Time.realtimeSinceStartup;
			yield return 0f;
			roundTime = newList.time - classlistStart.time;
			int _facilityForces = newList.mtf_and_guards + newList.scientists;
			int _chaosInsurgency = newList.chaos_insurgents + newList.class_ds;
			int _anomalies = newList.scps_except_zombies + newList.zombies;
			float _d_escape_percentage = ((classlistStart.class_ds != 0) ? ((escaped_ds + newList.class_ds) / classlistStart.class_ds) : 0);
			float _s_escape_percentage = ((classlistStart.scientists == 0) ? 1 : ((escaped_scientists + newList.scientists) / classlistStart.scientists));
			if (newList.class_ds == 0 && _facilityForces == 0)
			{
				roundEnded = true;
			}
			else
			{
				int num = 0;
				if (_facilityForces > 0)
				{
					num++;
				}
				if (_chaosInsurgency > 0)
				{
					num++;
				}
				if (_anomalies > 0)
				{
					num++;
				}
				if (num <= 1)
				{
					roundEnded = true;
				}
			}
			if (!roundEnded)
			{
				continue;
			}
			LeadingTeam leadingTeam = LeadingTeam.Draw;
			if (_facilityForces > 0)
			{
				if (escaped_ds == 0 && escaped_scientists != 0)
				{
					leadingTeam = LeadingTeam.FacilityForces;
				}
			}
			else
			{
				leadingTeam = ((escaped_ds != 0) ? LeadingTeam.ChaosInsurgency : LeadingTeam.Anomalies);
			}
			string textSum = "Round finished! Anomalies: " + _anomalies + " | Chaos: " + _chaosInsurgency + " | Facility Forces: " + _facilityForces + " | D escaped percentage: " + _d_escape_percentage + " | S escaped percentage: : " + _s_escape_percentage;
			GameConsole.Console.singleton.AddLog(textSum, Color.gray);
			ServerLogs.AddLog(ServerLogs.Modules.Logger, textSum, ServerLogs.ServerLogType.GameEvent);
			yield return Timing.WaitForSeconds(1.5f);
			int timeToRoundRestart = Mathf.Clamp(ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);
			CallRpcShowRoundSummary(classlistStart, newList, leadingTeam, escaped_ds, escaped_scientists, kills_by_scp, timeToRoundRestart);
			yield return Timing.WaitForSeconds(timeToRoundRestart - 1);
			CallRpcDimScreen();
			yield return Timing.WaitForSeconds(1f);
			PlayerManager.localPlayer.GetComponent<PlayerStats>().Roundrestart();
		}
	}

	[ClientRpc]
	private void RpcShowRoundSummary(SumInfo_ClassList list_start, SumInfo_ClassList list_finish, LeadingTeam leadingTeam, int e_ds, int e_sc, int scp_kills, int round_cd)
	{
		Timing.RunCoroutine(_ShowRoundSummary(list_start, list_finish, leadingTeam, e_ds, e_sc, scp_kills, round_cd, changed_into_zombies), Segment.FixedUpdate);
	}

	private IEnumerator<float> _ShowRoundSummary(SumInfo_ClassList list_start, SumInfo_ClassList list_finish, LeadingTeam leadingTeam, int e_ds, int e_sc, int scp_kills, int round_cd, int changedIntoZombies)
	{
		Radio.roundEnded = true;
		string _info = string.Empty;
		switch (leadingTeam)
		{
		case LeadingTeam.Anomalies:
			ui_text_who_won.text = "<color=red> " + TranslationReader.Get("Summary", 1);
			_info = TranslationReader.Get("Summary", 5).Replace("[summary_scpfrags]", "<color=red>" + scp_kills + "</color>");
			break;
		case LeadingTeam.FacilityForces:
			ui_text_who_won.text = "<color=#0096FF> " + TranslationReader.Get("Summary", 2);
			_info = TranslationReader.Get("Summary", 6);
			break;
		case LeadingTeam.ChaosInsurgency:
			ui_text_who_won.text = "<color=#FF8E00> " + TranslationReader.Get("Summary", 3);
			_info = TranslationReader.Get("Summary", 7);
			break;
		case LeadingTeam.Draw:
			ui_text_who_won.text = "<color=#FEFEFE> " + TranslationReader.Get("Summary", 4);
			_info = TranslationReader.Get("Summary", 8);
			break;
		}
		ui_text_who_won.text += "</color>";
		int seconds = list_finish.time - list_start.time;
		int minutes = 0;
		while (seconds >= 60)
		{
			seconds -= 60;
			minutes++;
		}
		int livingSCPs = list_finish.scps_except_zombies + list_finish.zombies;
		int maxSCPs = list_start.scps_except_zombies + changedIntoZombies;
		_info = _info + "\n" + ((list_finish.warhead_kills != -1) ? TranslationReader.Get("Summary", 11).Replace("[summary_warhead_kills]", "<color=red>" + list_finish.warhead_kills + "</color>") : TranslationReader.Get("Summary", 12));
		_info = _info + "\n" + TranslationReader.Get("Summary", 9).Replace("[summary_d_escaped]", "<color=red>" + (e_ds + list_finish.class_ds) + "</color>/<color=red>" + list_start.class_ds + "</color>");
		_info = _info + "\n" + TranslationReader.Get("Summary", 10).Replace("[summary_s_escaped]", "<color=red>" + (e_sc + list_finish.scientists) + "</color>/<color=red>" + list_start.scientists + "</color>");
		_info = _info + "\n" + TranslationReader.Get("Summary", 14).Replace("[summary_scp_terminated]", "<color=red>" + (maxSCPs - livingSCPs) + "</color>/<color=red>" + maxSCPs + "</color>");
		_info = _info + "\n\n" + TranslationReader.Get("Summary", 16).Replace("[summary_damages]", "<color=red>" + Damages + "</color>").Replace("[summary_kills]", "<color=red>" + Kills + "</color>");
		_info = _info + "\n\n" + TranslationReader.Get("Summary", 13).Replace("[summary_round_minutes]", "<color=red>" + minutes + "</color>").Replace("[summary_round_seconds]", "<color=red>" + seconds + "</color>");
		_info = _info + "\n" + TranslationReader.Get("Summary", 15).Replace("[summary_next_round_countdown]", "<color=red>" + round_cd + "</color>");
		ui_text_info.text = _info;
		ui_root.gameObject.SetActive(true);
		CanvasRenderer[] renderers = GetComponentsInChildren<CanvasRenderer>();
		GetComponent<RectTransform>().localPosition = Vector3.zero;
		ui_root.gameObject.GetComponent<RectTransform>().localPosition = Vector3.zero;
		for (float deltaTime = 0f; deltaTime < 0.8f; deltaTime += 0.02f)
		{
			CanvasRenderer[] array = renderers;
			foreach (CanvasRenderer canvasRenderer in array)
			{
				canvasRenderer.SetAlpha(deltaTime);
			}
			yield return 0f;
		}
	}

	[ClientRpc]
	private void RpcDimScreen()
	{
		Timing.RunCoroutine(_FadeScreenOut(), Segment.FixedUpdate);
	}

	private IEnumerator<float> _FadeScreenOut()
	{
		float _s_time = 0f;
		while (_s_time < 1f)
		{
			_s_time += 0.04f;
			fadeOutImage.color = Color.Lerp(Color.clear, Color.black, _s_time);
			yield return 0f;
		}
	}

	public static bool RoundInProgress()
	{
		return PlayerManager.localPlayer != null && PlayerManager.localPlayer.GetComponent<CharacterClassManager>().roundStarted && !singleton.roundEnded && AlphaWarheadController.host != null;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcShowRoundSummary(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShowRoundSummary called on server.");
		}
		else
		{
			((RoundSummary)obj).RpcShowRoundSummary(GeneratedNetworkCode._ReadSumInfo_ClassList_RoundSummary(reader), GeneratedNetworkCode._ReadSumInfo_ClassList_RoundSummary(reader), (LeadingTeam)reader.ReadInt32(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcDimScreen(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDimScreen called on server.");
		}
		else
		{
			((RoundSummary)obj).RpcDimScreen();
		}
	}

	public void CallRpcShowRoundSummary(SumInfo_ClassList list_start, SumInfo_ClassList list_finish, LeadingTeam leadingTeam, int e_ds, int e_sc, int scp_kills, int round_cd)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcShowRoundSummary called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcShowRoundSummary);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteSumInfo_ClassList_RoundSummary(networkWriter, list_start);
		GeneratedNetworkCode._WriteSumInfo_ClassList_RoundSummary(networkWriter, list_finish);
		networkWriter.Write((int)leadingTeam);
		networkWriter.WritePackedUInt32((uint)e_ds);
		networkWriter.WritePackedUInt32((uint)e_sc);
		networkWriter.WritePackedUInt32((uint)scp_kills);
		networkWriter.WritePackedUInt32((uint)round_cd);
		SendRPCInternal(networkWriter, 0, "RpcShowRoundSummary");
	}

	public void CallRpcDimScreen()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcDimScreen called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcDimScreen);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcDimScreen");
	}

	static RoundSummary()
	{
		kRpcRpcShowRoundSummary = -1028913821;
		NetworkBehaviour.RegisterRpcDelegate(typeof(RoundSummary), kRpcRpcShowRoundSummary, InvokeRpcRpcShowRoundSummary);
		kRpcRpcDimScreen = 784848710;
		NetworkBehaviour.RegisterRpcDelegate(typeof(RoundSummary), kRpcRpcDimScreen, InvokeRpcRpcDimScreen);
		NetworkCRC.RegisterBehaviour("RoundSummary", 0);
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
