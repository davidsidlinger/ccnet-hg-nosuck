using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using NHg;
using NHg.Fixtures;

using NUnit.Framework;

namespace CruiseControl.Mercurial.Fixtures
{
	[TestFixture]
	[Category("Integration")]
	[Category("Slow")]
	[Ignore]
	public class CruiseControlIntegrationFixture
	{
		private static Hg GetProvider(string repositoryPath)
		{
			var cruiseControlHgProcess = new CruiseControlHgProcess(new TestProcessExecutor());
			return new Hg(cruiseControlHgProcess, new HgXmlLogParser(), repositoryPath);
		}

		private static string WriteAndCommitRandomFile(SelfCleaningDirectory repoDirectory)
		{
			var fileContent = Encoding.ASCII.GetString((new byte[8196]).Tap(b => new Random().NextBytes(b)));
			var filePath = Path.Combine(repoDirectory.Path, Path.GetRandomFileName());
			using (var stream = File.CreateText(filePath))
			{
				stream.Write(fileContent);
			}
			var provider = GetProvider(repoDirectory.Path);
			provider.AddRemove();
			provider.Commit(DateTimeOffset.Now.ToString(CultureInfo.InvariantCulture));
			return Path.GetFileName(filePath);
		}

		[Test]
		public void MultipleHeadsFail()
		{
			using (var pullFromDirectory = new SelfCleaningDirectory())
			{
				var pullFromPath = pullFromDirectory.Path;
				var provider = GetProvider(pullFromPath);
				provider.Init();

				WriteAndCommitRandomFile(pullFromDirectory);

				var firstRev = provider.CurrentRevision;

				WriteAndCommitRandomFile(pullFromDirectory);

				provider.Update(firstRev);

				WriteAndCommitRandomFile(pullFromDirectory);

				using (var workingDirectory = new SelfCleaningDirectory())
				{
					var workingPath = workingDirectory.Path;
					var sourceControl = new HgSourceControl(new HgXmlLogParser(),
					                                        new TestProcessExecutor())
					                    {
					                    	SourceRepository = pullFromPath,
					                    	FailIfMultipleHeads = true,
					                    };
					var fromResult = new MockIntegrationResult { WorkingDirectory = workingPath };
					var toResult = new MockIntegrationResult { WorkingDirectory = workingPath };
					Assert.That(
					            () => { sourceControl.GetModifications(fromResult, toResult); },
					            Throws.TypeOf(typeof(HgSourceControlException)).With.Property("Message").EqualTo(
					                                                                                             "Multiple or no heads in branch 'default'."));
				}
			}
		}

		[Test]
		public void MultipleHeadsOkIfSpecified()
		{
			using (var pullFromDirectory = new SelfCleaningDirectory())
			{
				var pullFromPath = pullFromDirectory.Path;
				var provider = GetProvider(pullFromPath);
				provider.Init();

				WriteAndCommitRandomFile(pullFromDirectory);

				var firstRev = provider.CurrentRevision;

				WriteAndCommitRandomFile(pullFromDirectory);

				provider.Update(firstRev);

				WriteAndCommitRandomFile(pullFromDirectory);

				using (var workingDirectory = new SelfCleaningDirectory())
				{
					var workingPath = workingDirectory.Path;
					var sourceControl = new HgSourceControl(new HgXmlLogParser(),
					                                        new TestProcessExecutor())
					                    {
					                    	SourceRepository = pullFromPath,
					                    	FailIfMultipleHeads = false,
					                    };
					var fromResult = new MockIntegrationResult { WorkingDirectory = workingPath };
					var toResult = new MockIntegrationResult { WorkingDirectory = workingPath };
					var modifications = sourceControl.GetModifications(fromResult, toResult);
					Assert.That(modifications.Length, Is.EqualTo(3));
				}
			}
		}

