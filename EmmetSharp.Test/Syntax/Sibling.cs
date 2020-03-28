﻿using System;
using EmmetSharp.Renderer;
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
            var actual = AbbreviationRenderer.Render("div+p");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Sibling_EmptyNode_ShouldFormatError()
        {
            AbbreviationRenderer.Render("div++p");
        }

        [TestMethod]
        public void Sibling_AsChild_CanParse()
        {
            var expected =
                "<p>" +
                    "<a></a>" +
                    "<span></span>" +
                "</p>";
            var actual = AbbreviationRenderer.Render("p>a+span");
            Assert.AreEqual(expected, actual);
        }
    }
}
