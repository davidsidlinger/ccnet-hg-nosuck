using System;
using System.Diagnostics;
using System.IO;

namespace NHg.Fixtures
{
	[DebuggerStepThrough]
	public class SelfCleaningDirectory : IDisposable
	{
		private readonly string _path;

		public SelfCleaningDirectory()
		{
			_path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
			Directory.CreateDirectory(_path);
		}

		public string Path
		{
			get { return _path; }
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (_path != null)
			{
				Directory.Delete(_path, true);
			}
		}

		#endregion
	}
}
