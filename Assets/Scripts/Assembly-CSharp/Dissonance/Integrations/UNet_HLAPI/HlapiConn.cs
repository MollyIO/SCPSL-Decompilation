using System;
using UnityEngine.Networking;

namespace Dissonance.Integrations.UNet_HLAPI
{
	public struct HlapiConn : IEquatable<HlapiConn>
	{
		public readonly NetworkConnection Connection;

		public HlapiConn(NetworkConnection connection)
		{
			this = default(HlapiConn);
			Connection = connection;
		}

		public override int GetHashCode()
		{
			return Connection.GetHashCode();
		}

		public override string ToString()
		{
			return Connection.ToString();
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}
			return obj is HlapiConn && Equals((HlapiConn)obj);
		}

		public bool Equals(HlapiConn other)
		{
			if (Connection == null)
			{
				if (other.Connection == null)
				{
					return true;
				}
				return false;
			}
			return Connection.Equals(other.Connection);
		}
	}
}
