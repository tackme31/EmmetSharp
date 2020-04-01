using System;
using EmmetSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmmetSharp.Test.Syntax
{
    [TestClass]
    public class ClimbUp
    {
        [TestMethod]
        public void ClimbUp_SingleHut_CanParse()
        {
            var expected =
                "<p>" +
                    "<a><span></span></a>" +
                    "<h1></h1>" +
                "</p>";
            var actual = Emmet.Expand("p>a>span^h1");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ClimbUp_MultipleHut_CanParse()
        {
            var expected =
                "<p>" +
                    "<a><span></span></a>" +
                "</p>" +
                "<h1></h1>";
            var actual = Emmet.Expand("p>a>span^^h1");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ClimbUp_MultiClimbing_CanParse()
        {
            var expected =
                "<p>" +
                    "<a><span></span></a>" +
                    "<h1></h1>" +
                "</p>" +
                "<i></i>";
            var actual = Emmet.Expand("p>a>span^h1^i");
            Assert.AreEqual(expected, actual);
        }
    }
}
