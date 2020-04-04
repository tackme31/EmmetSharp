using EmmetSharp.Models;
using EmmetSharp.Parser;
using EmmetSharp.Renderer;
using System;
using System.Text;

namespace EmmetSharp
{
    public static class Emmet
    {
        public static string Expand(string abbreviation, Func<HtmlTag, HtmlTag> tagFormatter = null, bool escapeText = true)
        {
            if (string.IsNullOrWhiteSpace(abbreviation))
            {
                throw new ArgumentException($"Argument '{nameof(abbreviation)}' cannot be null or empty.");
            }

            var sb = new StringBuilder();
            var nodes = AbbreviationParser.Parse(abbreviation);
            foreach (var node in nodes)
            {
                var html = HtmlRenderer.Render(node, tagFormatter, escapeText);
                sb.Append(html);
            }

            return sb.ToString(); ;
        }
    }
}
