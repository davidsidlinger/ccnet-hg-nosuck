using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace NHg
{
	[DebuggerStepThrough]
	public class HgArguments
	{
		private static readonly Regex _whitespaceExpression = new Regex(@"\s",
		                                                                RegexOptions.Compiled | RegexOptions.CultureInvariant);

		private readonly ICollection<string> _arguments = new List<string>();

		private HgArguments() {}

		public static HgArguments Log()
		{
			return new HgArguments().Add("log");
		}

		public static HgArguments Init(string repositoryPath)
		{
			if (string.IsNullOrEmpty(repositoryPath))
			{
				throw new ArgumentOutOfRangeException("repositoryPath", repositoryPath, "RepositoryPath cannot be null or empty.");
			}
			return new HgArguments().Add("init").Add(repositoryPath);
		}

		public static HgArguments Version()
		{
			return new HgArguments().Add("version");
		}

		public static HgArguments Verify()
		{
			return new HgArguments().Add("verify");
		}

		public static HgArguments IdentifyRevision()
		{
			return new HgArguments().Add("identify").Add("--id");
		}

		public static HgArguments IdentifyBranch()
		{
			return new HgArguments().Add("identify").Add("--branch");
		}

		public static HgArguments Parents()
		{
			return new HgArguments().Add("parents");
		}

		public static HgArguments Pull(string sourceRepository)
		{
			return new HgArguments().Add("pull").Add(sourceRepository);
		}

		public static HgArguments Update()
		{
			return new HgArguments().Add("update");
		}

		public static HgArguments ChangeBranch(Branch branch)
		{
			if (branch == null)
			{
				throw new ArgumentNullException("branch");
			}
			if (branch == Branch.NotSpecified)
			{
				throw new ArgumentOutOfRangeException("branch", branch, "Branch must be specified.");
			}
			return new HgArguments().Add("branch").Add(branch.ToString());
		}

		public static HgArguments Commit(string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentOutOfRangeException("message", message, "Message cannot be null or empty.");
			}
			return new HgArguments().Add("commit").Add("--message", message);
		}

		public static HgArguments AddRemove()
		{
			return new HgArguments().Add("addremove");
		}

		public static HgArguments Tag(Revision revision, string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentOutOfRangeException("name", name, "Name cannot be null or empty.");
			}

			return new HgArguments()
				.Add("tag")
				.Add(revision)
				.Add(name);
		}

		public static HgArguments Tag(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentOutOfRangeException("name", name, "Name cannot be null or empty.");
			}

			return new HgArguments()
				.Add("tag")
				.Add(name);
		}

		public static HgArguments Heads(Branch branch)
		{
			if (branch == null)
			{
				throw new ArgumentNullException("branch");
			}

			return new HgArguments().Add("heads").Add(branch.ToString());
		}

		private static string WrapIfNecessary(string value)
		{
			Debug.Assert(!string.IsNullOrEmpty(value));
			var trimmed = value.Trim();
			return _whitespaceExpression.IsMatch(trimmed)
			       	? string.Format(CultureInfo.InvariantCulture, "\"{0}\"", trimmed)
			       	: trimmed;
		}

		public HgArguments OnlyBranch(Branch branch)
		{
			if (branch == null)
			{
				throw new ArgumentNullException("branch");
			}
			return branch != Branch.NotSpecified ? Add("--only-branch", branch.ToString()) : this;
		}

		public HgArguments AddRevisionRange(RevisionRange revisionRange)
		{
			if (revisionRange == null)
			{
				throw new ArgumentNullException("revisionRange");
			}
			return revisionRange != RevisionRange.NotSpecified ? Add("--rev", revisionRange.ToString()) : this;
		}

		public override string ToString()
		{
			return string.Join(" ", _arguments.ToArray());
		}

		public HgArguments XmlStyle()
		{
			return Add("--style", "xml");
		}

		public HgArguments Verbose()
		{
			return Add("-v");
		}

		public HgArguments Add(Revision revision)
		{
			if (revision == null)
			{
				throw new ArgumentNullException("revision");
			}
			return revision != Revision.NotSpecified ? Add("--rev", revision.ToString()) : this;
		}

		public HgArguments AddRepository(string repository)
		{
			return string.IsNullOrEmpty(repository) ? this : Add("--repository", repository);
		}

		public HgArguments NonInteractive()
		{
			return Add("--noninteractive");
		}

		public HgArguments Similarity(int similarityPercentage)
		{
			var percentage = Math.Max(Math.Min(similarityPercentage, 100), 0);
			return Add("--similarity", percentage.ToString(CultureInfo.InvariantCulture));
		}

		public HgArguments Force()
		{
			return Add("--force");
		}

		private HgArguments Add(string argument)
		{
			Debug.Assert(!string.IsNullOrEmpty(argument));
			_arguments.Add(WrapIfNecessary(argument));
			return this;
		}

		private HgArguments Add(string argument, string value)
		{
			Add(argument);
			Add(value);
			return this;
		}
	}
}
