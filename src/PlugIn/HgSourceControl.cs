using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

using Exortech.NetReflector;

using NHg;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Core.Util;

namespace CruiseControl.Mercurial
{
	[ReflectorType("hgNoSuck")]
	public class HgSourceControl : ProcessSourceControl
	{
		private static readonly Version VersionWarningThreshold = new Version(1, 5);
		private readonly HgXmlLogParser _hgXmlLogParser;

		public HgSourceControl()
			: this(
				new HgXmlLogParser(),
				new ProcessExecutor()
				) {}

		public HgSourceControl(HgXmlLogParser hgXmlLogParser, ProcessExecutor executor)
			: base(hgXmlLogParser.ToHistoryParser(), executor)
		{
			if (hgXmlLogParser == null)
			{
				throw new ArgumentNullException("hgXmlLogParser");
			}
			if (executor == null)
			{
				throw new ArgumentNullException("executor");
			}

			_hgXmlLogParser = hgXmlLogParser;

			SetDefaults();
		}

		[ReflectorProperty("workingDirectory", Required = false)]
		public string WorkingDirectory { get; set; }

		[ReflectorProperty("repo", Required = true)]
		public string SourceRepository { get; set; }

		[ReflectorProperty("branch", Required = false)]
		public string BranchRaw { get; set; }

		[ReflectorProperty("tag", Required = false)]
		public string TagRaw { get; set; }

		[ReflectorProperty("failIfMultipleHeads", Required = false)]
		public bool FailIfMultipleHeads { get; set; }

		[ReflectorProperty("autoGetSource", Required = false)]
		public bool AutoGetSource { get; set; }

		[ReflectorProperty("webUrlBuilder", InstanceTypeKey = "type", Required = false)]
		public IModificationUrlBuilder WebUrlBuilder { get; set; }

		private Branch Branch
		{
			get { return new Branch(BranchRaw); }
		}

		private Tag Tag
		{
			get { return string.IsNullOrEmpty(TagRaw) ? Branch.AsTag() : new Tag(TagRaw); }
		}

		private static void CheckVersion(Hg hg)
		{
			Debug.Assert(hg != null);

			var version = hg.Version;
			if (version < VersionWarningThreshold)
			{
				Log.Warning("Hg installed version {0} is less than tested version of {1}.", version, VersionWarningThreshold);
			}
		}

		private static Modification[] ConvertChangesetsToModifications(IEnumerable<Changeset> changesets)
		{
			return changesets.SelectMany(changeset => changeset.ToModifications()).ToArray();
		}

		private static void EnsureRepository(Hg hg)
		{
			Log.Debug("Ensuring '{0}' is a repository.", hg.RepositoryPath);
			if (hg.IsRepository)
			{
				return;
			}
			Log.Info("Creating repository at '{0}'.");
			hg.Init();
		}

		private static void EnsureWorkingDirectory(string workingDirectory)
		{
			Log.Debug("Ensuring working directory '{0}' exists.", workingDirectory);
			if (Directory.Exists(workingDirectory))
			{
				return;
			}
			Log.Info("Working directory '{0}' does not exist. Creating.", workingDirectory);
			Directory.CreateDirectory(workingDirectory);
		}

		public override void LabelSourceControl(IIntegrationResult result)
		{
			return;
		}

		public override Modification[] GetModifications(IIntegrationResult from, IIntegrationResult to)
		{
			if (from == null)
			{
				throw new ArgumentNullException("from");
			}
			if (to == null)
			{
				throw new ArgumentNullException("to");
			}

			var progressInformation = to.BuildProgressInformation;

			progressInformation.SignalStartRunTask("Hg: Get Modifications");

			var lastBuilt = from.GetRevision();
			Log.Debug("Last built from revision '{0}'", lastBuilt);
			var workingDirectory = WorkingDirectoryOrDefault(to.WorkingDirectory);

			progressInformation.AddTaskInformation("Hg: Initializing working directory");
			InitializeWorkingDirectory(workingDirectory);

			var hg = new Hg(new CruiseControlHgProcess(ProcessExecutor), _hgXmlLogParser, workingDirectory);

			progressInformation.AddTaskInformation("Hg: Pulling changes");
			PullChanges(hg);

			progressInformation.AddTaskInformation("Hg: Verifying repository");
			VerifyRepository(hg);

			progressInformation.AddTaskInformation("Hg: Getting target revision");
			var targetRevision = GetTargetRevision(hg);

			progressInformation.AddTaskInformation("Hg: Getting changesets");
			var changesets = GetChangesets(hg, lastBuilt, targetRevision);
			Log.Debug("{0} Hg changeset(s) found", changesets.Count());

			to.SetRevision(targetRevision);

			progressInformation.AddTaskInformation("Hg: Generating modifications");
			return ConvertChangesetsToModifications(changesets)
				.TapIf(() => WebUrlBuilder != null, ems => WebUrlBuilder.SetupModification(ems))
				.Tap(FillIssueUrl);
		}

