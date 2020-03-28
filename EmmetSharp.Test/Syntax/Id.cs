using System;
using EmmetSharp.Renderer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmmetSharp.Test.Syntax
{
    [TestClass]
    public class Id
    {
        [TestMethod]
        public void Id_Single_CanParse()
        {
            var expected = "<div id=\"id\"></div>";
            var actual = ExpressionRenderer.Render("div#id");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Id_Multiple_ShouldFormatError()
        {
            ExpressionRenderer.Render("div#id1#id2");
        }
    }
}
