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
            @"({(?<text>.+)})?" +
            @"$",
            RegexOptions.Compiled | RegexOptions.Singleline);

        public static List<HtmlTag> Parse(string abbreviation)
        {
            abbreviation = ApplyClimbUp(abbreviation);
            var abbreviations = SplitAbbreviationAt(abbreviation, '>');
            return ParseInner(abbreviations);
        }

        private static List<HtmlTag> ParseInner(List<string> abbreviations)
        {
            if (abbreviations.Count < 1)
            {
                return new List<HtmlTag>();
            }

            var firstAbbreviation = abbreviations[0];
            if (string.IsNullOrWhiteSpace(firstAbbreviation))
            {
                throw new FormatException($"An empty tag is contained in the abbreviation.");
            }

            var firstSiblings = SplitAbbreviationAt(firstAbbreviation, '+');
            if (!MultiplicationRegex.IsMatch(firstAbbreviation) && abbreviations.Count == 1 && firstSiblings.Count == 1)
            {
                return new List<HtmlTag>()
                {
                    CreateHtmlTag(firstSiblings[0])
                };
            }

            var result = new List<HtmlTag>();
            var lastMultiplir = 1;
            foreach (var sibling in firstSiblings)
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
                    var tags = ParseInner(siblingAbbreviations);
                    result.AddRange(tags);
                }

                lastMultiplir = multiplier;
            }

            var restAbbreviations = abbreviations.GetRange(1, abbreviations.Count - 1);
            if (result.Count > 0 && restAbbreviations.Count > 0)
            {
                var tags = ParseInner(restAbbreviations);
                var lastTags = result.GetRange(result.Count - lastMultiplir, lastMultiplir);

                // When the last tag is multiplied, set its children to each tag.
                foreach (var lastTag in lastTags)
                {
                    lastTag.Children = tags;
                }
            }

            return result;
        }

        private static string ApplyClimbUp(string input)
        {
            if (input.IndexOf('^') < 0)
            {
                return input;
            }

            var children = SplitAbbreviationAt(input, '>');

            // Check the input has climbing-up nodes to the root.
            List<string> climbedUp = default;
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
                    climbedUp = climbs;
                    climbedUpDepth = depth;
                    break;
                }
            }

            // No climbed-up nodes
            if (climbedUpDepth < 0)
            {
                if (children.Count == 1)
                {
                    return children[0];
                }

                // Apply children recursively
                var first = children[0];
                var rest = ApplyClimbUp(string.Join(">", children.Skip(1)));
                return $"{first}>{rest}";
            }

            // Parts that do not need to be climbed-up
            var firstInClimedUp = climbedUp.TakeWhile((_, hutCount) => hutCount < climbedUpDepth);
            // And need to be climbed-up.
            var restInClimedUp = climbedUp.SkipWhile((_, hutCount) => hutCount < climbedUpDepth);

            // Get non-climbed-up parts in the input (p>b>a^b^^h1^h2>span => p>b>a^b)
            var nonClimbedUpList = children
                .GetRange(0, climbedUpDepth)
                .Append(string.Join("^", firstInClimedUp).TrimEnd('^'))
                .SkipWhile(abbr => string.IsNullOrWhiteSpace(abbr))
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
                .Select(ApplyClimbUp) // Apply recursively
                .Select(sibling => $"({sibling})"));
        }


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

        private static HtmlTag CreateHtmlTag(string htmlTag)
        {
            var tagMatch = HtmlTagRegex.Match(htmlTag);
            if (!tagMatch.Success)
            {
                throw new FormatException($"Invalid format of the tag abbreviation (Value: {htmlTag})");
            }

            var tag = tagMatch.Groups["tag"].Value;
            var id = tagMatch.Groups["id"].Value;
            var classList = GetCaptureValues(tagMatch, "class");
            var attributes = GetCaptureValues(tagMatch, "attr").Select(ParseAttribute).ToDictionary(attr => attr.name, attr => attr.value);
            var text = tagMatch.Groups["text"].Value;

            // HTML tag
            if (!string.IsNullOrWhiteSpace(tag))
            {
                return new HtmlTag
                {
                    TagName = tag,
                    Id = id,
                    ClassList = classList,
                    Attributes = attributes,
                    Text = text,
                };
            }

            // Only text
            if (!string.IsNullOrWhiteSpace(text) &&
                string.IsNullOrWhiteSpace(tag) &&
                string.IsNullOrWhiteSpace(id) &&
                !classList.Any() &&
                !attributes.Any())
            {
                return new HtmlTag()
                {
                    Text = text,
                };
            }

            throw new FormatException($"Tag name is missing (Value: {htmlTag})");

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

        private static List<string> SplitAbbreviationAt(string abbreviation, char delimiter)
        {
            var result = new List<string>();
            var sb = new StringBuilder();
            var nest = 0;
            var inText = false;
            var inAttr = false;
            var trimmedAbbr = TrimParenthesis(abbreviation);
            for (var i = 0; i < trimmedAbbr.Length; i++)
            {
                // Update status
                switch (trimmedAbbr[i])
                {
                    case '{' when !inText && !inAttr:
                        inText = true;
                        break;
                    case '}' when inText && !inAttr:
                        inText = false;
                        break;
                    case '[' when !inText && !inAttr:
                        inAttr = true;
                        break;
                    case ']' when !inText && inAttr:
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

        private static string TrimParenthesis(string value)
        {
            while (value.Length > 2
                && value[0] == '('
                && value[value.Length - 1] == ')')
            {
                if (HasNonNestedPart(value))
                {
                    return value;
                }
                value = value.Substring(1, value.Length - 2);
            }

            return value;

            bool HasNonNestedPart(string v)
            {
                var nest = 0;
                for (var i = 0; i < v.Length; i++)
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
