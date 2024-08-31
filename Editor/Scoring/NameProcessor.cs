using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Search;
using Object = UnityEngine.Object;

namespace AutoAssigner.Scoring
{
    public class NameProcessor
    {
        private const int MaxDistance = 5;

        public static (string path, long score) GetMatching(IList<string> allPaths, PropertyIdentifiers targetName)
        {
            if (allPaths.Count == 0)
                return (null, 0);

            return allPaths
                .Select(s => (s, GetScore(Path.GetFileNameWithoutExtension(s), targetName)))
                .OrderByDescending(t => t.Item2)
                .First();
        }

        public static (Type type, long score) GetMatching(IList<Type> allPaths, PropertyIdentifiers targetName)
        {
            if (allPaths.Count == 0)
                return (null, 0);

            return allPaths
                .Select(type => (type, GetScore(type.Name, targetName)))
                .OrderByDescending(t => t.Item2)
                .First();
        }

        public static TO GetMatching<TO>(IList<TO> all, PropertyIdentifiers targetName)
            where TO : Object
        {
            if (all.Count == 0)
                return null;

            var valueTuples = all
                .Select(s => (s, GetScore(s.name, targetName)))
                .OrderByDescending(t => t.Item2);

            return valueTuples.First().s;
        }

        public static void CutLowQualityPaths(List<string> paths, PropertyIdentifiers targetName)
        {
            paths.Sort((p1, p2) => GetScore(Path.GetFileNameWithoutExtension(p1), targetName)
                .CompareTo(GetScore(Path.GetFileNameWithoutExtension(p2), targetName)));
            var top = GetScore(Path.GetFileNameWithoutExtension(paths[^1]), targetName);
            paths.RemoveAll(o => top - GetScore(Path.GetFileNameWithoutExtension(o), targetName) > MaxDistance);
        }

        public static long GetScore(string name, PropertyIdentifiers target)
        {
            long propertyNameScore = 0;
            FuzzySearch.FuzzyMatch(name, target.PropertyName, ref propertyNameScore);

            long objTypeScore = 0;
            FuzzySearch.FuzzyMatch(name, target.ObjectType, ref objTypeScore);

            long objNameScore = 0;
            FuzzySearch.FuzzyMatch(name, target.ObjectName, ref objNameScore);

            return propertyNameScore * 3 + (long)(objTypeScore * 0.5) + (long)(objNameScore * 0.8);
        }
    }
}