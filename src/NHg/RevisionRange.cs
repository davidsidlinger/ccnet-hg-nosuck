using System;
using System.Globalization;
using System.Linq;

namespace NHg
{
	public class RevisionRange : IEquatable<RevisionRange>
	{
		private readonly Revision _start;
		private readonly Revision _finish;

		public RevisionRange(Revision start, Revision finish)
		{
			if (start == null)
			{
				throw new ArgumentNullException("start");
			}
			if (finish == null)
			{
				throw new ArgumentNullException("finish");
			}

			_start = start;
			_finish = finish;
		}

		public static RevisionRange NotSpecified
		{
			get { return new RevisionRange(Revision.NotSpecified, Revision.NotSpecified); }
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", _start, _finish);
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
			return obj.GetType() == typeof(RevisionRange) && Equals((RevisionRange)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_start.GetHashCode() * 397) ^ _finish.GetHashCode();
			}
		}

		public static bool operator ==(RevisionRange left, RevisionRange right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(RevisionRange left, RevisionRange right)
		{
			return !Equals(left, right);
		}

		#region IEquatable<RevisionRange> Members

		public bool Equals(RevisionRange other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return Equals(other._start, _start) && Equals(other._finish, _finish);
		}

		#endregion
	}
}
