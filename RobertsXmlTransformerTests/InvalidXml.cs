using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;

namespace RobertsXmlTransformerTests
{
    [TestClass]
    public class InvalidXml
    {
        private readonly string TargetXml = @"<?xml version=""1.0""?><a><b key=""abc""></b></a>";

        [TestMethod]
        public void MismatchedElementTag()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <c key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""Replaced""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            AssertRaisesException<XmlException>(() =>
                t.ApplyTransform(TargetDocument, XmlTransform),
                @"The 'c' start tag on line 3 position 22 does not match the end tag of 'b'. Line 3, position 86.");
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
