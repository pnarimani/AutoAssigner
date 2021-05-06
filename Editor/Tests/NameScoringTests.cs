using NUnit.Framework;

namespace AutoAssigner.Tests
{
    internal class NameScoringTests
    {
        [TestCase("_comboActivatedSFX", "ComboStartSFX", "BackButtonSFX")]
        [TestCase("_comboSFX ", "CombosSFX", "SFX")]
        [TestCase("prefPrefab6", "Prefab 6", "Prefab 1")]
        [TestCase("prefPrefab6", "Prefab (6)", "Prefab 1")]
        [TestCase("prefPrefab6", "pr", "ar")]
        [TestCase("_shootParent", "ShootStick", "Image (1)")]
        public void Simple(string name, string winner, string loser)
        {
            int winnerScore = NameProcessor.GetScore(name, winner);
            int loserScore = NameProcessor.GetScore(name, loser);
            Assert.Greater(winnerScore, loserScore);
        }

        [TestCase("_hello6field89", "_ hello 6 field 89")]
        [TestCase("simpleName", "Simple Name")]
        [TestCase("Name", "Name")]
        [TestCase("888", "888")]
        [TestCase("888hell", "888 hell")]
        [TestCase("Image (1)", "Image (1)")]
        public void SplitPascalCase(string name, string expected)
        {
            Assert.AreEqual(expected, NameProcessor.SplitPascalCase(name));
        }
    }
}