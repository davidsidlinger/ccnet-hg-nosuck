using System;

namespace NHg
{
	public class HgResult : IHgResult
	{
		private readonly bool _failed;
		private readonly string _standardOutput;
		private readonly string _standardError;

		public HgResult(bool failed, string standardOutput, string standardError)
		{
			if (standardOutput == null)
			{
				throw new ArgumentNullException("standardOutput");
			}
			if (standardError == null)
			{
				throw new ArgumentNullException("standardError");
			}

			_failed = failed;
			_standardOutput = standardOutput;
			_standardError = standardError;
		}

		#region IHgResult Members

		public bool Failed
		{
			get { return _failed; }
		}

		public string StandardOutput
		{
			get { return _standardOutput; }
		}

		public string StandardError
		{
			get { return _standardError; }
		}

		#endregion
	}
}