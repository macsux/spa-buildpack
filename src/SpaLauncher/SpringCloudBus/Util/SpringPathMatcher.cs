using System.Text.RegularExpressions;

namespace SpaLauncher.SpringCloudBus.Util
{
public class SpringPathMatcher
{
    private const string Seperator = ":";
        /// <summary>
        /// Initializes a new <see cref="SpringPathMatcher"/>.
        /// </summary>
        /// <param name="pattern">Ant-style pattern.</param>
        public SpringPathMatcher()
        {
            
        }

        /// <summary>
        /// Validates whether the input matches the given pattern.
        /// </summary>
        /// <param name="input">Path for which to check if it matches the ant-pattern.</param>
        /// <returns>Whether the input matches the pattern.</returns>
        /// <inheritdoc/>
        public bool IsMatch(string pattern, string input)
        {
            input = input ?? string.Empty;
            return Regex.IsMatch(input,
                EscapeAndReplace(pattern),
                RegexOptions.Singleline
            );
        }

        private  string EscapeAndReplace(string pattern)
        {
            if (pattern == "**")
            {
                pattern = Seperator + pattern;
            }
            if (pattern.EndsWith(Seperator))
            {
                pattern += "**";
            }

            var pattern2 = Regex.Escape(pattern)
                .Replace($@"{Seperator}\*\*/", $@"(.*[\{Seperator}])")
                .Replace(@"\*\*/", "(.*)")
                .Replace($@"{Seperator}\*\*", "(.*)")
                .Replace(@"\*", $@"([^\{Seperator}]*)")
                .Replace(@"\?", "(.)")
                .Replace(@"}", ")")
                .Replace(@"\{", "(")
                .Replace(@",", "|");

            return $"^{pattern2}$";
        }

    }
}