using System.Collections.Generic;
using AutoAssigner.Scoring;
using NUnit.Framework;

namespace AutoAssigner.Tests
{
    internal class NameScoringTests
    {
        [TestCaseSource(nameof(TestCaseSource))]
        public void Simple(TestCaseData data)
        {
            var winnerScore = NameProcessor.GetScore(data.Winner, data.Name);
            var loserScore = NameProcessor.GetScore(data.Loser, data.Name);
            Assert.Greater(winnerScore, loserScore);
        }

        private static IEnumerable<TestCaseData> TestCaseSource()
        {
            yield return new TestCaseData
            {
                Name = new PropertyIdentifiers
                {
                    PropertyName = "_boosterList",
                    ObjectType = "DiceGameUI",
                    ObjectName = "DiceUI",
                },
                Winner = "BoosterList",
                Loser = "DiceUI",
            };
        }

        public class TestCaseData
        {
            public PropertyIdentifiers Name { get; set; }
            public string Winner { get; set; }
            public string Loser { get; set; }
        }
    }
}