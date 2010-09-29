using System;
using System.IO;
using System.Linq;

namespace NHg
{
	public class ChangesetPath
	{
		private readonly ChangeAction _action;
		private readonly string _path;

		public ChangesetPath(ChangeAction action, string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentOutOfRangeException("path", path, "Path cannot be null or empty.");
			}

			_action = action;
			_path = path.Trim();
		}

		public ChangeAction Action
		{
			get { return _action; }
		}

		public string DirectoryName
		{
			get { return Path.GetDirectoryName(_path); }
		}

		public string FileName
		{
			get { return Path.GetFileName(_path); }
		}
	}
}
