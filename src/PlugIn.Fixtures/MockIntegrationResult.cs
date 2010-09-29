using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace CruiseControl.Mercurial.Fixtures
{
	public class MockIntegrationResult : IIntegrationResult
	{
		private readonly List<NameValuePair> _sourceControlData = new List<NameValuePair>();

		private readonly BuildProgressInformation _buildProgressInformation = new BuildProgressInformation(
			Path.GetTempPath(),
			"Test Project");

		#region IIntegrationResult Members

		public string BaseFromArtifactsDirectory(string pathToBase)
		{
			throw new NotImplementedException();
		}

		public string BaseFromWorkingDirectory(string pathToBase)
		{
			throw new NotImplementedException();
		}

		public void MarkStartTime()
		{
			throw new NotImplementedException();
		}

		public void MarkEndTime()
		{
			throw new NotImplementedException();
		}

		public bool IsInitial()
		{
			throw new NotImplementedException();
		}

		public void AddTaskResult(string result)
		{
			throw new NotImplementedException();
		}

		public void AddTaskResult(ITaskResult result)
		{
			throw new NotImplementedException();
		}

		public void AddTaskResultFromFile(string filename)
		{
			throw new NotImplementedException();
		}

		public void AddTaskResultFromFile(string filename, bool wrapInCData)
		{
			throw new NotImplementedException();
		}

		public bool HasModifications()
		{
			throw new NotImplementedException();
		}

		public bool ShouldRunBuild()
		{
			throw new NotImplementedException();
		}

		public IIntegrationResult Clone()
		{
			throw new NotImplementedException();
		}

		public void Merge(IIntegrationResult value)
		{
			throw new NotImplementedException();
		}

		public string ProjectName
		{
			get { throw new NotImplementedException(); }
		}

		public string ProjectUrl
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public string WorkingDirectory { get; set; }

		public string ArtifactDirectory
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public string BuildLogDirectory
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public List<NameValuePair> Parameters
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public BuildCondition BuildCondition
		{
			get { throw new NotImplementedException(); }
		}

		public string Label
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public IntegrationStatus Status
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public DateTime StartTime
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public DateTime EndTime
		{
			get { throw new NotImplementedException(); }
		}

		public TimeSpan TotalIntegrationTime
		{
			get { throw new NotImplementedException(); }
		}

		public bool Failed
		{
			get { throw new NotImplementedException(); }
		}

		public bool Fixed
		{
			get { throw new NotImplementedException(); }
		}

		public bool Succeeded
		{
			get { throw new NotImplementedException(); }
		}

		public IntegrationRequest IntegrationRequest
		{
			get { throw new NotImplementedException(); }
		}

		public IntegrationStatus LastIntegrationStatus
		{
			get { throw new NotImplementedException(); }
		}

		public ArrayList FailureUsers
		{
			get { throw new NotImplementedException(); }
		}

		public DateTime LastModificationDate
		{
			get { throw new NotImplementedException(); }
		}

		public string LastChangeNumber
		{
			get { throw new NotImplementedException(); }
		}

		public IntegrationSummary LastIntegration
		{
			get { throw new NotImplementedException(); }
		}

		public string LastSuccessfulIntegrationLabel
		{
			get { throw new NotImplementedException(); }
		}

		public IList TaskResults
		{
			get { throw new NotImplementedException(); }
		}

		public Modification[] Modifications
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public Exception ExceptionResult
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public string TaskOutput
		{
			get { throw new NotImplementedException(); }
		}

		public Exception SourceControlError
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public bool HasSourceControlError
		{
			get { throw new NotImplementedException(); }
		}

		public IntegrationStatus LastBuildStatus
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public IDictionary IntegrationProperties
		{
			get { throw new NotImplementedException(); }
		}

		public BuildProgressInformation BuildProgressInformation
		{
			get { return _buildProgressInformation; }
		}

		public List<NameValuePair> SourceControlData
		{
			get { return _sourceControlData; }
		}

		#endregion
	}
}
