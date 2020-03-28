using System;
using EmmetSharp;
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
            var actual = Emmet.Render("div#id");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Id_Multiple_ShouldFormatError()
        {
            Emmet.Render("div#id1#id2");
        }
    }
}
