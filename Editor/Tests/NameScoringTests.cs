using AutoAssigner.Scoring;
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

        
    }
}