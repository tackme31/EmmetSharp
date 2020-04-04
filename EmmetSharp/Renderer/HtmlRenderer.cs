using EmmetSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace EmmetSharp.Renderer
{
    public static class HtmlRenderer
    {
        private static readonly HashSet<string> NoEndTags = new HashSet<string> { "br", "hr", "img", "input", "meta", "area", "base", "col", "embed", "keygen", "link", "param", "source" };

        public static string Render(HtmlTag tag, Func<HtmlTag, HtmlTag> tagFormatter = null, bool escape = true)
        {
            if (tag == null)
            {
                throw new ArgumentNullException($"Argument '{nameof(tag)}' cannot be null.");
            }

            return RenderInner(tag, tagFormatter, escape);
        }

        private static string RenderInner(HtmlTag tag, Func<HtmlTag, HtmlTag> tagFormatter, bool escape)
        {
            var sb = new StringBuilder();

            var formattedTag = tagFormatter?.Invoke(tag) ?? tag;
            if (!string.IsNullOrWhiteSpace(formattedTag.TagName))
            {
                var startTag = formattedTag.ToString(TagType.Start);
                sb.Append(startTag);
            }

            if (!string.IsNullOrWhiteSpace(formattedTag.Text))
            {
                var text = escape
                    ? WebUtility.HtmlEncode(formattedTag.Text)
                    : formattedTag.Text;
                sb.Append(text);
            }

            foreach (var child in formattedTag?.Children ?? Enumerable.Empty<HtmlTag>())
            {
                if (formattedTag.Children != null)
                {
                    var innerHtml = RenderInner(child, tagFormatter, escape);
                    sb.Append(innerHtml);
                }
            }

            if (!string.IsNullOrWhiteSpace(formattedTag.TagName) &&
                !NoEndTags.Contains(formattedTag.TagName.ToLower()))
            {
                var endTag = formattedTag.ToString(TagType.End);
                sb.Append(endTag);
            }

            return sb.ToString();
        }
    }
}
