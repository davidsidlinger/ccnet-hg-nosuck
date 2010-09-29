using NUnit.Framework;

namespace NHg.Fixtures
{
	[TestFixture]
	public class RevisionFixture
	{
		[Test]
		[Category("Fast")]
		public void ShortVersionOfLongHashComparesEqual()
		{
			var longRevision = new Revision("e0e50f0f23264cb59ab059fa049741f3e0e50f0f");
			var shortRevision = new Revision("e0e50f0f2326");
			Assert.That(longRevision, Is.EqualTo(shortRevision));
			Assert.That(shortRevision, Is.EqualTo(longRevision));
		}
	}
}
