using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

internal class QueryServer
{
	private readonly int _port;

	private readonly bool _useV6;

	internal List<QueryUser> Users;

	private Thread _thr;

	private Thread _checkThr;

	internal Stopwatch Stopwatch;

	private TcpListener _listner;

	private TcpListener _listnerv6;

	internal int TimeoutThreshold = 10;

	private bool _serverStop;

	internal QueryServer(int p, bool v6)
	{
		_port = p;
		_useV6 = v6;
	}

	internal void StartServer()
	{
		_serverStop = false;
		Stopwatch = new Stopwatch();
		_thr = new Thread(StartUp)
		{
			IsBackground = true
		};
		_thr.Start();
		_checkThr = new Thread(CheckClients)
		{
			IsBackground = true,
			Priority = ThreadPriority.BelowNormal
		};
	}

	private void CheckClients()
	{
		while (!_serverStop)
		{
			for (int num = Users.Count - 1; num >= 0; num--)
			{
				if (!Users.ElementAt(num).IsConnected())
				{
					ServerConsole.AddLog("Query user connected from " + Users.ElementAt(num).Ip + " timed out.");
					try
					{
						Users.ElementAt(num).CloseConn();
						Users.RemoveAt(num);
					}
					catch
					{
					}
				}
			}
			Thread.Sleep(10000);
		}
	}

	internal void StopServer()
	{
		ServerConsole.AddLog("Stopping query server...");
		_checkThr.Abort();
		_serverStop = true;
	}

	private void StartUp()
	{
		ServerConsole.AddLog("Starting query server on port " + _port + " TCP...");
		Users = new List<QueryUser>();
		Stopwatch.Start();
		_checkThr.Start();
		try
		{
			_listner = new TcpListener(IPAddress.Any, _port);
			_listner.Start();
			if (_useV6)
			{
				_listnerv6 = new TcpListener(IPAddress.IPv6Any, _port);
				_listnerv6.Start();
			}
			while (!_serverStop)
			{
				if (_listner.Pending())
				{
					AcceptSocket(_listner);
				}
				else if (_listnerv6.Pending())
				{
					AcceptSocket(_listnerv6);
				}
				else
				{
					Thread.Sleep(500);
				}
			}
			_listner.Stop();
			if (_useV6)
			{
				_listnerv6.Stop();
			}
			foreach (QueryUser user in Users)
			{
				user.CloseConn(true);
			}
			Users.Clear();
			ServerConsole.AddLog("Query server stopped.");
		}
		catch (Exception ex)
		{
			ServerConsole.AddLog("Server ERROR: " + ex.StackTrace);
		}
	}

	private void AcceptSocket(TcpListener lst)
	{
		TcpClient tcpClient = lst.AcceptTcpClient();
		QueryUser item = new QueryUser(this, tcpClient, tcpClient.Client.RemoteEndPoint.ToString());
		Users.Add(item);
		ServerConsole.AddLog(string.Concat("New query connection from ", tcpClient.Client.RemoteEndPoint, " on ", tcpClient.Client.LocalEndPoint, "."));
	}
}
