using System;
using System.Collections.Generic;
using System.Linq;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Security;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Remote;

namespace CruiseControl.Mercurial.Fixtures
{
	public class MockProject : IProject
	{
		private readonly string _workingDirectory;

		public MockProject(string workingDirectory)
		{
			_workingDirectory = workingDirectory;
		}

		#region IProject Members

		public IIntegrationResult Integrate(IntegrationRequest request)
		{
			throw new NotImplementedException();
		}

		public void Purge(bool purgeWorkingDirectory, bool purgeArtifactDirectory, bool purgeSourceControlEnvironment)
		{
			throw new NotImplementedException();
		}

		public void Initialize()
		{
			throw new NotImplementedException();
		}

		public ProjectStatus CreateProjectStatus(IProjectIntegrator integrator)
		{
			throw new NotImplementedException();
		}

		public void AbortRunningBuild()
		{
			throw new NotImplementedException();
		}

		public void AddMessage(Message message)
		{
			throw new NotImplementedException();
		}

		public void NotifyPendingState()
		{
			throw new NotImplementedException();
		}

		public void NotifySleepingState()
		{
			throw new NotImplementedException();
		}

		public List<PackageDetails> RetrievePackageList()
		{
			throw new NotImplementedException();
		}

		public List<PackageDetails> RetrievePackageList(string buildName)
		{
			throw new NotImplementedException();
		}

		public string Name
		{
			get { throw new NotImplementedException(); }
		}

		public NameValuePair[] LinkedSites
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public string Category
		{
			get { throw new NotImplementedException(); }
		}

		public string Description
		{
			get { throw new NotImplementedException(); }
		}

		public ITrigger Triggers
		{
			get { throw new NotImplementedException(); }
		}

		public string WebURL
		{
			get { throw new NotImplementedException(); }
		}

		public string WorkingDirectory
		{
			get { return _workingDirectory; }
		}

		public string ArtifactDirectory
		{
			get { throw new NotImplementedException(); }
		}

		public ExternalLink[] ExternalLinks
		{
			get { throw new NotImplementedException(); }
		}

		public string Statistics
		{
			get { throw new NotImplementedException(); }
		}

		public string ModificationHistory
		{
			get { throw new NotImplementedException(); }
		}

		public string RSSFeed
		{
			get { throw new NotImplementedException(); }
		}

		public IIntegrationRepository IntegrationRepository
		{
			get { throw new NotImplementedException(); }
		}

		public string QueueName
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public int QueuePriority
		{
			get { throw new NotImplementedException(); }
		}

		public ProjectActivity CurrentActivity
		{
			get { throw new NotImplementedException(); }
		}

		public IProjectAuthorisation Security
		{
			get { throw new NotImplementedException(); }
		}

		public int MaxSourceControlRetries
		{
			get { throw new NotImplementedException(); }
		}

		public DisplayLevel AskForForceBuildReason
		{
			get { throw new NotImplementedException(); }
		}

		public bool stopProjectOnReachingMaxSourceControlRetries
		{
			get { throw new NotImplementedException(); }
		}

		public Common.SourceControlErrorHandlingPolicy SourceControlErrorHandling
		{
			get { throw new NotImplementedException(); }
		}

		public ProjectInitialState InitialState
		{
			get { throw new NotImplementedException(); }
		}

		public ProjectStartupMode StartupMode
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}
