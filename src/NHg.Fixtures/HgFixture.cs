using System;
using System.IO;
using System.Linq;

using Moq;

using NUnit.Framework;

namespace NHg.Fixtures
{
	[TestFixture]
	[Category("Unit")]
	public class HgFixture
	{
		[Test]
		[Category("Fast")]
		public void DetectsRepositoryTheCheapWay()
		{
			using (var repoDirectory = new SelfCleaningDirectory())
			{
				var hg = new Hg(new HgProcess(), new HgXmlLogParser(), repoDirectory.Path);
				Assert.That(hg.IsRepository, Is.False);
				Directory.CreateDirectory(Path.Combine(repoDirectory.Path, ".hg"));
				Assert.That(hg.IsRepository);
			}
		}

		[Test]
		[Category("Fast")]
		public void VersionFindsAnyVersionyLookingThingInFirstLine()
		{
			var processMock = new Mock<IHgProcess>();
			processMock
				.Setup(p => p.Execute(It.IsAny<HgArguments>()))
				.Returns(new HgResult(false,
				                      "I have no idea what you're talking about, but version 51.50.\r\nMaybe you mean version 19.84?",
				                      ""))
				.AtMostOnce()
				.Verifiable();
			using (var repoDirectory = new SelfCleaningDirectory())
			{
				var hg = new Hg(processMock.Object, new HgXmlLogParser(), repoDirectory.Path);
				var actual = hg.Version;
				Assert.That(actual, Is.EqualTo(new Version(51, 50)));
			}
		}

		[Test]
		[Category("Fast")]
		public void VersionNaughtDotNaughtReturnedWhenProcessFails()
		{
			var processMock = new Mock<IHgProcess>();
			processMock
				.Setup(p => p.Execute(It.IsAny<HgArguments>()))
				.Returns(new HgResult(true, "", ""))
				.AtMostOnce()
				.Verifiable();
			using (var repoDirectory = new SelfCleaningDirectory())
			{
				var hg = new Hg(processMock.Object, new HgXmlLogParser(), repoDirectory.Path);
				var actual = hg.Version;
				Assert.That(actual, Is.EqualTo(new Version(0, 0)));
			}
		}

		[Test]
		[Category("Fast")]
		public void VersionNaughtDotNaughtReturnedWhenVersionTextNotFound()
		{
			var processMock = new Mock<IHgProcess>();
			processMock
				.Setup(p => p.Execute(It.IsAny<HgArguments>()))
				.Returns(new HgResult(false, "I have no idea what you're talking about.", ""))
				.AtMostOnce()
				.Verifiable();
			using (var repoDirectory = new SelfCleaningDirectory())
			{
				var hg = new Hg(processMock.Object, new HgXmlLogParser(), repoDirectory.Path);
				var actual = hg.Version;
				Assert.That(actual, Is.EqualTo(new Version(0, 0)));
			}
		}
	}
}
