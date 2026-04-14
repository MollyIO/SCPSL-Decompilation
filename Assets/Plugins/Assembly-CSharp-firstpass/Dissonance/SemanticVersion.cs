using System;
using Dissonance.Extensions;
using UnityEngine;

namespace Dissonance
{
	[Serializable]
	public class SemanticVersion : IComparable<SemanticVersion>, IEquatable<SemanticVersion>
	{
		[SerializeField]
		private int _major;

		[SerializeField]
		private int _minor;

		[SerializeField]
		private int _patch;

		[SerializeField]
		private string _tag;

		public int Major
		{
			get
			{
				return _major;
			}
		}

		public int Minor
		{
			get
			{
				return _minor;
			}
		}

		public int Patch
		{
			get
			{
				return _patch;
			}
		}

		public string Tag
		{
			get
			{
				return _tag;
			}
		}

		public SemanticVersion()
		{
		}

		public SemanticVersion(int major, int minor, int patch, [CanBeNull] string tag = null)
		{
			_major = major;
			_minor = minor;
			_patch = patch;
			_tag = tag;
		}

		public int CompareTo([CanBeNull] SemanticVersion other)
		{
			if (other == null)
			{
				return 1;
			}
			if (!Major.Equals(other.Major))
			{
				return Major.CompareTo(other.Major);
			}
			if (!Minor.Equals(other.Minor))
			{
				return Minor.CompareTo(other.Minor);
			}
			if (!Patch.Equals(other.Patch))
			{
				return Patch.CompareTo(other.Patch);
			}
			if (Tag != other.Tag)
			{
				if (Tag != null && other.Tag == null)
				{
					return -1;
				}
				if (Tag == null && other.Tag != null)
				{
					return 1;
				}
				return string.Compare(Tag, other.Tag, StringComparison.Ordinal);
			}
			return 0;
		}

		public override string ToString()
		{
			if (Tag == null)
			{
				return string.Format("{0}.{1}.{2}", Major, Minor, Patch);
			}
			return string.Format("{0}.{1}.{2}-{3}", Major, Minor, Patch, Tag);
		}

		public bool Equals(SemanticVersion other)
		{
			if (object.ReferenceEquals(null, other))
			{
				return false;
			}
			if (object.ReferenceEquals(this, other))
			{
				return true;
			}
			return CompareTo(other) == 0;
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}
			if (object.ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((SemanticVersion)obj);
		}

		public override int GetHashCode()
		{
			int major = _major;
			major = (major * 397) ^ _minor;
			major = (major * 397) ^ _patch;
			return (major * 397) ^ ((_tag != null) ? _tag.GetFnvHashCode() : 0);
		}
	}
}
