using AutoAssigner.Scoring;
using NUnit.Framework;

namespace AutoAssigner.Tests
{
    [TestFixture]
    public class PascalCaseTests
    {
        [TestCase("_hello6field89", "_ hello 6 field 89")]
        [TestCase("simpleName", "Simple Name")]
        [TestCase("Name", "Name")]
        [TestCase("888", "888")]
        [TestCase("888hell", "888 hell")]
        [TestCase("Image (1)", "Image (1)")]
        [TestCase("AKShot", "AK Shot")]
        [TestCase("AK 4", "AK 4")]
        public void SplitPascalCase(string name, string expected)
        {
            Assert.AreEqual(expected, name.SplitPascalCase());
        }
    }
}