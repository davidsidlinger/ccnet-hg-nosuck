using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using log4net;

namespace NHg
{
	public class Hg
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(Hg));
		private readonly HgXmlLogParser _xmlLogParser;
		private readonly IHgProcess _processExecutor;
		private readonly string _repositoryPath;

		public Hg(IHgProcess processExecutor, HgXmlLogParser xmlLogParser, string repositoryPath)
		{
			if (processExecutor == null)
			{
				throw new ArgumentNullException("processExecutor");
			}
			if (xmlLogParser == null)
			{
				throw new ArgumentNullException("xmlLogParser");
			}
			if (string.IsNullOrEmpty(repositoryPath))
			{
				throw new ArgumentOutOfRangeException("repositoryPath", repositoryPath, "RepositoryPath cannot be null or empty.");
			}
			VerifyRepositoryPath(repositoryPath);
			_processExecutor = processExecutor;
			_xmlLogParser = xmlLogParser;
			_repositoryPath = repositoryPath;
			_log.DebugCurrent("Created Hg with repository path of '{0}'", _repositoryPath);
		}

		public static string NotSpecified
		{
			get { return "D87AECA3-934B-4502-94FD-C45F11658A85"; }
		}

		public string RepositoryPath
		{
			get { return _repositoryPath; }
		}

		public virtual Version Version
		{
			get
			{
				_log.DebugCurrent("Getting Hg version");
				var versionArguments = HgArguments.Version();
				var versionResult = ExecuteNonInteractive(versionArguments, "getting version", false);
				if (versionResult.Failed)
				{
					_log.WarnCurrent("Version check failed. 0.0 will be returned. Standard error: {0}", versionResult.StandardError);
					return new Version(0, 0);
				}
				var firstLine = new StringReader(versionResult.StandardOutput).ReadLine();
				var versionMatch = Regex.Match(firstLine, @"version (?<version>\d+(\.\d+)?)");
				return versionMatch.Success
				       	? new Version(versionMatch.Groups["version"].Value).Tap(v => _log.DebugCurrent("Hg version is {0}", v))
				       	: new Version(0, 0).Tap(
				       	                        v =>
				       	                        _log.WarnCurrent("Could not find version information in command output: {0}",
				       	                                         firstLine));
			}
		}

		public virtual bool IsRepository
		{
			get { return Directory.Exists(Path.Combine(_repositoryPath, ".hg")); }
		}

		public virtual Branch CurrentBranch
		{
			get
			{
				_log.Debug("Getting current branch");
				var identifyBranchArguments = HgArguments
					.IdentifyBranch()
					.AddRepository(_repositoryPath);
				var identifyBranchResult = ExecuteNonInteractive(identifyBranchArguments, "getting current branch")
					.StandardOutput
					.Trim();
				var branch = new Branch(identifyBranchResult);
				return branch.Tap(b => _log.DebugCurrent("Current branch is '{0}'", b));
			}
		}

		public virtual Revision CurrentRevision
		{
			get
			{
				_log.Debug("Getting current revision");
				var currentRevisionArguments = HgArguments
					.IdentifyRevision()
					.AddRepository(_repositoryPath);
				var currentRevisionResult = ExecuteNonInteractive(currentRevisionArguments, "get current revision").StandardOutput;
				var revision = new Revision(Regex.Replace(currentRevisionResult, "[^0-9a-f]", "", RegexOptions.IgnoreCase));
				return revision.Tap(r => _log.DebugCurrent("Current revision is '{0}'", r));
			}
		}

		private static void VerifyRepositoryPath(IEnumerable<char> repositoryPath)
		{
			Debug.Assert(repositoryPath != null);

			if (repositoryPath.Any(c => Path.GetInvalidPathChars().Contains(c)))
			{
				throw new ArgumentOutOfRangeException("repositoryPath", repositoryPath, "Repository path must be a valid path.");
			}
		}

		public virtual Revision GetRevisionForTag(Tag tag)
		{
			if (tag == null)
			{
				throw new ArgumentNullException("tag");
			}

			_log.DebugCurrent("Getting revision for tag '{0}'", tag);

			var tagRevisionArguments = HgArguments
				.IdentifyRevision()
				.Add(tag.AsRevision())
				.AddRepository(_repositoryPath);
			var tagRevisionResult = ExecuteNonInteractive(tagRevisionArguments, "revision for tag")
				.StandardOutput
				.Trim();
			return (new Revision(tagRevisionResult)).Tap(r => _log.DebugCurrent("Tag '{0}' is revision '{1}'", tag, r));
		}

		public virtual Branch GetBranchForTag(Tag tag)
		{
			if (tag == null)
			{
				throw new ArgumentNullException("tag");
			}

			_log.DebugCurrent("Getting branch for tag '{0}'", tag);

			var tagBranchArguments = HgArguments
				.IdentifyBranch()
				.Add(tag.AsRevision())
				.AddRepository(_repositoryPath);
			var tagBranchResult = ExecuteNonInteractive(tagBranchArguments, "branch for tag")
				.StandardOutput
				.Trim();
			return (new Branch(tagBranchResult)).Tap(b => _log.DebugCurrent("Tag '{0}' is on branch '{1}'", tag, b));
		}

		public virtual ICollection<Changeset> Log(Branch branch, RevisionRange revisionRange)
		{
			if (branch == null)
			{
				throw new ArgumentNullException("branch");
			}
			if (revisionRange == null)
			{
				throw new ArgumentNullException("revisionRange");
			}

			_log.DebugCurrent("Getting log of branch '{0}' with range '{1}'", branch, revisionRange);

			var logArguments = HgArguments
				.Log()
				.Verbose()
				.XmlStyle()
				.AddRevisionRange(revisionRange)
				.OnlyBranch(branch)
				.AddRepository(_repositoryPath);

			var logResult = ExecuteNonInteractive(logArguments, "read log")
				.StandardOutput
				.Trim();

			return string.IsNullOrEmpty(logResult)
			       	? Enumerable.Empty<Changeset>().ToList()
			       	: _xmlLogParser.GetChangesets(new StringReader(logResult));
		}

		public virtual void Init()
		{
			_log.DebugCurrent("Creating repository at '{0}'", _repositoryPath);
			var initArguments = HgArguments.Init(_repositoryPath);
			ExecuteNonInteractive(initArguments, "init");
		}

		public void Pull(string sourceRepository)
		{
			if (string.IsNullOrEmpty(sourceRepository))
			{
				throw new ArgumentOutOfRangeException("sourceRepository", sourceRepository, "From must not be empty.");
			}

			_log.DebugCurrent("Pulling changes from '{0}' into '{1}'", sourceRepository, _repositoryPath);
			var pullArguments = HgArguments
				.Pull(sourceRepository)
				.AddRepository(_repositoryPath);
			ExecuteNonInteractive(pullArguments, "pull");
		}

		public virtual void Update(Revision revision)
		{
			if (revision == null)
			{
				throw new ArgumentNullException("revision");
			}

			_log.DebugCurrent("Updating '{0}' to revision '{1}'", _repositoryPath, revision);

			var updateArguments = HgArguments
				.Update()
				.Add(revision)
				.AddRepository(_repositoryPath);
			ExecuteNonInteractive(updateArguments, "update");
		}

		public ICollection<Revision> Heads(Branch branch)
		{
			if (branch == null)
			{
				throw new ArgumentNullException("branch");
			}

			_log.DebugCurrent("Getting heads of '{0}' for branch '{1}'", _repositoryPath, branch);

			var headsArguments = HgArguments
				.Heads(branch)
				.AddRepository(_repositoryPath);

			var headsResult = ExecuteNonInteractive(headsArguments, "get heads");

			var headRegex = new Regex(@"changeset\:\D*\d+\:(?<rev>[a-f0-9]{12})", RegexOptions.IgnoreCase);
			var revisions = headRegex.Matches(headsResult.StandardOutput)
				.Cast<Match>()
				.Where(m => m.Success && m.Groups["rev"].Success)
				.Select(m => new Revision(m.Groups["rev"].Value))
				.Select(rev => rev.Tap(r => _log.DebugCurrent("Found head revision '{0}' for branch '{1}'", r, branch)))
				.ToList();
			return revisions;
		}

		public void AddRemove()
		{
			var addRemoveArguments = HgArguments
				.AddRemove()
				.AddRepository(_repositoryPath);
			ExecuteNonInteractive(addRemoveArguments, "add/remove");
		}

		public void Commit(string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentOutOfRangeException("message", message, "Message cannot be null or empty.");
			}
			var commitArguments = HgArguments
				.Commit(message)
				.AddRepository(_repositoryPath);
			ExecuteNonInteractive(commitArguments, "commit");
		}

		public void Tag(Revision revision, string name, bool force)
		{
			if (revision == null)
			{
				throw new ArgumentNullException("revision");
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentOutOfRangeException("name", name, "Name cannot be null or empty.");
			}
			var tagArguments = HgArguments
				.Tag(revision, name)
				.AddRepository(_repositoryPath);
			if (force)
			{
				tagArguments = tagArguments.Force();
			}
			ExecuteNonInteractive(tagArguments, "tag");
		}

		private IHgResult ExecuteNonInteractive(HgArguments hgArguments,
		                                        string attemptedAction,
		                                        bool throwOnProcessFailure = true)
		{
			Debug.Assert(hgArguments != null);
			Debug.Assert(!string.IsNullOrEmpty(attemptedAction));

			var result = _processExecutor.Execute(hgArguments);
			if (throwOnProcessFailure && result.Failed)
			{
				var message = string.Format(CultureInfo.CurrentCulture,
				                            "Error occurred while attempting to {0}.\nStandard Output:\n{1}\nStandard Error:\n{2}",
				                            attemptedAction,
				                            result.StandardOutput,
				                            result.StandardError);
				throw new HgSourceControlException(message);
			}
			return result;
		}
	}
}
