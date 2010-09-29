using System;
using System.Linq;

namespace NHg
{
	public class Branch : IEquatable<Branch>
	{
		private static readonly Branch DefaultInstance = new Branch("default");
		private readonly string _value;

		public Branch(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentOutOfRangeException("value", value, "Value cannot be null or empty.");
			}
			_value = value;
		}

		public static Branch Default
		{
			get { return DefaultInstance; }
		}

		public static Branch NotSpecified
		{
			get { return new Branch(Hg.NotSpecified); }
		}

		public override string ToString()
		{
			return Equals(NotSpecified) ? "" : _value;
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
			return obj.GetType() == typeof(Branch) && Equals((Branch)obj);
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		public Tag AsTag()
		{
			return Equals(NotSpecified) ? Tag.NotSpecified : new Tag(_value);
		}

		public static bool operator ==(Branch left, Branch right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Branch left, Branch right)
		{
			return !Equals(left, right);
		}

		#region IEquatable<Branch> Members

		public bool Equals(Branch other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			return ReferenceEquals(this, other) || Equals(other._value, _value);
		}

		#endregion
	}
}
