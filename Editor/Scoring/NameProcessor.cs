using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Object = UnityEngine.Object;

namespace AutoAssigner.Scoring
{
    public class NameProcessor
    {
        private const int MaxDistance = 5;

        public static (string path, int score) GetMatching(IList<string> allPaths, string targetName)
        {
            if (allPaths.Count == 0)
                return (null, 0);

            return allPaths
                .Select(s => (s, GetScore(Path.GetFileNameWithoutExtension(s), targetName)))
                .OrderByDescending(t => t.Item2)
                .First();
        }

        public static (Type type, int score) GetMatching(IList<Type> allPaths, string targetName)
        {
            if (allPaths.Count == 0)
                return (null, 0);

            return allPaths
                .Select(type => (type, GetScore(type.Name, targetName)))
                .OrderByDescending(t => t.Item2)
                .First();
        }

        public static (TO item, int score) GetMatching<TO>(IList<TO> all, string targetName) where TO : Object
        {
            if (all.Count == 0)
                return (null, 0);

            return all
                .Select(s => (s, GetScore(s.name, targetName)))
                .OrderByDescending(t => t.Item2)
                .First();
        }

        public static void CutLowQualityPaths(List<string> paths, string targetName)
        {
            paths.Sort((p1, p2) => GetScore(Path.GetFileNameWithoutExtension(p1), targetName)
                .CompareTo(GetScore(Path.GetFileNameWithoutExtension(p2), targetName)));
            int top = GetScore(Path.GetFileNameWithoutExtension(paths[paths.Count - 1]), targetName);
            paths.RemoveAll(o => top - GetScore(Path.GetFileNameWithoutExtension(o), targetName) > MaxDistance);
        }

        public static int GetScore(string name, string target)
        {
            int score = 0;

            name = name.Trim(' ', '_', '.')
                .Replace('.', ' ')
                .Replace('_', ' ');

            target = target.Trim(' ', '_', '.')
                .Replace('.', ' ')
                .Replace('_', ' ');

            string[] nameParts = name.SplitPascalCase().Split(' ');
            string[] targetParts = target.SplitPascalCase().Split(' ');

            foreach (string namePart in nameParts)
            {
                if (string.IsNullOrWhiteSpace(namePart))
                    continue;

                float multiplier = namePart.Length == 1 ? 0.2f : 1f;

                if (target.Contains(namePart, StringComparison.Ordinal))
                    score += (int)(12 * multiplier);
                if (target.Contains(namePart, StringComparison.OrdinalIgnoreCase))
                    score += (int)(10 * multiplier);

                foreach (string targetPart in targetParts)
                {
                    if (namePart.Equals(targetPart, StringComparison.Ordinal))
                        score += 20;
                    else if (namePart.Equals(targetPart, StringComparison.OrdinalIgnoreCase))
                        score += 18;
                }
            }

            foreach (string targetPart in targetParts)
            {
                if (string.IsNullOrWhiteSpace(targetPart))
                    continue;

                float multiplier = targetPart.Length == 1 ? 0.2f : 1f;

                if (name.Contains(targetPart, StringComparison.Ordinal))
                    score += (int)(12 * multiplier);
                else if (name.Contains(targetPart, StringComparison.OrdinalIgnoreCase))
                    score += (int)(10 * multiplier);
            }

            return score;
        }
    }
}