﻿using EmmetSharp.Models;
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

        private static readonly Regex NodeRegex = new Regex(
            @"^" +
            @"(?<tag>[^.#{}\[\]\s]+?)?" +
            @"(#(?<id>[^.#{}\[\]\s]+?))?" +
            @"(\.(?<class>[^.#{}\[\]\s]+?)){0,}" +
            @"(\[((?<attr>[^=.#{}\[\]\s]+(=""[^""]*"")?)\s?){0,}\])?" +
            @"({(?<text>.+)})?" +
            @"$",
            RegexOptions.Compiled | RegexOptions.Singleline);

        public static Node Parse(string abbreviation)
        {
            var root = CreateNode("root");
            var abbreviations = SplitAbbreviationAt(abbreviation, '>');
            root.Children = ParseInner(abbreviations);
            return root;
        }

        private static List<Node> ParseInner(List<string> abbreviations)
        {
            if (abbreviations.Count < 1)
            {
                return new List<Node>();
            }

            var firstAbbreviation = abbreviations[0];
            var firstSiblings = SplitAbbreviationAt(firstAbbreviation, '+');
            if (!MultiplicationRegex.IsMatch(firstAbbreviation) && abbreviations.Count == 1 && firstSiblings.Count == 1)
            {
                return new List<Node>()
                {
                    CreateNode(firstSiblings[0])
                };
            }

            var result = new List<Node>();
            var lastNodeMultiplir = 1;
            foreach (var sibling in firstSiblings)
            {
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
                    var nodes = ParseInner(siblingAbbreviations);
                    result.AddRange(nodes);
                }

                lastNodeMultiplir = multiplier;
            }

            var restAbbreviations = abbreviations.GetRange(1, abbreviations.Count - 1);
            if (result.Count > 0 && restAbbreviations.Count > 0)
            {
                var nodes = ParseInner(restAbbreviations);
                var lastNodes = result.GetRange(result.Count - lastNodeMultiplir, lastNodeMultiplir);

                // When the last node is multiplied, set its children to each node.
                foreach (var lastNode in lastNodes)
                {
                    lastNode.Children = nodes;
                }
            }

            return result;
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

        private static Node CreateNode(string node)
        {
            var tagMatch = NodeRegex.Match(node);
            if (!tagMatch.Success)
            {
                throw new FormatException($"Invalid format of the node abbreviation (Value: {node})");
            }

            var tag = tagMatch.Groups["tag"].Value;
            var id = tagMatch.Groups["id"].Value;
            var classList = GetCaptureValues(tagMatch, "class");
            var attributes = GetCaptureValues(tagMatch, "attr").Select(ParseAttribute).ToDictionary(attr => attr.name, attr => attr.value);
            var text = tagMatch.Groups["text"].Value;

            // HTML tag
            if (!string.IsNullOrWhiteSpace(tag))
            {
                return new Node
                {
                    Tag = tag,
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
                return new Node()
                {
                    Text = text,
                };
            }

            throw new FormatException($"Tag name is missing (Value: {node})");

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

            if (result.Any(exp => exp.Length == 0))
            {
                throw new FormatException($"An empty node is contained in the abbreviation (Value: {abbreviation})");
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
