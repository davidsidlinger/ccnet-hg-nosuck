using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NHg
{
	public class Hash : IEquatable<Hash>
	{
		private static readonly Regex HashRegex = new Regex("^(?<short>[0-9a-f]{12})(?<long>[0-9a-f]{28})?$",
		                                                    RegexOptions.IgnoreCase);

		private readonly string _value;

		public Hash(string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!HashRegex.IsMatch(value))
			{
				throw new ArgumentOutOfRangeException("value", value, "Hash must be 12 or 40 hexadecimal digits.");
			}
			_value = value;
		}

		public bool IsLong
		{
			get { return HashRegex.Match(_value).Groups["long"].Success; }
		}

		public bool IsShort
		{
			get { return !IsLong; }
		}

		public static bool IsHash(string value)
		{
			return !string.IsNullOrEmpty(value) && HashRegex.IsMatch(value);
		}

		public Hash ToShortHash()
		{
			return IsShort ? new Hash(_value) : new Hash(HashRegex.Match(_value).Groups["short"].Value);
		}

		public Revision ToRevision()
		{
			return new Revision(_value);
		}

		public override string ToString()
		{
			return _value;
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
			return obj.GetType() == typeof(Hash) && Equals((Hash)obj);
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		public static bool operator ==(Hash left, Hash right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Hash left, Hash right)
		{
			return !Equals(left, right);
		}

		#region IEquatable<Hash> Members

		public bool Equals(Hash other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			return ReferenceEquals(this, other) || Equals(ToShortHash()._value, other.ToShortHash()._value);
		}

		#endregion
	}
}
