using System;

namespace NHg.Fixtures
{
	public class FolderFilePair : IEquatable<FolderFilePair>
	{
		private readonly string _file;
		private readonly string _folder;

		public FolderFilePair(string folder, string file)
		{
			_folder = folder;
			_file = file;
		}

		public string Folder
		{
			get { return _folder; }
		}

		public string File
		{
			get { return _file; }
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
			if (obj.GetType() != typeof(FolderFilePair))
			{
				return false;
			}
			return Equals((FolderFilePair)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((_folder != null ? _folder.GetHashCode() : 0) * 397) ^ (_file != null ? _file.GetHashCode() : 0);
			}
		}

		public static bool operator ==(FolderFilePair left, FolderFilePair right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(FolderFilePair left, FolderFilePair right)
		{
			return !Equals(left, right);
		}

		#region IEquatable<FolderFilePair> Members

		public bool Equals(FolderFilePair other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}
			return Equals(other._folder, _folder) && Equals(other._file, _file);
		}

		#endregion
	}
}
