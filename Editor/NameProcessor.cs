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
        public static string GetMatching(IList<string> allPaths, string targetName)
        {
            if (allPaths.Count == 0)
                return null;

            return allPaths
                .Select(s => (s, GetScore(Path.GetFileNameWithoutExtension(s), targetName)))
                .OrderByDescending(t => t.Item2)
                .First()
                .Item1;
        }
        
        
        public static Type GetMatching(IList<Type> allPaths, string targetName)
        {
            if (allPaths.Count == 0)
                return null;

            return allPaths
                .Select(type => (type, GetScore(type.Name, targetName)))
                .OrderByDescending(t => t.Item2)
                .First()
                .Item1;
        }

        public static TO GetMatching<TO>(IList<TO> all, string targetName) where TO : Object
        {
            if (all.Count == 0)
                return null;

            return all
                .Select(s => (s, GetScore(s.name, targetName)))
                .OrderByDescending(t => t.Item2)
                .First()
                .Item1;
        }

        public static int GetScore(string name, string target)
        {
            int score = 0;

            name = name.Trim(' ', '_', '.').Replace('.', ' ');
            target = target.Trim(' ', '_', '.').Replace('.', ' ');

            string[] nameParts = SplitPascalCase(name).Split(' ');
            string[] targetParts = SplitPascalCase(target).Split(' ');

            foreach (string namePart in nameParts)
            {
                if (Contains(target, namePart, CompareOptions.Ordinal))
                    score += 12;
                if (Contains(target, namePart, CompareOptions.OrdinalIgnoreCase))
                    score += 10;
            }

            foreach (string targetPart in targetParts)
            {
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
            switch (input)
            {
                case "":
                case null:
                    return input;
                default:
                    var stringBuilder = new StringBuilder(input.Length);
                    
                    if (char.IsLetter(input[0]))
                    {
                        stringBuilder.Append(char.ToUpper(input[0]));
                    }
                    else
                    {
                        stringBuilder.Append(input[0]);
                    }
                    
                    for (int index = 1; index < input.Length; ++index)
                    {
                        char c = input[index];
                        if (char.IsUpper(c) && !char.IsUpper(input[index - 1]))
                            stringBuilder.Append(' ');

                        if (!char.IsLetter(c) && char.IsLetter(input[index - 1]))
                            stringBuilder.Append(' ');
                        
                        if (char.IsLetter(c) && !char.IsLetter(input[index - 1]))
                            stringBuilder.Append(' ');
                        
                        stringBuilder.Append(c);
                    }
                    return stringBuilder.ToString();
            }
        }
    }
}