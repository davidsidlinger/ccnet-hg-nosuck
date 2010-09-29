using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using log4net;

namespace NHg
{
	public class HgXmlLogParser
	{
		public static readonly string ChangesetName = "logentry";
		private static readonly ILog Log = LogManager.GetLogger(typeof(HgXmlLogParser));

		public ICollection<Changeset> GetChangesets(TextReader history)
		{
			XDocument historyDocument;

			try
			{
				historyDocument = XDocument.Load(history, LoadOptions.SetLineInfo);
				Log.Debug("Loaded hg history XML output");
			}
			catch (XmlException xmlException)
			{
				throw new HgSourceControlException("History parser expects XML-style output.", xmlException);
			}

			return historyDocument
				.Descendants(ChangesetName)
				.Tap(
				     logEntries =>
				     Log.DebugFormat(CultureInfo.CurrentCulture, "Found {0} 'logEntry' nodes.", logEntries.Count()))
				.Where(logEntry => logEntry.Attribute(Changeset.RevisionAttributeName).Value != "-1")
				.Select(Changeset.FromLogEntryElement)
				.ToList();
		}
	}
}
