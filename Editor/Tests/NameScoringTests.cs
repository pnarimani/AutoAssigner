using NUnit.Framework;

namespace AutoAssigner.Tests
{
    internal class NameScoringTests
    {
        [TestCase("_comboActivatedSFX", "ComboStartSFX", "BackButtonSFX")]
        [TestCase("_comboSFX ", "CombosSFX", "SFX")]
        public void NameScoringTestsSimplePasses(string name, string winner, string loser)
        {
            int winnerScore = AutoAssignProcessor<NameScoringTests>.GetScore(name, winner);
            int loserScore = AutoAssignProcessor<NameScoringTests>.GetScore(name, loser);
            Assert.Greater(winnerScore, loserScore);
        }
    }
}