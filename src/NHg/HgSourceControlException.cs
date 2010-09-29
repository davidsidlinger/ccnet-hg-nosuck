using System;
using System.Linq;
using System.Runtime.Serialization;

namespace NHg
{
	[Serializable]
	public class HgSourceControlException : Exception
	{
		public HgSourceControlException() {}

		public HgSourceControlException(string message) : base(message) {}

		public HgSourceControlException(string message, Exception innerException) : base(message, innerException) {}

		protected HgSourceControlException(SerializationInfo info, StreamingContext context) : base(info, context) {}
	}
}
