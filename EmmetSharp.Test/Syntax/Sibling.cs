using System;
using EmmetSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmmetSharp.Test.Syntax
{
    [TestClass]
    public class Sibling
    {
        [TestMethod]
        public void Sibling_DoubleNode_CanParse()
        {
            var expected =
                "<div></div>" +
                "<p></p>";
            var actual = Emmet.Render("div+p");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Sibling_EmptyNode_ShouldFormatError()
        {
            Emmet.Render("div++p");
        }

        [TestMethod]
        public void Sibling_AsChild_CanParse()
        {
            var expected =
                "<p>" +
                    "<a></a>" +
                    "<span></span>" +
                "</p>";
            var actual = Emmet.Render("p>a+span");
            Assert.AreEqual(expected, actual);
        }
    }
}
