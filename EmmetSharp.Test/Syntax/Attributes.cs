using System;
using EmmetSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmmetSharp.Test.Syntax
{
    [TestClass]
    public class Attributes
    {
        [TestMethod]
        public void Attributes_WithValue_CanParse()
        {
            var expected = "<input type=\"checkbox\">";
            var actual = Emmet.Expand("input[type=\"checkbox\"]");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Attributes_WithoutValue_CanParse()
        {
            var expected = "<input disabled=\"\">";
            var actual = Emmet.Expand("input[disabled]");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Attributes_Multiple_CanParse()
        {
            var expected = "<input type=\"checkbox\" checked=\"\">";
            var actual = Emmet.Expand("input[type=\"checkbox\" checked]");
            Assert.AreEqual(expected, actual);
        }
    }
}
