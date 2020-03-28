using System;
using EmmetSharp.Renderer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmmetSharp.Test.Syntax
{
    [TestClass]
    public class Child
    {
        [TestMethod]
        public void Child_DoubleNode_CanParse()
        {
            var expected =
                "<div>" +
                    "<p>" +
                    "</p>" +
                "</div>";
            var actual = ExpressionRenderer.Render("div>p");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Child_EmptyNode_ShouldFormatError()
        {
            ExpressionRenderer.Render("div>>p");
        }
    }
}
