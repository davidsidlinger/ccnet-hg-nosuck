using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace NHg.Fixtures
{
	[TestFixture]
	[Category("Unit")]
	public class HgHistoryParserFixture
	{
		#region Setup/Teardown

		[SetUp]
		public void SetUp()
		{
			var myAssembly = Assembly.GetAssembly(GetType());
			using (var stream = myAssembly.GetManifestResourceStream("NHg.Fixtures.HgLog.xml"))
			{
				Assert.That(stream != null);
				var reader = new StreamReader(stream);
				_xmlLogContent = reader.ReadToEnd();
			}
			_changesets = _parser.GetChangesets(new StringReader(_xmlLogContent));
		}

		#endregion

		private readonly HgXmlLogParser _parser;
		private string _xmlLogContent;
		private ICollection<Changeset> _changesets;

		private const string NonXmlLogOutput =
			@"changeset:   2:d38b4a5270b3
tag:         tip
user:        David Sidlinger <david.sidlinger@gmail.com>
date:        Fri Apr 09 09:59:34 2010 -0500
summary:     Empty projects and references in place.

changeset:   1:0d607b5d0ad7
user:        David Sidlinger <david.sidlinger@gmail.com>
date:        Fri Apr 09 09:50:29 2010 -0500
summary:     Libraries and blank solution

changeset:   0:93eab8fd8830
user:        David Sidlinger <david.sidlinger@gmail.com>
date:        Fri Apr 09 09:50:08 2010 -0500
summary:     Ignore file

";

		public HgHistoryParserFixture()
		{
			_parser = new HgXmlLogParser();
		}

		[Test]
		[Category("Fast")]
		public void DoesNotExplodeWhenGivenXmlNotFromHg()
		{
			var actual = _parser.GetChangesets(new StringReader("<root/>"));
			Assert.That(actual.Count, Is.EqualTo(0));
		}

		[Test]
		[Category("Fast")]
		public void FindsAuthors()
		{
			var authors = _changesets
				.Select(changeset => new
				                        {
				                        	changeset.Author.Email,
				                        	changeset.Author.Name
				                        })
				.Distinct();
			Assert.That((object)authors.Count(), Is.EqualTo(3));
			var emailUserPattern = new Regex(@"^(?<user>[^@]+).*");
			foreach (var author in authors)
			{
				var userMatch = emailUserPattern.Match(author.Email);
				Assert.That(userMatch.Success);
				var userGroup = userMatch.Groups["user"];
				Assert.That(userGroup.Success);
				Assert.That((object)userGroup.Value, Is.EqualTo(author.Name));
			}
		}

		[Test]
		[Category("Fast")]
		public void ModificationTypes()
		{
			var changesetPaths = _changesets.SelectMany(c => c.Paths);
			var adds = changesetPaths.Where(changesetPath => changesetPath.Action == ChangeAction.Add);
			var modifies = changesetPaths.Where(changesetPath => changesetPath.Action == ChangeAction.Modify);
			var remove = changesetPaths.Where(changesetPath => changesetPath.Action == ChangeAction.Remove);
			var unknowns = changesetPaths.Where(changesetPath => changesetPath.Action == ChangeAction.Unknown);

			Assert.That((object)adds.Count(), Is.EqualTo(7));
			Assert.That((object)modifies.Count(), Is.EqualTo(2));
			Assert.That((object)remove.Count(), Is.EqualTo(1));
			Assert.That((object)unknowns.Count(), Is.EqualTo(1));
		}

		[Test]
		[Category("Fast")]
		public void ParsesPathsCorrectly()
		{
			var expectedPaths = new[]
			                    {
			                    	new FolderFilePair("", ".hgignore"),
			                    	new FolderFilePair("lib", "one.two.three"),
			                    	new FolderFilePair(@"one\two\three", "some.file"),
			                    	new FolderFilePair("", "File With Spaces And No Extension"),
			                    	new FolderFilePair("", "File With Spaces.AndExtension"),
			                    	new FolderFilePair("Every Thing", "Has Spaces.file"),
			                    	new FolderFilePair("", "somethingcrazy"),
			                    	new FolderFilePair("", ".dummy"),
			                    };
			var actualPaths = _changesets.SelectMany(c => c.Paths).Select(path => new FolderFilePair(path.DirectoryName, path.FileName));

			var missingExpecteds = from expectedPath in expectedPaths
			                       join actualPath in actualPaths on expectedPath equals actualPath into
			                       	modificationPathGroup
			                       from rightActualPath in modificationPathGroup.DefaultIfEmpty()
			                       where rightActualPath == null
			                       select expectedPath;

			if (missingExpecteds.Count() > 0)
			{
				Assert.Fail(
				            "Paths not found in modifications:\n{0}",
				            missingExpecteds
				            	.Aggregate(new StringBuilder(),
				            	           (agg, s) => agg.AppendFormat("Folder: {0} File: {1}\n", s.Folder, s.File)));
			}

			var missingActuals = from actualPath in actualPaths
			                     join expectedPath in expectedPaths on actualPath equals expectedPath into
			                     	modificationPathGroup
			                     from rightExpectedPath in modificationPathGroup.DefaultIfEmpty()
			                     where rightExpectedPath == null
			                     select actualPath;

			if (missingActuals.Count() > 0)
			{
				Assert.Fail(
				            "Paths not found in expected list:\n{0}",
				            missingActuals
				            	.Aggregate(new StringBuilder(),
				            	           (agg, s) => agg.AppendFormat("Folder: {0} File: {1}\n", s.Folder, s.File)));
			}
		}

		[Test]
		[Category("Fast")]
		public void RejectsNonXmlOutput()
		{
			Assert.That(
			            (ActualValueDelegate)(() =>
			                                  _parser.GetChangesets(new StringReader(NonXmlLogOutput))),
			            Throws.TypeOf(typeof(HgSourceControlException)));
		}

		[Test]
		[Category("Fast")]
		public void YieldsCorrectNumberOfModifications()
		{
			Assert.That(_changesets.SelectMany(c => c.Paths).Count(), Is.EqualTo(11));
		}
	}
}
