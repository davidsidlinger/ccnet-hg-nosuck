using NUnit.Framework;

namespace NHg.Fixtures
{
	[TestFixture]
	[Category("Unit")]
	public class HashFixture
	{
		private static readonly Hash _longHash = new Hash("e0e50f0f23264cb59ab059fa049741f3e0e50f0f");
		private static readonly Hash _shortHash = new Hash("e0e50f0f2326");

		[Test]
		[Category("Fast")]
		public void EqualsOnlyCaresAboutShortHash()
		{
			Assert.That(_longHash, Is.EqualTo(_shortHash));
			Assert.That(_shortHash, Is.EqualTo(_longHash));
		}

		[Test]
		[Category("Fast")]
		public void IsShortAndIsLong()
		{
			Assert.That(_longHash.IsLong);
			Assert.That(_longHash.IsShort, Is.False);
			Assert.That(_shortHash.IsShort);
			Assert.That(_shortHash.IsLong, Is.False);
		}
	}
}
