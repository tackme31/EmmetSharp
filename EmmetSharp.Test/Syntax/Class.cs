using System;
using EmmetSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmmetSharp.Test.Syntax
{
    [TestClass]
    public class Class
    {
        [TestMethod]
        public void Class_Single_CanParse()
        {
            var expected = "<div class=\"class\"></div>";
            var actual = Emmet.Render("div.class");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Class_Multiple_CanParse()
        {
            var expected = "<div class=\"class1 class2\"></div>";
            var actual = Emmet.Render("div.class1.class2");
            Assert.AreEqual(expected, actual);
        }
    }
}
