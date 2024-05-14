using System.Text;

namespace AutoAssigner.Scoring
{
    internal static class PascalCase
    {
        public static string SplitPascalCase(this string input)
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

                bool isNextLetterLowerCase = char.IsLower(next) && char.IsLetter(next);

                if (char.IsUpper(curr) && (!char.IsUpper(prev) || isNextLetterLowerCase))
                    stringBuilder.Append(' ');
                else if (!char.IsLetter(curr) && char.IsLetter(prev))
                    stringBuilder.Append(' ');
                else if (char.IsLetter(curr) && !char.IsLetter(prev))
                    stringBuilder.Append(' ');

                stringBuilder.Append(curr);
            }

            return stringBuilder.ToString();
        }
    }
}