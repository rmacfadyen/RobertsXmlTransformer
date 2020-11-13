using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;

namespace RobertsXmlTransformerTests
{
    [TestClass]
    public class ComplexTransforms
    {
        [TestMethod]
        public void RemovePreviousComment()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b><!--hello--><c key=""abc"" value=""def"" /></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b>
                        <c key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""RemovePreviousComment"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /></b></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void RemovePreviousCommentWithWhitespace()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b>  <!--hello-->  <c key=""abc"" value=""def"" /></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b>
                        <c key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""RemovePreviousComment"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /></b></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void RemoveFollowingComment()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /><!--hello--></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b>
                        <c key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""RemoveFollowingComment"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /></b></a>", TargetDocument.InnerXml);
        }
        
        [TestMethod]
        public void RemoveFollowingCommentWithWhitespace()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" />    <!--hello-->     </b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b>
                        <c key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""RemoveFollowingComment"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /></b></a>", TargetDocument.InnerXml);
        }
        [TestMethod]
        public void AppendWithWhitespace()
        {
            var TargetXml = @"<?xml version=""1.0""?><a>    <b key=""abc"" value=""def""></b></a>";

            var TargetDocument = new XmlDocument { PreserveWhitespace = true };
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <c key=""abc"" value=""ghi"" xdt:Locator=""XPath(//a/b[@key='abc'])"" xdt:Transform=""InsertAfter(//a/b[@key='abc'])""></c>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a>    <b key=""abc"" value=""def""></b>    <c key=""abc"" value=""ghi"" /></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void RemovePreviousCommentNoComment()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /><!--hello--></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b>
                        <c key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""RemovePreviousComment"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /><!--hello--></b></a>", TargetDocument.InnerXml);
        }
        [TestMethod]
        public void RemovePreviousAtRoot()
        {
            var TargetXml = @"<?xml version=""1.0""?><!--hello--><a><b><c key=""abc"" value=""def"" /><!--hello--></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"" xdt:Transform=""RemovePreviousComment"">
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /><!--hello--></b></a>", TargetDocument.InnerXml);
        }
        [TestMethod]
        public void RemovePreviousAtRootNoComment()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /><!--hello--></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"" xdt:Transform=""RemovePreviousComment"">
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /><!--hello--></b></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void RemoveFollowingAtRoot()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /><!--hello--></b></a><!--hello-->";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"" xdt:Transform=""RemoveFollowingComment"">
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /><!--hello--></b></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void RemoveFollowingCommentNoComment()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /></b><!--hello--></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b>
                        <c key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""RemoveFollowingComment"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /></b><!--hello--></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void CommentInTransform()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b><!-- something -->
                        <c key=""ghi"" value=""lmno"" xdt:Transform=""Insert"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /><c key=""ghi"" value=""lmno"" /></b></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void TransformUnderLocator()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b key=""1""><c key=""abc"" value=""def"" /></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""1"" xdt:Locator=""match(key)"">
                        <c key=""jkl"" value=""lmno"" xdt:Transform=""insert"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""1""><c key=""abc"" value=""def"" /><c key=""jkl"" value=""lmno"" /></b></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void NestedLocator()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b key=""1""><c key=""abc"" value=""def"" /></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""1"" xdt:Locator=""match(key)"">
                        <c key=""abc"" value=""lmno"" xdt:Locator=""match(key)"" xdt:Transform=""SetAttributes(value)"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""1""><c key=""abc"" value=""lmno"" /></b></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void NestedXPathLocator()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b key=""1""><c key=""abc"" value=""def"" /></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""1"" xdt:Locator=""XPath(//a/b[@key='1'])"">
                        <c key=""abc"" value=""lmno"" xdt:Locator=""match(key)"" xdt:Transform=""SetAttributes(value)"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""1""><c key=""abc"" value=""lmno"" /></b></a>", TargetDocument.InnerXml);
        }

        // TODO: do this for all transforms
        // TODO: what if there's a locator on the root element?
        [TestMethod]
        public void ReplaceEntireTarget()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"" xdt:Transform=""Replace""><b><c key=""ghi"" value=""lmno"" /></b></a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b><c key=""ghi"" value=""lmno"" /></b></a>", TargetDocument.InnerXml);
        }


        [TestMethod]
        public void InsertEntireTarget()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"" xdt:Transform=""Insert"">
                    <b>
                        <c key=""ghi"" value=""lmno"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
             @"The Locator XPath '/' did not match an element in the target document, cannot apply Transform Insert on element <a xdt:Transform=""Insert""> (line 2, column 20)");
        }

        [TestMethod]
        public void CommentEntireTarget()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"" xdt:Transform=""Comment(abc 123)"">
                    <b>
                        <c key=""ghi"" value=""lmno"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
             @"Cannot apply transform to root element on element <a xdt:Transform=""Comment(abc 123)""> (line 2, column 20)");
        }


        [TestMethod]
        public void InsertBeforeEntireTarget()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"" xdt:Transform=""InsertBefore(//a)"">
                    <b>
                        <c key=""ghi"" value=""lmno"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
             @"Cannot apply transform to root element on element <a xdt:Transform=""InsertBefore(//a)""> (line 2, column 20)");
        }

        [TestMethod]
        public void InsertafterEntireTarget()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"" xdt:Transform=""InsertAfter(//a)"">
                    <b>
                        <c key=""ghi"" value=""lmno"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
             @"Cannot apply transform to root element on element <a xdt:Transform=""InsertAfter(//a)""> (line 2, column 20)");
        }

        [TestMethod]
        public void SetAttributeEntireTarget()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a key=""123"" xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"" xdt:Transform=""SetAttributes(key)"">
                    <b>
                        <c key=""ghi"" value=""lmno"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);
            Assert.AreEqual(@"<?xml version=""1.0""?><a key=""123""><b><c key=""abc"" value=""def"" /></b></a>", TargetDocument.InnerXml);
            
        }

        [TestMethod]
        public void RemoveAttributeEntireTarget()
        {
            var TargetXml = @"<?xml version=""1.0""?><a key=""abc""><b><c key=""abc"" value=""def"" /></b></a>";


            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"" xdt:Transform=""RemoveAttributes(key)"">
                    <b>
                        <c key=""ghi"" value=""lmno"" />
                    </b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);
            Assert.AreEqual(@"<?xml version=""1.0""?><a><b><c key=""abc"" value=""def"" /></b></a>", TargetDocument.InnerXml);

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