		public override void Initialize(IProject project)
		{
			if (project == null)
			{
				throw new ArgumentNullException("project");
			}

			var workingDirectory = WorkingDirectoryOrDefault(project.WorkingDirectory);
			InitializeWorkingDirectory(workingDirectory);
		}

		public override void GetSource(IIntegrationResult result)
		{
			if (!AutoGetSource)
			{
				return;
			}

			if (result == null)
			{
				throw new ArgumentNullException("result");
			}

			var workingDirectory = WorkingDirectoryOrDefault(result.WorkingDirectory);

			var hg = new Hg(new CruiseControlHgProcess(ProcessExecutor), _hgXmlLogParser, workingDirectory);

			UpdateWorkingDirectory(hg);
		}

		private void InitializeWorkingDirectory(string workingDirectory)
		{
			Debug.Assert(!string.IsNullOrEmpty(workingDirectory));
			EnsureWorkingDirectory(workingDirectory);
			var hg = new Hg(new CruiseControlHgProcess(ProcessExecutor), _hgXmlLogParser, workingDirectory);
			EnsureRepository(hg);
			CheckVersion(hg);
		}

		private void SetDefaults()
		{
			BranchRaw = "default";
			TagRaw = "";
			WorkingDirectory = "";
			FailIfMultipleHeads = true;
			AutoGetSource = true;
		}

		private Revision GetTargetRevision(Hg hg)
		{
			Debug.Assert(hg != null);
			return hg.GetRevisionForTag(Tag);
		}

		private void VerifyRepository(Hg hg)
		{
			Debug.Assert(hg != null);
			if (FailIfMultipleHeads && hg.Heads(Branch).Count.Tap(i => Log.Debug("{0} head(s) in repository", i)) != 1)
			{
				var message = string.Format(CultureInfo.CurrentCulture, "Multiple or no heads in branch '{0}'.", Branch);
				throw new HgSourceControlException(message);
			}
			if (hg.GetBranchForTag(Tag).Tap(b => Log.Debug("Branch for tag '{0}' is '{1}'", Tag, b)) != Branch)
			{
				var message = string.Format(CultureInfo.CurrentCulture, "Tag '{0}' is not on branch '{1}'.", Tag, Branch);
				throw new HgSourceControlException(message);
			}
		}

		private IEnumerable<Changeset> GetChangesets(Hg hg,
		                                             Revision fromRevision,
		                                             Revision toRevision)
		{
			Debug.Assert(hg != null);
			Debug.Assert(fromRevision != null);
			Debug.Assert(toRevision != null);

			return hg
				.Log(Branch, fromRevision.Through(toRevision))
				.Where(changeset => changeset.Hash.ToRevision() != fromRevision);
		}

		private void UpdateWorkingDirectory(Hg hg)
		{
			Debug.Assert(hg != null);
			Log.Debug("Updating to tag or branch '{0}'.", Tag);
			hg.Update(Tag.AsRevision());
		}

		private void PullChanges(Hg hg)
		{
			Debug.Assert(hg != null);
			hg.Pull(SourceRepository);
		}

		private string WorkingDirectoryOrDefault(string defaultWorkingDirectory)
		{
			return string.IsNullOrEmpty(WorkingDirectory)
			       	? defaultWorkingDirectory
			       	: Path.Combine(defaultWorkingDirectory, WorkingDirectory);
		}
	}
}
