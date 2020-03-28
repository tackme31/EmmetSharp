using System;
using EmmetSharp.Renderer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmmetSharp.Test.Syntax
{
    [TestClass]
    public class Node
    {
        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Node_InvaidAttributesOrder1_ShouldFormatError()
        {
            AbbreviationRenderer.Render("div.class#id");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Node_InvaidAttributesOrder2_ShouldFormatError()
        {
            AbbreviationRenderer.Render("div[attr=\"value\"]#id");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Node_InvaidAttributesOrder3_ShouldFormatError()
        {
            AbbreviationRenderer.Render("div[attr=\"value\"].class");
        }

        [TestMethod]
        public void Node_HasAllPattern_CanParse()
        {
            var expected = "<div id=\"id\" class=\"class1 class2\" attr1=\"value1\" attr2=\"\">text</div>";
            var actual = AbbreviationRenderer.Render("div#id.class1.class2[attr1=\"value1\" attr2]{text}");
            Assert.AreEqual(expected, actual);
        }
    }
}
