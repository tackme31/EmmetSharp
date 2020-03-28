using EmmetSharp.Models;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace EmmetSharp.Renderer
{
    public static class HtmlTagRenderer
    {
        public static string RenderStartTag(Node node, Func<Node, Node> nodeFomatter = null)
        {
            if (node == null)
            {
                throw new ArgumentNullException($"The argument '{nameof(node)}' should be not null.");
            }

            node = nodeFomatter?.Invoke(node) ?? node;
            var sb = new StringBuilder();

            sb.Append("<");
            sb.Append(node.Tag.Escape());

            if (!string.IsNullOrWhiteSpace(node.Id))
            {
                sb.Append(" id=\"");
                sb.Append(node.Id.Escape());
                sb.Append("\"");
            }

            if (node.ClassList != null && node.ClassList.Any())
            {
                sb.Append(" class=\"");

                var firstClass = node.ClassList.First();
                sb.Append(firstClass.Escape());

                foreach (var @class in node.ClassList.Skip(1))
                {
                    sb.Append(" ");
                    sb.Append(@class.Escape());

                }

                sb.Append("\"");
            }

            if (node.Attributes != null && node.Attributes.Any())
            {
                foreach (var attribute in node.Attributes)
                {
                    sb.Append(" ");
                    sb.Append(attribute.Key.Escape());
                    sb.Append("=\"");
                    sb.Append(attribute.Value.Escape()); ;
                    sb.Append("\"");
                }
            }

            sb.Append(">");

            return sb.ToString();
        }

        public static string RenderEndTag(Node node)
        {
            if (node == null)
            {
                throw new ArgumentNullException($"The argument '{nameof(node)}' should be not null.");
            }

            return $"</{node.Tag}>";
        }

        internal static string Escape(this string text)
        {
            return WebUtility.HtmlEncode(text);
        }
    }
}
