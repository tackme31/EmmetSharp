using EmmetSharp.Models;
using EmmetSharp.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmmetSharp.Renderer
{
    public static class AbbreviationRenderer
    {
        private static readonly HashSet<string> NoEndTags = new HashSet<string> { "br", "hr", "img", "input", "meta", "area", "base", "col", "embed", "keygen", "link", "param", "source" };

        public static string Render(string abbreviation, Func<string, string> textFormatter = null)
        {
            if (string.IsNullOrWhiteSpace(abbreviation))
            {
                throw new ArgumentException($"The argument '{nameof(abbreviation)}' should be not null.");
            }

            var rootNode = AbbreviationParser.Parse(abbreviation);
            return RenderInner(rootNode.Children, textFormatter);
        }

        private static string RenderInner(IList<Node> nodes, Func<string, string> textFormatter = null)
        {
            if (nodes == null || !nodes.Any())
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var node in nodes)
            {
                // Text node
                if (string.IsNullOrWhiteSpace(node.Tag))
                {
                    var text = textFormatter?.Invoke(node.Text) ?? node.Text;
                    sb.Append(text);

                    var innerHtml = RenderInner(node.Children, textFormatter);
                    sb.Append(innerHtml);
                    continue;
                }

                var startTag = HtmlTagRenderer.RenderStartTag(node);
                sb.Append(startTag);

                if (!string.IsNullOrWhiteSpace(node.Text))
                {
                    var text = textFormatter?.Invoke(node.Text) ?? node.Text;
                    sb.Append(text);
                }

                if (node.Children != null)
                {
                    var innerHtml = RenderInner(node.Children, textFormatter);
                    sb.Append(innerHtml);
                }

                if (!NoEndTags.Contains(node.Tag.ToLower()))
                {
                    var endTag = HtmlTagRenderer.RenderEndTag(node);
                    sb.Append(endTag);
                }
            }

            return sb.ToString();
        }
    }
}
