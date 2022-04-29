using NUnit.Framework;

namespace GGLabo.Games.Tests.Runtime
{
    public class SampleTest
    {
        [Test]
        public void SampleAssertTest()
        {
            Assert.That(true, Is.EqualTo(true));
        }
    }
}