using NUnit.Framework;

namespace NHg.Fixtures
{
	[TestFixture]
	public class HgProcessFixture
	{
		[Test]
		[Category("Integration")]
		public void RunsProcess()
		{
			var process = new HgProcess();
			var actual = process.Execute(HgArguments.Version().NonInteractive());
			Assert.That(actual.StandardOutput, Is.StringContaining("Mercurial"));
		}
	}
}