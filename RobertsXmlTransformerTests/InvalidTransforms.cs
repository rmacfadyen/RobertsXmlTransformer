using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;

namespace RobertsXmlTransformerTests
{
    [TestClass]
    public class InvalidTransforms
    {
        private readonly string CommonTargetXml = @"<?xml version=""1.0""?><a><b key=""abc""></b></a>";


        [TestMethod]
        public void InsertToNodeWithMissingParent()
        {
            var TargetXml = @"<?xml version=""1.0""?><a></a>";

            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b>
                        <c key=""abc"" value=""ghi"" xdt:Transform=""Insert""></c>
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"The Locator XPath '//a/b' did not match an element in the target document, cannot apply Transform Insert on element <c key=""abc"" value=""ghi"" xdt:Transform=""Insert""> (line 4, column 26)");
        }



        [TestMethod]
        public void UnknwnTransform()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""Replaced""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"Invalid Transform type: Replaced on element <b key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""Replaced""> (line 3, column 22)");
        }


        [TestMethod]
        public void CommentWithoutCloseParenthesis()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""Comment(abc""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"Invalid Transform value 'Comment(abc', does not end with a close parenthesis on element <b key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""Comment(abc""> (line 3, column 22)");
        }

        [TestMethod]
        public void SetAttributesUnknownAttribute()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" notvalue=""ghi"" xdt:Locator=""Match(key)"" xdt:Transform=""SetAttributes(value)""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"Transform SetAtttibutes specifies a nonexistant attribute: 'value' on element <b key=""abc"" notvalue=""ghi"" xdt:Locator=""Match(key)"" xdt:Transform=""SetAttributes(value)""> (line 3, column 22)");
        }

        [TestMethod]
        public void InsertBeforeMissingXPath()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b><b key=""xyz""></b></a>";

            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <c key=""abc"" value=""ghi"" xdt:Locator=""XPath(//a/b[@key='abc'])"" xdt:Transform=""InsertBefore()""></c>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"Transform InsertAfter cannot have an empty XPath argument on element <c key=""abc"" value=""ghi"" xdt:Locator=""XPath(//a/b[@key='abc'])"" xdt:Transform=""InsertBefore()""> (line 3, column 22)");
        }
        
        [TestMethod]
        public void InsertAfterMissingXPath()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b><b key=""xyz""></b></a>";

            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <c key=""abc"" value=""ghi"" xdt:Locator=""XPath(//a/b[@key='abc'])"" xdt:Transform=""InsertAfter""></c>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"Transform InsertAfter cannot have an empty XPath argument on element <c key=""abc"" value=""ghi"" xdt:Locator=""XPath(//a/b[@key='abc'])"" xdt:Transform=""InsertAfter""> (line 3, column 22)");
        }


        [TestMethod]
        public void EmptyComment()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""Comment()""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"Transform Comment cannot have an empty argument on element <b key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""Comment()""> (line 3, column 22)");
        }


        [TestMethod]
        public void RemoveAttributesEmptyAttributes()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" notvalue=""ghi"" xdt:Locator=""Match(key)"" xdt:Transform=""RemoveAttributes""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"Transform RemoveAttributes cannot have an empty list of attributes on element <b key=""abc"" notvalue=""ghi"" xdt:Locator=""Match(key)"" xdt:Transform=""RemoveAttributes""> (line 3, column 22)");
        }

        [TestMethod]
        public void RemoveAttributesNoAttributes()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" notvalue=""ghi"" xdt:Locator=""Match(key)"" xdt:Transform=""RemoveAttributes()""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"Transform RemoveAttributes cannot have an empty list of attributes on element <b key=""abc"" notvalue=""ghi"" xdt:Locator=""Match(key)"" xdt:Transform=""RemoveAttributes()""> (line 3, column 22)");
        }


        

        [TestMethod]
        public void BadTransformAtRoot()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"" xdt:Transform=""Replaced"">
                    <b>
                        <c key=""ghi"" value=""lmno"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
             @"Invalid Transform type: Replaced on element <a xdt:Transform=""Replaced""> (line 2, column 20)");
        }


        private static void AssertRaisesException<TException>(Action action, string expectedMessage) where TException : Exception
        {
            try
            {
                action();
                Assert.Fail(
                    $"Call suceeded. Expected exception of type: {typeof(TException).Name} with message: {expectedMessage}");
            }
            catch (Exception ex)
            {
                if (ex is AssertFailedException)
                    throw;

                var exception = ex as TException;
                Assert.IsNotNull(exception,
                    $"Expected exception of type: {typeof(TException).Name}, actual type: {ex.GetType().Name}");
                Assert.AreEqual(expectedMessage, exception.Message);
            }
        }

    }
}
