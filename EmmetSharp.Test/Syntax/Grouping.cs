﻿using System;
using EmmetSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmmetSharp.Test.Syntax
{
    [TestClass]
    public class Grouping
    {
        [TestMethod]
        public void Grouping_TopLevel_CanParse()
        {
            var expected =
                "<a></a>" +
                "<p></p>";
            var actual = Emmet.Render("(a+p)");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Grouping_SiblingAsChild_CanParse()
        {
            var expected =
                "<div>" +
                    "<a></a>" +
                    "<p></p>" +
                "</div>";
            var actual = Emmet.Render("div>(a+p)");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Grouping_ChildInMiddleSiblings_CanParse()
        {
            var expected =
                "<p></p>" +
                "<div>" +
                    "<h1></h1>" +
                "</div>" +
                "<p></p>";
            var actual = Emmet.Render("p+(div>h1)+p");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Grouping_MultiNested_CanParse()
        {
            var expected =
                "<p></p>" +
                "<a></a>" +
                "<span></span>" +
                "<h1></h1>";
            var actual = Emmet.Render("((p+a)+span)+h1");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Grouping_MissingOpen_ShouldFormatError()
        {
            Emmet.Render("(a+p))");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Grouping_MissingClose_ShouldFormatError()
        {
            Emmet.Render("((a+p)");
        }
    }
}
