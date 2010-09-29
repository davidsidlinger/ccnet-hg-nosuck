using System;
using System.Linq;

namespace NHg
{
	public class Tag : IEquatable<Tag>
	{
		private readonly string _value;

		public Tag(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentOutOfRangeException("value", value, "Value cannot be null or empty.");
			}
			_value = value;
		}

		public static Tag NotSpecified
		{
			get { return new Tag(Hg.NotSpecified); }
		}

		public override string ToString()
		{
			return Equals(NotSpecified) ? "" : _value;
		}

		public Revision AsRevision()
		{
			return Equals(NotSpecified) ? Revision.NotSpecified : new Revision(_value);
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
			return obj.GetType() == typeof(Tag) && Equals((Tag)obj);
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		public static bool operator ==(Tag left, Tag right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Tag left, Tag right)
		{
			return !Equals(left, right);
		}

		#region IEquatable<Tag> Members

		public bool Equals(Tag other)
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
