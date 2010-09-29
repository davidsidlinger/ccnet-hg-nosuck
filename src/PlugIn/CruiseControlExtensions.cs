using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using NHg;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Sourcecontrol;
using ThoughtWorks.CruiseControl.Remote;

namespace CruiseControl.Mercurial
{
	public static class CruiseControlExtensions
	{
		private const string RevisionKey = "ccnet-hg-nosuck-revision";

		public static Revision GetRevision(this IIntegrationResult integrationResult)
		{
			if (integrationResult == null)
			{
				throw new ArgumentNullException("integrationResult");
			}

			var sourceControlProperties = NameValuePair.ToDictionary(integrationResult.SourceControlData);
			return sourceControlProperties.ContainsKey(RevisionKey)
			       	? new Revision(sourceControlProperties[RevisionKey])
			       	: Revision.BeforeFirst;
		}

		public static void SetRevision(this IIntegrationResult integrationResult, Revision revision)
		{
			if (integrationResult == null)
			{
				throw new ArgumentNullException("integrationResult");
			}

			var sourceControlProperties = NameValuePair.ToDictionary(integrationResult.SourceControlData);
			sourceControlProperties[RevisionKey] = revision.ToString();
			integrationResult.SourceControlData.Clear();
			NameValuePair.Copy(sourceControlProperties, integrationResult.SourceControlData);
		}

		public static IEnumerable<Modification> ToModifications(this Changeset changeset)
		{
			if (changeset == null)
			{
				throw new ArgumentNullException("changeset");
			}

			return changeset.Paths
				.Select(path => new Modification
				                {
				                	ChangeNumber = changeset.Hash.ToString(),
				                	Comment = changeset.Message,
				                	EmailAddress = changeset.Author.Email,
				                	FileName = path.FileName,
				                	FolderName = path.DirectoryName,
				                	ModifiedTime = changeset.Date.ToLocalTime().DateTime,
				                	Type = path.Action.ToString(),
				                	UserName = changeset.Author.Name,
				                	Version = changeset.Hash.ToString(),
				                });
		}

		public static IHistoryParser ToHistoryParser(this HgXmlLogParser hgXmlLogParser)
		{
			if (hgXmlLogParser == null)
			{
				throw new ArgumentNullException("hgXmlLogParser");
			}
			return new HistoryParser(hgXmlLogParser);
		}

		#region Nested type: HistoryParser

		private class HistoryParser : IHistoryParser
		{
			private readonly HgXmlLogParser _hgXmlLogParser;

			internal HistoryParser(HgXmlLogParser hgXmlLogParser)
			{
				Debug.Assert(hgXmlLogParser != null);
				_hgXmlLogParser = hgXmlLogParser;
			}

			#region IHistoryParser Members

			public Modification[] Parse(TextReader history, DateTime from, DateTime to)
			{
				if (history == null)
				{
					throw new ArgumentNullException("history");
				}

				return _hgXmlLogParser.GetChangesets(history).SelectMany(c => c.ToModifications()).ToArray();
			}

			#endregion
		}

		#endregion
	}
}
