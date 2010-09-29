using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

namespace NHg.Fixtures
{
	[TestFixture]
	[Category("Integration")]
	[Category("Slow")]
	[Ignore]
	public class HgIntegrationFixture
	{
		private static readonly HgXmlLogParser _defaultXmlLogParser = new HgXmlLogParser();
		private static readonly IHgProcess DefaultProcessExecutor = new HgProcess();

		private static Hg CreateProvider(SelfCleaningDirectory repoDirectory)
		{
			return new Hg(DefaultProcessExecutor, _defaultXmlLogParser, repoDirectory.Path);
		}

		private static void AssertResultNotFailed(IHgResult processResult)
		{
			Assert.That(processResult.Failed, Is.False, processResult.StandardError);
		}

		private static void ChangeToBranch(SelfCleaningDirectory repoDirectory, Branch branch)
		{
			var hgArguments = HgArguments.ChangeBranch(branch).AddRepository(repoDirectory.Path);
			AssertResultNotFailed(DefaultProcessExecutor.Execute(hgArguments));
		}

		private static void TagCurrent(SelfCleaningDirectory repoDirectory, Tag tag)
		{
			var hgArguments = HgArguments
				.Tag(tag.ToString())
				.AddRepository(repoDirectory.Path);
			AssertResultNotFailed(DefaultProcessExecutor.Execute(hgArguments));
		}

		private static void AddRandomCommit(SelfCleaningDirectory selfCleaningDirectory)
		{
			WriteRandomFile(selfCleaningDirectory);
			AddRemoveFiles(selfCleaningDirectory);
			CommitChanges(selfCleaningDirectory);
		}

		private static void CommitChanges(SelfCleaningDirectory selfCleaningDirectory)
		{
			var hgArguments = HgArguments
				.Commit(DateTimeOffset.Now.Ticks.ToString())
				.AddRepository(selfCleaningDirectory.Path);
			AssertResultNotFailed(DefaultProcessExecutor.Execute(hgArguments));
		}

		private static void AddRemoveFiles(SelfCleaningDirectory selfCleaningDirectory)
		{
			var hgArguments = HgArguments
				.AddRemove()
				.Similarity(50)
				.AddRepository(selfCleaningDirectory.Path);
			AssertResultNotFailed(DefaultProcessExecutor.Execute(hgArguments));
		}

		private static void InitializeRepository(SelfCleaningDirectory repoDirectory)
		{
			var hgArguments = HgArguments.Init(repoDirectory.Path);
			AssertResultNotFailed(DefaultProcessExecutor.Execute(hgArguments));
		}

		private static string WriteRandomFile(SelfCleaningDirectory selfCleaningDirectory)
		{
			var path = Path.Combine(selfCleaningDirectory.Path, Path.GetRandomFileName());
			MangleFile(path);
			return path;
		}

		private static void MangleFile(string path)
		{
			using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
			{
				var bytes = new byte[32384];
				new Random().NextBytes(bytes);
				stream.Write(bytes, 0, bytes.Length);
			}
		}

		[Test]
		public void CheckVersion()
		{
			using (var repoDirectory = new SelfCleaningDirectory())
			{
				var provider = CreateProvider(repoDirectory);
				var version = provider.Version;
				Assert.That(version, Is.GreaterThan(new Version(0, 0)));
			}
		}

		[Test]
		public void CurrentBranchFunctionsAsDesigned()
		{
			var expected = new Branch("differenty");
			using (var repoDirectory = new SelfCleaningDirectory())
			{
				InitializeRepository(repoDirectory);
				Enumerable.Range(1, 5).ToList().ForEach(i => AddRandomCommit(repoDirectory));
				ChangeToBranch(repoDirectory, expected);
				Enumerable.Range(1, 5).ToList().ForEach(i => AddRandomCommit(repoDirectory));
				var provider = new Hg(DefaultProcessExecutor, _defaultXmlLogParser, repoDirectory.Path);
				provider.Update(Branch.Default.AsTag().AsRevision());
				Assert.That(provider.CurrentBranch, Is.EqualTo(Branch.Default));
				provider.Update(expected.AsTag().AsRevision());
				Assert.That(provider.CurrentBranch, Is.EqualTo(expected));
			}
		}

		[Test]
		public void GetLog()
		{
			using (var repoDirectory = new SelfCleaningDirectory())
			{
				var provider = CreateProvider(repoDirectory);
				InitializeRepository(repoDirectory);
				Enumerable.Range(1, 5).ToList().ForEach(i => AddRandomCommit(repoDirectory));
				var actual = provider.Log(Branch.Default, Revision.First.Through(Branch.Default.AsTag().AsRevision()));
				Assert.That(actual.Count, Is.EqualTo(5));
			}
		}

		[Test]
		public void GetLogWithSameRevision()
		{
			using (var repoDirectory = new SelfCleaningDirectory())
			{
				InitializeRepository(repoDirectory);
				AddRandomCommit(repoDirectory);
				AddRandomCommit(repoDirectory);
				var provider = CreateProvider(repoDirectory);
				var actual = provider.Log(Branch.Default, Revision.First.Through(Revision.First));
				Assert.That(actual.Count, Is.EqualTo(1));
			}
		}

		[Test]
		public void GetsCorrectRevisionForTag()
		{
			using (var repoDirectory = new SelfCleaningDirectory())
			{
				var provider = CreateProvider(repoDirectory);
				InitializeRepository(repoDirectory);
				AddRandomCommit(repoDirectory);
				var expected = provider.CurrentRevision;
				var tag = new Tag("Whoop whoop");
				TagCurrent(repoDirectory, tag);
				AddRandomCommit(repoDirectory);
				Assert.That(provider.GetRevisionForTag(tag), Is.EqualTo(expected));
			}
		}

		[Test]
		public void GetsCorrectRevisionWhenNoRevisions()
		{
			using (var repoDirectory = new SelfCleaningDirectory())
			{
				var provider = CreateProvider(repoDirectory);
				InitializeRepository(repoDirectory);
				Assert.That(provider.CurrentRevision, Is.EqualTo(Revision.BeforeFirst));
			}
		}

		[Test]
		public void IgnoresFlagsOnRevisions()
		{
			using (var repoDirectory = new SelfCleaningDirectory())
			{
				InitializeRepository(repoDirectory);
				var randomFile = WriteRandomFile(repoDirectory);
				AddRemoveFiles(repoDirectory);
				var provider = CreateProvider(repoDirectory);
				Assert.That(provider.CurrentRevision, Is.EqualTo(Revision.BeforeFirst));
				CommitChanges(repoDirectory);
				var expectedRevision = provider.CurrentRevision;
				MangleFile(randomFile);
				Assert.That(provider.CurrentRevision, Is.EqualTo(expectedRevision));
			}
		}

		[Test]
		public void IsRepository()
		{
			using (var repoDirectory = new SelfCleaningDirectory())
			{
				var provider = CreateProvider(repoDirectory);
				Assert.That(provider.IsRepository, Is.False);
				InitializeRepository(repoDirectory);
				Assert.That(provider.IsRepository);
			}
		}

		[Test]
		public void LogWithNoRevisionsIsOk()
		{
			using (var repoDirectory = new SelfCleaningDirectory())
			{
				var provider = CreateProvider(repoDirectory);
				InitializeRepository(repoDirectory);
				var actual = provider.Log(Branch.Default, Revision.BeforeFirst.Through(Revision.BeforeFirst));
				Assert.That(actual.Count, Is.EqualTo(0));
			}
		}
	}
}
