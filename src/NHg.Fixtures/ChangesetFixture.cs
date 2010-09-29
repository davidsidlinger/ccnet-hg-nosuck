using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using NUnit.Framework;

namespace NHg.Fixtures
{
	[TestFixture(Description = "Unit tests for Changeset")]
	public class ChangesetFixture
	{
		private readonly string _xmlLogContent;

		public ChangesetFixture()
		{
			var myAssembly = Assembly.GetAssembly(GetType());
			using (var stream = myAssembly.GetManifestResourceStream("NHg.Fixtures.HgLog.xml"))
			{
				Assert.That(stream != null);
				var reader = new StreamReader(stream);
				_xmlLogContent = reader.ReadToEnd();
			}
		}

		private static string ConfoundElement(string content, string elementName)
		{
			Debug.Assert(elementName != null);
			Debug.Assert(content != null);

			return content.Replace("<" + elementName, "<confounded").Replace("</" + elementName, "</confounded");
		}

		private static string ConfoundAttribute(string content, string attributeName)
		{
			return content.Replace(" " + attributeName + "=\"", " confounded=\"");
		}

		[Test]
		[Category("Fast")]
		public void DoesNotRejectEntriesWithoutDate()
		{
			var content = ConfoundElement(_xmlLogContent, "date");
			var logEntry =
				XDocument.Parse(content).Descendants(HgXmlLogParser.ChangesetName).FirstOrDefault();
			Assert.That((object)logEntry, Is.Not.Null);
			Assert.That((object)logEntry.Element(Changeset.DateElementName), Is.Null);
			var actual = Changeset.FromLogEntryElement(logEntry);
			Assert.That(DateTimeOffset.Now - actual.Date, Is.LessThan(TimeSpan.FromSeconds(5)));
		}

		[Test]
		[Category("Fast")]
		public void DoesNotRejectEntriesWithoutEmail()
		{
			var content = ConfoundAttribute(_xmlLogContent, "email");
			var logEntry =
				XDocument.Parse(content).Descendants(HgXmlLogParser.ChangesetName).FirstOrDefault();
			Assert.That((object)logEntry, Is.Not.Null);
			Assert.That((object)logEntry.Element(Changeset.AuthorElementName).Attribute(Changeset.EmailAttributeName), Is.Null);
			var changeset = Changeset.FromLogEntryElement(logEntry);
			Assert.That(changeset.Author.Name, Is.Not.Empty);
			Assert.That(changeset.Author.Email, Is.EqualTo(""));
		}

		[Test]
		[Category("Fast")]
		public void DoesNotRejectEntriesWithoutRevision()
		{
			var content = ConfoundAttribute(_xmlLogContent, "revision");
			var logEntry =
				XDocument.Parse(content).Descendants(HgXmlLogParser.ChangesetName).FirstOrDefault();
			Assert.That((object)logEntry, Is.Not.Null);
			Assert.That((object)logEntry.Attribute(Changeset.RevisionAttributeName), Is.Null);
			var actual = Changeset.FromLogEntryElement(logEntry);
			Assert.That(actual.RevisionNumber, Is.EqualTo(0));
		}

		[Test]
		[Category("Fast")]
		public void RejectsEntriesWithoutAHash()
		{
			var content = ConfoundAttribute(_xmlLogContent, Changeset.HashAttributeName);
			var logEntry =
				XDocument.Parse(content).Descendants(HgXmlLogParser.ChangesetName).FirstOrDefault();
			Assert.That((object)logEntry, Is.Not.Null);
			Assert.That((object)logEntry.Attribute(Changeset.HashAttributeName), Is.Null);
			Assert.That(
			            () => Changeset.FromLogEntryElement(logEntry),
			            Throws.ArgumentException.With.Property("ParamName").EqualTo("logEntry"));
		}

		[Test]
		[Category("Fast")]
		public void RejectsEntriesWithoutAuthor()
		{
			var content = ConfoundElement(_xmlLogContent, "author");
			var logEntry =
				XDocument.Parse(content).Descendants(HgXmlLogParser.ChangesetName).FirstOrDefault();
			Assert.That((object)logEntry, Is.Not.Null);
			Assert.That((object)logEntry.Element(Changeset.AuthorElementName), Is.Null);
			Assert.That(
			            () => Changeset.FromLogEntryElement(logEntry),
			            Throws.TypeOf(typeof(ArgumentOutOfRangeException)).With.Property("ParamName").EqualTo("name"));
		}

		[Test]
		[Category("Fast")]
		public void RejectsEntriesWithoutMessage()
		{
			var content = ConfoundElement(_xmlLogContent, Changeset.MessageElementName);
			var logEntry =
				XDocument.Parse(content).Descendants(HgXmlLogParser.ChangesetName).FirstOrDefault();
			Assert.That((object)logEntry, Is.Not.Null);
			Assert.That((object)logEntry.Element(Changeset.MessageElementName), Is.Null);
			Assert.That(
			            () => Changeset.FromLogEntryElement(logEntry),
			            Throws.TypeOf(typeof(ArgumentOutOfRangeException)).With.Property("ParamName").EqualTo("message"));
		}

		[Test]
		[Category("Fast")]
		public void TwelveCharacterHashesAreOk()
		{
			var content = Regex.Replace(_xmlLogContent, "[0-9a-f]{40}", match => match.Value.Substring(0, 12));
			var logEntry =
				XDocument.Parse(content).Descendants(HgXmlLogParser.ChangesetName).First();
			Assert.That((object)logEntry, Is.Not.Null);
			Assert.That((object)logEntry.Attribute(Changeset.HashAttributeName).Value.Length, Is.EqualTo(12));
			Assert.That(Changeset.FromLogEntryElement(logEntry).Hash.ToString().Length, Is.EqualTo(12));
		}

		[Test]
		[Category("Fast")]
		public void WantsHashesToBeHgHashes()
		{
			var content = Regex.Replace(_xmlLogContent, "[0-9a-f]{40}", match => match.Value.Substring(0, 39));
			var logEntry =
				XDocument.Parse(content).Descendants(HgXmlLogParser.ChangesetName).FirstOrDefault();
			Assert.That((object)logEntry, Is.Not.Null);
			Assert.That((object)logEntry.Attribute(Changeset.HashAttributeName).Value.Length, Is.EqualTo(39));
			Assert.That(
			            () => Changeset.FromLogEntryElement(logEntry),
			            Throws.TypeOf(typeof(ArgumentOutOfRangeException)).With.Property("ParamName").EqualTo("value"));
		}
	}
}
