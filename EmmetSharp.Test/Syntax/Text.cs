using System;
using EmmetSharp;
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
            var actual = Emmet.Render("p{text}");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Text_WithoutTag_CanParse()
        {
            var expected = "text";
            var actual = Emmet.Render("{text}");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Text_Sibling_CanParse()
        {
            var expected = "click <a>here</a>";
            var actual = Emmet.Render("{click }+a{here}");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Text_WithoutTagWithId_ShouldFormatError()
        {
            Emmet.Render("#id{text}");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Text_WithoutTagWithClass_ShouldFormatError()
        {
            Emmet.Render(".class{text}");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Text_WithoutTagWithAttribute_ShouldFormatError()
        {
            Emmet.Render("[attr=\"value\"]{text}");
        }
    }
}