		[Test]
		public void Simple()
		{
			using (var pullFromDirectory = new SelfCleaningDirectory())
			{
				var pullFromPath = pullFromDirectory.Path;
				var provider = GetProvider(pullFromPath);
				provider.Init();

				var fileName = WriteAndCommitRandomFile(pullFromDirectory);

				using (var workingDirectory = new SelfCleaningDirectory())
				{
					var workingPath = workingDirectory.Path;
					var sourceControl = new HgSourceControl(new HgXmlLogParser(),
					                                        new TestProcessExecutor())
					                    {
					                    	SourceRepository = pullFromPath,
					                    };
					var fromResult = new MockIntegrationResult { WorkingDirectory = workingPath };
					var toResult = new MockIntegrationResult { WorkingDirectory = workingPath };
					var modifications = sourceControl.GetModifications(fromResult, toResult);
					Assert.That(modifications.Length, Is.EqualTo(1));
					var modification = modifications.Single();
					Assert.That(modification.ModifiedTime, Is.GreaterThan(DateTime.Now.AddMinutes(-1)));
					Assert.That(modification.FileName, Is.EqualTo(fileName));

					sourceControl.GetSource(toResult);

					var workingProvider = GetProvider(workingPath);
					Assert.That(modification.Version, Is.StringStarting(workingProvider.CurrentRevision.ToString()));
				}
			}
		}

		[Test]
		public void SimulateMoreModifications()
		{
			using (var pullFromDirectory = new SelfCleaningDirectory())
			{
				var pullFromPath = pullFromDirectory.Path;
				var provider = GetProvider(pullFromPath);
				provider.Init();

				var firstFile = WriteAndCommitRandomFile(pullFromDirectory);
				var firstRevision = provider.CurrentRevision;

				using (var workingDirectory = new SelfCleaningDirectory())
				{
					var workingPath = workingDirectory.Path;
					var sourceControl = new HgSourceControl(new HgXmlLogParser(),
					                                        new TestProcessExecutor())
					                    {
					                    	SourceRepository = pullFromPath,
					                    };
					var fromResult = new MockIntegrationResult { WorkingDirectory = workingPath };
					var toResult = new MockIntegrationResult { WorkingDirectory = workingPath };
					var modifications = sourceControl.GetModifications(fromResult, toResult);
					Assert.That(modifications.Length, Is.EqualTo(1));
					Assert.That(modifications.Single().FileName, Is.EqualTo(firstFile));
					Assert.That(modifications.Single().Version, Is.StringStarting(firstRevision.ToString()));

					sourceControl.GetSource(toResult);

					var secondFile = WriteAndCommitRandomFile(pullFromDirectory);
					var secondRevision = provider.CurrentRevision;

					var againToResult = new MockIntegrationResult { WorkingDirectory = workingPath };
					var moreModifications = sourceControl.GetModifications(toResult, againToResult);
					Assert.That(moreModifications.Length, Is.EqualTo(1));
					Assert.That(moreModifications.Single().FileName, Is.EqualTo(secondFile));
					Assert.That(moreModifications.Single().Version, Is.StringStarting(secondRevision.ToString()));
				}
			}
		}

		[Test]
		public void TagSupport()
		{
			using (var pullFromDirectory = new SelfCleaningDirectory())
			{
				const string tagName = "Test tag";

				var pullFromPath = pullFromDirectory.Path;
				var provider = GetProvider(pullFromPath);
				provider.Init();

				WriteAndCommitRandomFile(pullFromDirectory);

				var taggedRevision = provider.CurrentRevision;
				provider.Tag(taggedRevision, tagName, false);

				using (var workingDirectory = new SelfCleaningDirectory())
				{
					var workingPath = workingDirectory.Path;
					var sourceControl = new HgSourceControl(new HgXmlLogParser(),
					                                        new TestProcessExecutor())
					                    {
					                    	SourceRepository = pullFromPath,
					                    	TagRaw = tagName,
					                    };
					var fromResult = new MockIntegrationResult { WorkingDirectory = workingPath };
					var toResult = new MockIntegrationResult { WorkingDirectory = workingPath };
					var modifications = sourceControl.GetModifications(fromResult, toResult);
					Assert.That(modifications.Length, Is.EqualTo(1));
					Assert.That(modifications.Single().Version, Is.StringStarting(taggedRevision.ToString()));

					sourceControl.GetSource(toResult);

					WriteAndCommitRandomFile(pullFromDirectory);

					var moreModifications = sourceControl.GetModifications(toResult,
					                                                       new MockIntegrationResult { WorkingDirectory = workingPath });
					Assert.That(moreModifications.Length, Is.EqualTo(0));

					provider.Tag(provider.CurrentRevision, tagName, true);

					var evenMoreModifications = sourceControl.GetModifications(toResult,
					                                                           new MockIntegrationResult
					                                                           { WorkingDirectory = workingPath });
					Assert.That(evenMoreModifications.Length, Is.EqualTo(2));
				}
			}
		}
	}
}
