using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

internal class QueryUser
{
	private readonly NetworkStream _s;

	private readonly QueryServer _server;

	private readonly Thread _thr;

	private readonly Thread _sol;

	private int _lastping;

	private bool _closing;

	private int _invalidPackets;

	private readonly string _querypassword;

	internal readonly string Ip;

	private bool _authenticated;

	private readonly UTF8Encoding _encoder;

	internal QueryUser(QueryServer s, TcpClient c, string ip)
	{
		_s = c.GetStream();
		_server = s;
		Ip = ip;
		Send("Welcome to SCP Secret Laboratory Query Protocol");
		_thr = new Thread(Receive)
		{
			IsBackground = true
		};
		_thr.Start();
		_encoder = new UTF8Encoding();
		_querypassword = ConfigFile.ServerConfig.GetString("administrator_query_password", string.Empty);
		_lastping = Convert.ToInt32(_server.Stopwatch.Elapsed.TotalSeconds) + 5;
		_authenticated = false;
	}

	internal bool IsConnected()
	{
		return _server.Stopwatch.Elapsed.TotalSeconds - (double)_lastping < (double)_server.TimeoutThreshold;
	}

	private void Receive()
	{
		_s.ReadTimeout = 200;
		_s.WriteTimeout = 200;
		while (!_closing)
		{
			try
			{
				byte[] array = new byte[4096];
				int num;
				try
				{
					num = _s.Read(array, 0, 4096);
				}
				catch
				{
					num = -1;
					Thread.Sleep(5);
				}
				if (num <= -1)
				{
					continue;
				}
				List<byte[]> list = AuthenticatedMessage.Decode(array);
				foreach (byte[] item in list)
				{
					string text = _encoder.GetString(item);
					AuthenticatedMessage authenticatedMessage = null;
					try
					{
						text = text.Substring(0, text.LastIndexOf(';'));
					}
					catch
					{
						_invalidPackets++;
						text = text.TrimEnd(default(char));
						if (text.EndsWith(";"))
						{
							text = text.Substring(0, text.Length - 1);
						}
					}
					if (_invalidPackets >= 5)
					{
						if (!_closing)
						{
							Send("Too many invalid packets sent.");
							ServerConsole.AddLog("Query connection from " + Ip + " dropped due to too many invalid packets sent.");
							_server.Users.Remove(this);
							CloseConn();
						}
						break;
					}
					try
					{
						authenticatedMessage = AuthenticatedMessage.AuthenticateMessage(text, TimeBehaviour.CurrentTimestamp(), _querypassword);
					}
					catch (MessageAuthenticationFailureException ex)
					{
						Send("Message can't be authenticated - " + ex.Message);
						ServerConsole.AddLog("Query command from " + Ip + " can't be authenticated - " + ex.Message);
					}
					catch (MessageExpiredException)
					{
						Send("Message expired");
						ServerConsole.AddLog("Query command from " + Ip + " is expired.");
					}
					catch (Exception ex3)
					{
						Send("Error during processing your message.");
						ServerConsole.AddLog("Query command from " + Ip + " can't be processed - " + ex3.Message + ".");
					}
					if (authenticatedMessage == null)
					{
						continue;
					}
					if (!_authenticated && authenticatedMessage.Administrator)
					{
						_authenticated = true;
					}
					string text2 = authenticatedMessage.Message;
					string[] array2 = new string[0];
					if (text2.Contains(" "))
					{
						text2 = text2.Split(' ')[0];
						array2 = authenticatedMessage.Message.Substring(text2.Length + 1).Split(' ');
					}
					text2 = text2.ToLower();
					if (authenticatedMessage.Message == "Ping")
					{
						_invalidPackets = 0;
						_lastping = Convert.ToInt32(_server.Stopwatch.Elapsed.TotalSeconds);
						Send("Pong");
						continue;
					}
					switch (text2)
					{
					case "roundrestart":
					{
						if (!AdminCheck(authenticatedMessage.Administrator))
						{
							break;
						}
						GameObject[] array3 = GameObject.FindGameObjectsWithTag("Player");
						foreach (GameObject gameObject in array3)
						{
							PlayerStats component = gameObject.GetComponent<PlayerStats>();
							if (component.isLocalPlayer && component.isServer)
							{
								component.Roundrestart();
							}
						}
						Send("Round restarted.");
						break;
					}
					case "shutdown":
						if (AdminCheck(authenticatedMessage.Administrator))
						{
							Send("Server is shutting down...");
							Application.Quit();
						}
						break;
					case "warhead":
					{
						AlphaWarheadController host = AlphaWarheadController.host;
						if (array2.Length == 0)
						{
							Send("Synax: warhead (status|detonate|cancel|enable|disable)");
							break;
						}
						switch (array2[0].ToLower())
						{
						case "status":
							if (host.detonated || host.timeToDetonation == 0f)
							{
								Send("Warhead has been detonated.");
							}
							else if (host.inProgress)
							{
								Send("Detonation is in progress.");
							}
							else if (!AlphaWarheadOutsitePanel.nukeside.enabled)
							{
								Send("Warhead is disabled.");
							}
							else if (host.timeToDetonation > AlphaWarheadController.host.RealDetonationTime())
							{
								Send("Warhead is restarting.");
							}
							else
							{
								Send("Warhead is ready to detonation.");
							}
							break;
						case "detonate":
							if (AdminCheck(authenticatedMessage.Administrator))
							{
								AlphaWarheadController.host.StartDetonation();
								Send("Detonation sequence started.");
							}
							break;
						case "cancel":
							if (AdminCheck(authenticatedMessage.Administrator))
							{
								AlphaWarheadController.host.CancelDetonation(null);
								Send("Detonation has been canceled.");
							}
							break;
						case "enable":
							if (AdminCheck(authenticatedMessage.Administrator))
							{
								AlphaWarheadOutsitePanel.nukeside.Networkenabled = true;
								Send("Warhead has been enabled.");
							}
							break;
						case "disable":
							if (AdminCheck(authenticatedMessage.Administrator))
							{
								AlphaWarheadOutsitePanel.nukeside.Networkenabled = false;
								Send("Warhead has been disabled.");
							}
							break;
						default:
							Send("WARHEAD: Unknown subcommand.");
							break;
						}
						break;
					}
					default:
						Send("Command not found");
						break;
					}
				}
			}
			catch (SocketException)
			{
				ServerConsole.AddLog("Query connection from " + Ip + " dropped (SocketException).");
				if (!_closing)
				{
					_server.Users.Remove(this);
					CloseConn();
				}
				break;
			}
			catch
			{
				ServerConsole.AddLog("Query connection from " + Ip + " dropped.");
				if (!_closing)
				{
					_server.Users.Remove(this);
					CloseConn();
				}
				break;
			}
		}
	}

	private bool AdminCheck(bool admin)
	{
		if (!admin)
		{
			Send("Access denied! You need to have administrator permissions.");
		}
		return admin;
	}

	public void CloseConn(bool shuttingdown = false)
	{
		_closing = true;
		if (shuttingdown)
		{
			Send("Server is shutting down...");
		}
		_s.Close();
		if (_thr != null)
		{
			_thr.Abort();
		}
	}

	public void Send(string msg)
	{
		msg = ((_authenticated && !(_querypassword == string.Empty) && !(_querypassword == "none") && _querypassword != null) ? AuthenticatedMessage.GenerateAuthenticatedMessage(msg, TimeBehaviour.CurrentTimestamp(), _querypassword) : AuthenticatedMessage.GenerateNonAuthenticatedMessage(msg));
		Send(Utf8.GetBytes(msg));
	}

	public void Send(byte[] msg)
	{
		try
		{
			byte[] array = AuthenticatedMessage.Encode(msg);
			_s.Write(array, 0, array.Length);
		}
		catch (Exception ex)
		{
			ServerConsole.AddLog("Can't send query response to " + Ip + ": " + ex.StackTrace);
		}
	}
}
