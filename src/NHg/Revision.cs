using System;
using System.Linq;

namespace NHg
{
	public class Revision : IEquatable<Revision>
	{
		private static readonly Revision _first = new Revision("0");
		private static readonly Revision _beforeFirst = new Revision("000000000000");
		private readonly string _value;

		public Revision(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentOutOfRangeException("value", value, "Value cannot be null or empty.");
			}
			_value = value;
		}

		public static Revision First
		{
			get { return _first; }
		}

		public static Revision BeforeFirst
		{
			get { return _beforeFirst; }
		}

		public static Revision NotSpecified
		{
			get { return new Revision(Hg.NotSpecified); }
		}

		public override string ToString()
		{
			return Equals(NotSpecified) ? "" : _value;
		}

		public RevisionRange Through(Revision end)
		{
			if (end == null)
			{
				throw new ArgumentNullException("end");
			}
			return new RevisionRange(this, end);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			return obj.GetType() == typeof(Revision) && Equals((Revision)obj);
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		public static bool operator ==(Revision left, Revision right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Revision left, Revision right)
		{
			return !Equals(left, right);
		}

		#region IEquatable<Revision> Members

		public bool Equals(Revision other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			if (Hash.IsHash(_value) && Hash.IsHash(other._value))
			{
				return Equals(new Hash(_value), new Hash(other._value));
			}
			return Equals(other._value, _value);
		}

		#endregion
	}
}
