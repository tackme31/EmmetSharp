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

        public static string Render(string abbreviation, Func<Node, Node> nodeFormatter = null)
        {
            if (string.IsNullOrWhiteSpace(abbreviation))
            {
                throw new ArgumentException($"The argument '{nameof(abbreviation)}' should be not null or empty.");
            }

            var rootNode = AbbreviationParser.Parse(abbreviation);
            return RenderInner(rootNode.Children, nodeFormatter);
        }

        private static string RenderInner(IList<Node> nodes, Func<Node, Node> nodeFormatter = null)
        {
            if (nodes == null || !nodes.Any())
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var node in nodes)
            {
                var formattedNode = nodeFormatter?.Invoke(node) ?? node;
                // Text node
                if (string.IsNullOrWhiteSpace(formattedNode.Tag))
                {
                    var text = formattedNode.Text ?? string.Empty;
                    sb.Append(text);

                    var innerHtml = RenderInner(formattedNode.Children, nodeFormatter);
                    sb.Append(innerHtml);
                    continue;
                }

                var startTag = HtmlTagRenderer.RenderStartTag(formattedNode);
                sb.Append(startTag);

                if (!string.IsNullOrWhiteSpace(formattedNode.Text))
                {
                    var text = formattedNode.Text ?? string.Empty;
                    sb.Append(text);
                }

                if (formattedNode.Children != null)
                {
                    var innerHtml = RenderInner(formattedNode.Children, nodeFormatter);
                    sb.Append(innerHtml);
                }

                if (!NoEndTags.Contains(formattedNode.Tag.ToLower()))
                {
                    var endTag = HtmlTagRenderer.RenderEndTag(formattedNode);
                    sb.Append(endTag);
                }
            }

            return sb.ToString();
        }
    }
}
