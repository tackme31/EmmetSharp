using EmmetSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EmmetSharp.Parser
{
    public class AbbreviationParser
    {
        private static readonly Regex MultiplicationRegex = new Regex(@"\*(?<multiplier>[1-9]\d*)$", RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly Regex NumberingRegex = new Regex(@"(?<numbering>\$+)(@(?<direction>-)?(?<base>[1-9]\d*)?)?", RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly Regex HtmlTagRegex = new Regex(
            @"^" +
            @"(?<tag>[^.#{}\[\]\s]+?)?" +
            @"(#(?<id>[^.#{}\[\]\s]+?))?" +
            @"(\.(?<class>[^.#{}\[\]\s]+?)){0,}" +
            @"(\[((?<attr>[^=.#{}\[\]\s]+(=""[^""]*"")?)\s?){0,}\])?" +
            @"({(?<text>[^{}]+)})?" +
            @"$",
            RegexOptions.Compiled | RegexOptions.Singleline);

        public static List<HtmlTag> Parse(string abbreviation)
        {
            abbreviation = ConvertClimbUpToSibling(abbreviation);
            var abbreviations = SplitAbbreviationAt(abbreviation, '>');
            return ParseInner(abbreviations);
        }

        private static List<HtmlTag> ParseInner(List<string> abbreviations)
        {
            if (abbreviations.Count < 1)
            {
                return new List<HtmlTag>();
            }

            var rootAbbreviation = abbreviations[0];
            if (string.IsNullOrWhiteSpace(rootAbbreviation))
            {
                throw new FormatException($"An empty tag is contained in the abbreviation.");
            }

            // When the abbreviation has no children, and its root has no siblings, no multiplications.
            var rootSiblings = SplitAbbreviationAt(rootAbbreviation, '+');
            if (abbreviations.Count == 1 &&
                rootSiblings.Count == 1 &&
                !MultiplicationRegex.IsMatch(rootAbbreviation))
            {
                return new List<HtmlTag>()
                {
                    CreateHtmlTag(rootSiblings[0])
                };
            }

            // Parse the root of the abbreviation.
            var result = new List<HtmlTag>();
            var lastMultiplir = 1;
            foreach (var sibling in rootSiblings)
            {
                if (string.IsNullOrWhiteSpace(sibling))
                {
                    throw new FormatException($"An empty tag is contained in the abbreviation.");
                }

                // Get multiplication data
                var siblingBody = sibling;
                var multiplier = 1;
                var multiplicationMatch = MultiplicationRegex.Match(sibling);
                if (multiplicationMatch.Success)
                {
                    siblingBody = MultiplicationRegex.Replace(sibling, string.Empty);
                    multiplier = int.Parse(multiplicationMatch.Groups["multiplier"].Value);
                }

                // Multiply nodes
                for (var i = 1; i <= multiplier; i++)
                {
                    var numberedBody = ReplaceNumberings(siblingBody, i, multiplier);
                    var siblingAbbreviations = SplitAbbreviationAt(numberedBody, '>');
                    // Parse the siblings of the root recursively
                    var tags = ParseInner(siblingAbbreviations);
                    result.AddRange(tags);
                }

                lastMultiplir = multiplier;
            }

            // Parse the children of the abbreviation recursively
            var childAbbreviations = abbreviations.GetRange(1, abbreviations.Count - 1);
            if (result.Count > 0 && childAbbreviations.Count > 0)
            {
                var lastTags = result.GetRange(result.Count - lastMultiplir, lastMultiplir);

                // When the last tag is multiplied, set its children to each tag.
                foreach (var lastTag in lastTags)
                {
                    var tags = ParseInner(childAbbreviations);
                    lastTag.Children = tags;
                }
            }

            return result;
        }

        /// <summary>
        /// Convert climb-up syntax to sibling syntax.
        /// </summary>
        /// <param name="abbreviation"></param>
        /// <returns></returns>
        private static string ConvertClimbUpToSibling(string abbreviation)
        {
            if (abbreviation.IndexOf('^') < 0)
            {
                return abbreviation;
            }

            var children = SplitAbbreviationAt(abbreviation, '>');

            // Check the input has climbing-up nodes to the root.
            var climbedUpDepth = -1;
            for (var depth = 0; depth < children.Count; depth++)
            {
                var climbs = SplitAbbreviationAt(children[depth], '^');
                if (depth == 0 && climbs.Count == 1)
                {
                    continue;
                }

                if (depth <= climbs.Count - 1)
                {
                    climbedUpDepth = depth;
                    break;
                }
            }

            // No climbed-up nodes
            if (climbedUpDepth < 0)
            {
                var first = children[0];
                var match = MultiplicationRegex.Match(first);
                if (match.Success)
                {
                    // Multiply the first node
                    var nonMultipliedPart = MultiplicationRegex.Replace(first, string.Empty);
                    var multiplied = ConvertClimbUpToSibling(nonMultipliedPart);
                    first = $"({multiplied}){match.Value}";
                }

                if (children.Count == 1)
                {
                    return first;
                }

                // Apply children recursively
                var rest = ConvertClimbUpToSibling(string.Join(">", children.Skip(1)));
                return $"{first}>{rest}";
            }

            // Split climbedUp into non-climbing part and climbing part.
            var climbedUp = SplitAbbreviationAt(children[climbedUpDepth], '^');
            var firstInClimedUp = climbedUp.TakeWhile((_, hutCount) => hutCount < climbedUpDepth);
            var restInClimedUp = climbedUp.SkipWhile((_, hutCount) => hutCount < climbedUpDepth);

            // Get non-climbed-up parts in the input (p>b>a^b^^h1^h2>span => p>b>a^b)
            var nonClimbedUpList = children
                .GetRange(0, climbedUpDepth)
                .Append(string.Join("^", firstInClimedUp).TrimEnd('^'))
                .Where(abbr => !string.IsNullOrWhiteSpace(abbr))
                .ToList();

            var siblingsOfRoot = new List<List<string>>();
            if (nonClimbedUpList.Any())
            {
                siblingsOfRoot.Add(nonClimbedUpList);
            }

            // Add all chimbed-up part to sibling of the root
            var climedUpList = restInClimedUp
                .Where(abbr => !string.IsNullOrWhiteSpace(abbr))
                .Select(abbr => new List<string>() { abbr });
            siblingsOfRoot.AddRange(climedUpList);

            // Append the rest children to the last sibling.
            var lastSibling = siblingsOfRoot.LastOrDefault();
            if (lastSibling != default)
            {
                var restChildren = children.GetRange(climbedUpDepth + 1, children.Count - climbedUpDepth - 1);
                lastSibling.AddRange(restChildren);
            }

            return string.Join("+", siblingsOfRoot
                .Select(sibling => string.Join(">", sibling))
                .Select(ConvertClimbUpToSibling) // Apply recursively
                .Select(sibling => $"({sibling})"));
        }

        /// <summary>
        /// Replace $ to numbers in multiplication syntax.
        /// </summary>
        /// <param name="abbreviation"></param>
        /// <param name="index"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        private static string ReplaceNumberings(string abbreviation, int index, int multiplier)
        {
            var numberingMatches = NumberingRegex
                .Matches(abbreviation)
                .OfType<Match>()
                .OrderByDescending(m => m.Value.Length);
            foreach (var numberingMatch in numberingMatches)
            {
                var numbering = numberingMatch.Groups["numbering"].Value;
                var direction = numberingMatch.Groups["direction"].Value;
                if (!int.TryParse(numberingMatch.Groups["base"].Value, out var @base))
                {
                    @base = 1;
                }

                var n = string.IsNullOrEmpty(direction)
                    ? index + @base - 1
                    : multiplier + @base - index;

                var numbers = n.ToString().PadLeft(numbering.Length, '0');
                abbreviation = abbreviation.Replace(numberingMatch.Value, numbers);
            }

            return abbreviation;
        }

        /// <summary>
        /// Create a HTML tag from a node abbreviation
        /// </summary>
        /// <param name="abbreviation"></param>
        /// <returns></returns>
        private static HtmlTag CreateHtmlTag(string abbreviation)
        {
            var tagMatch = HtmlTagRegex.Match(abbreviation);
            if (!tagMatch.Success)
            {
                throw new FormatException($"Invalid format of the tag abbreviation (Value: {abbreviation})");
            }

            var htmlTag = new HtmlTag
            {
                TagName = tagMatch.Groups["tag"].Value,
                Id = tagMatch.Groups["id"].Value,
                ClassList = GetCaptureValues(tagMatch, "class"),
                Attributes = GetCaptureValues(tagMatch, "attr").Select(ParseAttribute).ToDictionary(attr => attr.name, attr => attr.value),
                Text = tagMatch.Groups["text"].Value,
            };

            if (!htmlTag.IsTextNode && string.IsNullOrWhiteSpace(htmlTag.TagName))
            {
                throw new FormatException($"Tag name is missing (Value: {abbreviation})");
            }

            return htmlTag;

            ICollection<string> GetCaptureValues(Match m, string groupName)
            {
                return m.Groups[groupName].Captures
                    .OfType<Capture>()
                    .Select(capture => capture.Value)
                    .ToList();
            }

            (string name, string value) ParseAttribute(string raw)
            {
                var splitted = raw.Split('=');
                return splitted.Length <= 1
                    ? (splitted[0], null)
                    : (splitted[0], splitted[1].Trim('"'));
            }
        }

        /// <summary>
        /// Split abbreviation at a toplevel delimiter.
        /// </summary>
        /// <param name="abbreviation"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        private static List<string> SplitAbbreviationAt(string abbreviation, char delimiter)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            var nest = 0;
            var inText = false;
            var inAttr = false;
            var trimmedAbbr = TrimParentheses(abbreviation);
            for (var i = 0; i < trimmedAbbr.Length; i++)
            {
                // Update status
                switch (trimmedAbbr[i])
                {
                    case '{' when !inText:
                        inText = true;
                        break;
                    case '}' when inText:
                        inText = false;
                        break;
                    case '[' when !inAttr:
                        inAttr = true;
                        break;
                    case ']' when inAttr:
                        inAttr = false;
                        break;
                    case '(' when !inText && !inAttr:
                        nest++;
                        break;
                    case ')' when !inText && !inAttr:
                        nest--;
                        break;
                }

                if (trimmedAbbr[i] != delimiter || inText || inAttr || nest > 0)
                {
                    sb.Append(trimmedAbbr[i]);
                    continue;
                }

                result.Add(sb.ToString());
                sb.Clear();
            }

            if (sb.Length > 0)
            {
                result.Add(sb.ToString());
            }

            if (nest < 0)
            {
                throw new FormatException($"Too much close parenthesis (Value: {abbreviation})");
            }

            if (nest > 0)
            {
                throw new FormatException($"Too much open parenthesis (Value: {abbreviation})");
            }

            return result;
        }

        /// <summary>
        /// Remove outer parentheses.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string TrimParentheses(string value)
        {
            var start = 0;
            var count = value.Length;
            while (value[start] == '(' && value[start + count - 1] == ')')
            {
                if (HasNonNestedPart(value))
                {
                    break;
                }

                start += 1;
                count -= 2;
            }

            return value.Substring(start, count);

            bool HasNonNestedPart(string v)
            {
                var nest = 0;
                for (var i = start; i < count; i++)
                {
                    switch (v[i])
                    {
                        case '(':
                            nest++;
                            continue;
                        case ')':
                            nest--;
                            continue;
                    }

                    if (nest == 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
