using System;
using EmmetSharp.Renderer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmmetSharp.Test.Syntax
{
    [TestClass]
    public class Text
    {
        [TestMethod]
        public void Text_WithTag_CanParse()
        {
            var expected = "<p>text</p>";
            var actual = AbbreviationRenderer.Render("p{text}");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Text_WithoutTag_CanParse()
        {
            var expected = "text";
            var actual = AbbreviationRenderer.Render("{text}");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Text_Sibling_CanParse()
        {
            var expected = "click <a>here</a>";
            var actual = AbbreviationRenderer.Render("{click }+a{here}");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Text_WithoutTagWithId_ShouldFormatError()
        {
            AbbreviationRenderer.Render("#id{text}");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Text_WithoutTagWithClass_ShouldFormatError()
        {
            AbbreviationRenderer.Render(".class{text}");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Text_WithoutTagWithAttribute_ShouldFormatError()
        {
            AbbreviationRenderer.Render("[attr=\"value\"]{text}");
        }
    }
}
