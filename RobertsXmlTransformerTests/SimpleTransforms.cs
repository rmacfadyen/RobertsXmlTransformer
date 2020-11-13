using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;

namespace RobertsXmlTransformerTests
{
    [TestClass]
    public class SimpleTransforms
    {
        private readonly string CommonTargetXml = @"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b></a>";

        [TestMethod]
        public void RemoveNonExistantElement()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <c key=""abc"" xdt:Transform=""Remove"" />
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void RemovePriorCommentNonExistantElement()
        {
            var TargetDocument = new XmlDocument { PreserveWhitespace = true };
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <c key=""abc"" xdt:Transform=""RemovePreviousComment"" />
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void RemoveNonExistantElementWithLocator()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <c key=""abc"" xdt:Locator=""match(key)"" xdt:Transform=""Remove"" />
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void InsertWithIndentation()
        {
            var TargetXml = "<?xml version=\"1.0\"?>\r\n<configuration>\r\n  <appSettings>\r\n    <add key=\"abc\" value=\"def\" />\r\n  </appSettings>\r\n</configuration>";
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <configuration xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <appSettings>
                      <add key=""ghi"" value=""klm"" xdt:Transform=""Insert"" />
                    </appSettings>
                  </configuration>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual("<?xml version=\"1.0\"?>\r\n<configuration>\r\n  <appSettings>\r\n    <add key=\"abc\" value=\"def\" />\r\n    <add key=\"ghi\" value=\"klm\" />\r\n  </appSettings>\r\n</configuration>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void InsertWithIndentationAndComment()
        {
            var TargetXml = "<?xml version=\"1.0\"?>\r\n<configuration>\r\n  <appSettings>\r\n    <add key=\"abc\" value=\"def\" />\r\n  </appSettings>\r\n</configuration>";
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <configuration xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <appSettings>
                      <add key=""ghi"" value=""klm"" xdt:Transform=""Insert"" />
                      <add key=""ghi"" xdt:Locator=""match(key)"" xdt:Transform=""comment(\r\n    Hello\r\n    )"" />
                    </appSettings>
                  </configuration>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual("<?xml version=\"1.0\"?>\r\n<configuration>\r\n  <appSettings>\r\n    <add key=\"abc\" value=\"def\" />\r\n    <!--\r\n    Hello\r\n    -->\r\n    <add key=\"ghi\" value=\"klm\" />\r\n  </appSettings>\r\n</configuration>", TargetDocument.InnerXml);
        }


        [TestMethod]
        public void SimpleReplace()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" value=""ghi"" second=""lmno"" xdt:Locator=""Match(key)"" xdt:Transform=""Replace""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""abc"" value=""ghi"" second=""lmno"" /></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void ReplaceWithChildren()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" value=""ghi"" second=""lmno"" xdt:Locator=""Match(key)"" xdt:Transform=""Replace""><c f=""xyz""></c></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""abc"" value=""ghi"" second=""lmno""><c f=""xyz"" /></b></a>", TargetDocument.InnerXml);
        }



        [TestMethod]
        public void SimpleInsert()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b><b key=""xyz""></b></a>";

            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);
            
            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <c key=""abc"" value=""ghi"" xdt:Transform=""Insert""></c>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b><b key=""xyz""></b><c key=""abc"" value=""ghi"" /></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void SimpleInsertToNodeWithoutChildren()
        {
            var TargetXml = @"<?xml version=""1.0""?><a></a>";

            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <c key=""abc"" value=""ghi"" xdt:Transform=""Insert""></c>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><c key=""abc"" value=""ghi"" /></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void SimpleInsertUsingXPathLocator()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b><b key=""xyz""></b></a>";

            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <c key=""abc"" value=""ghi"" xdt:Locator=""XPath(//a/b[@key='abc'])"" xdt:Transform=""Insert""></c>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""abc"" value=""def""><c key=""abc"" value=""ghi"" /></b><b key=""xyz""></b></a>", TargetDocument.InnerXml);
        }


        [TestMethod]
        public void InsertWithChildren()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b><b key=""xyz""></b></a>";

            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <c key=""abc"" value=""ghi"" xdt:Locator=""XPath(//a/b[@key='abc'])"" xdt:Transform=""Insert""><d key=""lmno""></d></c>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""abc"" value=""def""><c key=""abc"" value=""ghi""><d key=""lmno"" /></c></b><b key=""xyz""></b></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void SimpleComment()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""Comment(abc 123)""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual("<?xml version=\"1.0\"?><a><!--abc 123--><b key=\"abc\" value=\"def\"></b></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void SimpleCommentIndentedCorrectly()
        {
            var TargetXml = "<?xml version=\"1.0\"?>\r\n<a>\r\n  <b key=\"abc\" value=\"def\">\r\n  </b>\r\n</a>";
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""Comment(\r\nabc 123\r\n)""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual("<?xml version=\"1.0\"?>\r\n<a>\r\n  <!--\r\nabc 123\r\n-->\r\n  <b key=\"abc\" value=\"def\">\r\n  </b>\r\n</a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void SimpleInsertAfter()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b><b key=""xyz""></b></a>";

            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <c key=""abc"" value=""ghi"" xdt:Locator=""XPath(//a/b[@key='abc'])"" xdt:Transform=""InsertAfter(//a/b[@key='abc'])""></c>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b><c key=""abc"" value=""ghi"" /><b key=""xyz""></b></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void InsertAfterWithChildren()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b><b key=""xyz""></b></a>";

            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <c key=""abc"" value=""ghi"" xdt:Locator=""XPath(//a/b[@key='abc'])"" xdt:Transform=""InsertAfter(//a/b[@key='abc'])""><d key=""lmno""></d></c>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b><c key=""abc"" value=""ghi""><d key=""lmno"" /></c><b key=""xyz""></b></a>", TargetDocument.InnerXml);
        }


        [TestMethod]
        public void SimpleInsertBefore()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b><b key=""xyz""></b></a>";

            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <c key=""abc"" value=""ghi"" xdt:Locator=""XPath(//a/b[@key='abc'])"" xdt:Transform=""InsertBefore(//a/b[@key='abc'])""></c>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><c key=""abc"" value=""ghi"" /><b key=""abc"" value=""def""></b><b key=""xyz""></b></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void InsertBeforeWithChildren()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b><b key=""xyz""></b></a>";

            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <c key=""abc"" value=""ghi"" xdt:Locator=""XPath(//a/b[@key='abc'])"" xdt:Transform=""InsertBefore(//a/b[@key='abc'])""><d key=""lmno""></d></c>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><c key=""abc"" value=""ghi""><d key=""lmno"" /></c><b key=""abc"" value=""def""></b><b key=""xyz""></b></a>", TargetDocument.InnerXml);
        }


        [TestMethod]
        public void SimpleRemove()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""Remove""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void SimpleRemoveAttributes()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" xdt:Locator=""Match(key)"" xdt:Transform=""RemoveAttributes(value)""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""abc""></b></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void SimpleSetAttributes()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" value=""ghi"" second=""lmno"" xdt:Locator=""Match(key)"" xdt:Transform=""SetAttributes""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""abc"" value=""ghi"" second=""lmno""></b></a>", TargetDocument.InnerXml);
        }
        [TestMethod]
        public void SimpleSetAttributesMultipleMatches()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" value=""def"" second=""lmno"" xdt:Locator=""Match(key,value)"" xdt:Transform=""SetAttributes""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""abc"" value=""def"" second=""lmno""></b></a>", TargetDocument.InnerXml);
        }
        [TestMethod]
        public void SimpleSetAttributesUsingConditionLocator()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" value=""def"" second=""lmno"" xdt:Locator=""Condition(@key='abc')"" xdt:Transform=""SetAttributes""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""abc"" value=""def"" second=""lmno""></b></a>", TargetDocument.InnerXml);
        }
        [TestMethod]
        public void SetAttributesSpecific()
        {
            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(CommonTargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b key=""abc"" value=""ghi"" xdt:Locator=""Match(key)"" xdt:Transform=""SetAttributes(value)""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a><b key=""abc"" value=""ghi""></b></a>", TargetDocument.InnerXml);
        }

        [TestMethod]
        public void SimpleRemoveAll()
        {
            var TargetXml = @"<?xml version=""1.0""?><a><b key=""abc"" value=""def""></b><b key=""xyz""></b></a>";

            var TargetDocument = new XmlDocument {PreserveWhitespace = true};
            TargetDocument.LoadXml(TargetXml);

            var XmlTransform =
                @"<?xml version=""1.0""?>
                  <a xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
                    <b xdt:Transform=""RemoveAll""></b>
                  </a>";

            var t = new RobertsXmlTransformer.Transformer();

            t.ApplyTransform(TargetDocument, XmlTransform);

            Assert.AreEqual(@"<?xml version=""1.0""?><a></a>", TargetDocument.InnerXml);
        }

    }
}
