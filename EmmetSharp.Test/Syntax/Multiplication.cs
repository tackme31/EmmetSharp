using System;
using EmmetSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmmetSharp.Test.Syntax
{
    [TestClass]
    public class Multiplication
    {
        [TestMethod]
        public void Multiplication_WithSingleTag_CanParse()
        {
            var expected =
                "<li></li>" +
                "<li></li>";
            var actual = Emmet.Render("li*2");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Multiplication_WithSibling_CanParse()
        {
            var expected =
                "<li></li><a></a>" +
                "<li></li><a></a>";
            var actual = Emmet.Render("(li+a)*2");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Multiplication_WithChild_CanParse()
        {
            var expected =
                "<div>" +
                    "<li></li>" +
                "</div>" +
                "<div>" +
                    "<li></li>" +
                "</div>";
            var actual = Emmet.Render("(div>li)*2");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Multiplication_ParentBy_CanParse()
        {
            var expected =
                "<div>" +
                    "<li></li>" +
                "</div>" +
                "<div>" +
                    "<li></li>" +
                "</div>";
            var actual = Emmet.Render("div*2>li");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Multiplication_WithNestedChild_CanParse()
        {
            var expected =
                "<div>" +
                    "<div>" +
                        "<li></li>" +
                    "</div>" +
                    "<div>" +
                        "<li></li>" +
                    "</div>" +
                "</div>";
            var actual = Emmet.Render("div>(div>li)*2");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Multiplication_MultiplePart_CanParse()
        {
            var expected =
                "<div>1</div><div>4</div>" +
                "<div>2</div><div>5</div>" +
                "<div>3</div><div>6</div>";
            var actual = Emmet.Render("(div{$}+div{$@4})*3");
            Assert.AreEqual(expected, actual);
        }
    }
}
