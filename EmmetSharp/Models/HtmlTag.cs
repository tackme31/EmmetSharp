using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace EmmetSharp.Models
{
    public class HtmlTag
    {
        public string TagName { get; set; }
        public string Id { get; set; }
        public ICollection<string> ClassList { get; set; }
        public IList<HtmlTag> Children { get; set; }
        public IDictionary<string, string> Attributes { get; set; }
        public string Text { get; set; }
        internal bool IsTextNode =>
            !string.IsNullOrWhiteSpace(Text) &&
            string.IsNullOrWhiteSpace(TagName) &&
            string.IsNullOrWhiteSpace(Id) &&
            (ClassList == null || !ClassList.Any()) &&
            (Attributes == null || !Attributes.Any());

        public string ToString(TagType type = TagType.Normal)
        {
            var sb = new StringBuilder();

            if (type == TagType.Start || type == TagType.Normal)
            {
                AppendStartTag(sb);
            }

            if (type == TagType.Normal)
            {
                sb.Append(WebUtility.HtmlEncode(Text));
            }

            if (type == TagType.End || type == TagType.Normal)
            {
                AppendEndTag(sb);
            }

            return sb.ToString();
        }

        private void AppendStartTag(StringBuilder sb)
        {
            sb.Append("<");
            sb.Append(WebUtility.HtmlEncode(TagName));

            if (!string.IsNullOrWhiteSpace(Id))
            {
                sb.Append(" id=\"");
                sb.Append(WebUtility.HtmlEncode(Id));
                sb.Append("\"");
            }

            if (ClassList != null && ClassList.Any())
            {
                sb.Append(" class=\"");

                var firstClass = ClassList.First();
                sb.Append(WebUtility.HtmlEncode(firstClass));

                foreach (var @class in ClassList.Skip(1))
                {
                    sb.Append(" ");
                    sb.Append(WebUtility.HtmlEncode(@class));

                }

                sb.Append("\"");
            }

            if (Attributes != null && Attributes.Any())
            {
                foreach (var attribute in Attributes)
                {
                    sb.Append(" ");
                    sb.Append(WebUtility.HtmlEncode(attribute.Key));
                    sb.Append("=\"");
                    sb.Append(WebUtility.HtmlEncode(attribute.Value));
                    sb.Append("\"");
                }
            }

            sb.Append(">");
        }

        private void AppendEndTag(StringBuilder sb)
        {
            sb.Append("</");
            sb.Append(WebUtility.HtmlEncode(TagName));
            sb.Append(">");
        }
    }

    public enum TagType { Normal, Start, End };
}
