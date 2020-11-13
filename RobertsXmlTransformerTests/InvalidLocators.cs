using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;

namespace RobertsXmlTransformerTests
{
    [TestClass]
    public class InvalidLocators
    {
        private readonly string TargetXml = @"<?xml version=""1.0""?><a><b key=""abc""></b></a>";


        [TestMethod]
        public void UnknownLocatorType()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b xdt:Locator=""Matches(key)""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() => 
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"Invalid Locator value 'Matches(key)', must be Match, Condition or Xpath on element <b xdt:Locator=""Matches(key)""> (line 3, column 22)");
        }


        [TestMethod]
        public void MissingOpenParenthesis()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b xdt:Locator=""Match""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"Invalid Locator value 'Match', does not include an open parenthesis on element <b xdt:Locator=""Match""> (line 3, column 22)");
        }


        [TestMethod]
        public void MissingCloseParenthesis()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b xdt:Locator=""Match(key""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"Invalid Locator value 'Match(key', does not end with a close parenthesis on element <b xdt:Locator=""Match(key""> (line 3, column 22)");
        }
        
        
        [TestMethod]
        public void MatchAgainstNonExistantAttribute()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b xdt:Locator=""Match(key)""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"Match Locator specified a nonexistant attribute: 'key' on element <b xdt:Locator=""Match(key)""> (line 3, column 22)");
        }

        [TestMethod]
        public void MatchNoAttribute()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b xdt:Locator=""Match()""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"Invalid Locator value 'Match()', does not specify any attribute names on element <b xdt:Locator=""Match()""> (line 3, column 22)");
        }


        [TestMethod]
        public void MatchAgainstNonTargetLocation()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <c key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""Replace""></c>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"The Locator XPath '//a/c[@key='abc']' did not match an element in the target document, cannot apply transform on element <c key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""Replace""> (line 3, column 22)");
        }


        [TestMethod]
        public void EmptyCondition()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" xdt:Locator=""Condition()"" xdt:Transform=""Replace""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"Invalid Locator value 'Condition()', does not specify a condition on element <b key=""abc"" xdt:Locator=""Condition()"" xdt:Transform=""Replace""> (line 3, column 22)");
        }

        [TestMethod]
        public void EmptyXPath()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" xdt:Locator=""XPath()"" xdt:Transform=""Replace""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"Invalid Locator value 'XPath()', does not specify an xpath on element <b key=""abc"" xdt:Locator=""XPath()"" xdt:Transform=""Replace""> (line 3, column 22)");
        }

        [TestMethod]
        public void IllegalXPath()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" xdt:Locator=""XPath(@abc=)"" xdt:Transform=""Replace""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<ArgumentException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"The Locator XPath '@abc=' for a Transform threw an exception (Expression must evaluate to a node-set.) on element <b key=""abc"" xdt:Locator=""XPath(@abc=)"" xdt:Transform=""Replace""> (line 3, column 22)");
        }


        private static void AssertRaisesException<TException>(Action action, string expectedMessage)
            where TException : Exception
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
