using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Object = UnityEngine.Object;

namespace AutoAssigner
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

            string[] nameParts = SplitPascalCase(name).Split(' ');
            string[] targetParts = SplitPascalCase(target).Split(' ');

            foreach (string namePart in nameParts)
            {
                if (string.IsNullOrWhiteSpace(namePart))
                    continue;

                if (Contains(target, namePart, CompareOptions.Ordinal))
                    score += 12;
                if (Contains(target, namePart, CompareOptions.OrdinalIgnoreCase))
                    score += 10;

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

                if (Contains(name, targetPart, CompareOptions.Ordinal))
                    score += 12;
                else if (Contains(name, targetPart, CompareOptions.OrdinalIgnoreCase))
                    score += 10;
            }

            return score;
        }

        public static bool Contains(string main, string sub, CompareOptions options)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(main, sub, options) >= 0;
        }

        public static string SplitPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            input = input.Replace(" ", "");

            if (string.IsNullOrEmpty(input))
                return input;

            var stringBuilder = new StringBuilder(input.Length);

            stringBuilder.Append(char.IsLetter(input[0]) ? char.ToUpper(input[0]) : input[0]);

            for (int i = 1; i < input.Length; ++i)
            {
                char next = char.MinValue;
                char prev = input[i - 1];
                char curr = input[i];

                if (i < input.Length - 1)
                    next = input[i + 1];

                if (char.IsUpper(curr) && (!char.IsUpper(prev) || !char.IsUpper(next)))
                    stringBuilder.Append(' ');
                else if (!char.IsLetter(curr) && char.IsLetter(prev))
                    stringBuilder.Append(' ');
                else if (char.IsLetter(curr) && !char.IsLetter(prev))
                    stringBuilder.Append(' ');
                
                stringBuilder.Append(curr);
            }

            return stringBuilder.ToString();
        }

        public static StringBuilder Reverse(StringBuilder sb)
        {
            int end = sb.Length - 1;
            int start = 0;

            while (end - start > 0)
            {
                char t = sb[end];
                sb[end] = sb[start];
                sb[start] = t;
                start++;
                end--;
            }

            return sb;
        }
    }
}