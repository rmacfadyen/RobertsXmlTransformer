using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace RobertsXmlTransformer
{
    /// <summary>
    /// Simple class to hold the definition of a locator
    /// </summary>
    class Locator
    {
        public LocatorTypes LocatorType { get; set; }
        public string LocatorXPath { get; set; }


        /// <summary>
        /// Split the locator into a type and content
        /// </summary>
        /// <param name="Locator"></param>
        /// <param name="LocatorType"></param>
        /// <param name="LocatorContent"></param>
        public static Locator Parse(XmlElement Element, string Locator)
        {
            Locator = Locator.Trim();

            if (!Locator.Contains("("))
            {
                throw new ArgumentException($"Invalid Locator value '{Locator}', does not include an open parenthesis");
            }

            if (!Locator.EndsWith(")"))
            {
                throw new ArgumentException($"Invalid Locator value '{Locator}', does not end with a close parenthesis");
            }

            var ParenthesisLocation = Locator.IndexOf("(", StringComparison.OrdinalIgnoreCase);
            var LocatorContent = Locator.Substring(ParenthesisLocation + 1, Locator.Length - ParenthesisLocation - 2).Trim();

            LocatorTypes LocatorType;
            string LocatorXPath;
            switch (Locator.Substring(0, ParenthesisLocation).Trim().ToUpperInvariant())
            {
                case "MATCH":
                    LocatorType = LocatorTypes.Match;
                    var AttribtuesToMatch = LocatorContent.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (AttribtuesToMatch.Length == 0)
                    {
                        throw new ArgumentException(
                            $"Invalid Locator value '{Element.Attributes["Locator", "http://schemas.microsoft.com/XML-Document-Transform"].Value}', does not specify any attribute names");
                    }
                    LocatorXPath = MatchLocatorXPath(Element, AttribtuesToMatch);
                    break;

                case "CONDITION":
                    LocatorType = LocatorTypes.Condition;
                    if (string.IsNullOrEmpty(LocatorContent))
                    {
                        throw new ArgumentException(
                            $"Invalid Locator value '{Element.Attributes["Locator", "http://schemas.microsoft.com/XML-Document-Transform"].Value}', does not specify a condition");
                    }
                    LocatorXPath = $"/{Element.Name}[{LocatorContent}]";
                    break;

                case "XPATH":
                    LocatorType = LocatorTypes.XPath;
                    if (string.IsNullOrEmpty(LocatorContent))
                    {
                        throw new ArgumentException(
                            $"Invalid Locator value '{Element.Attributes["Locator", "http://schemas.microsoft.com/XML-Document-Transform"].Value}', does not specify an xpath");
                    }
                    LocatorXPath = LocatorContent;
                    break;

                default:
                    throw new ArgumentException(
                        $"Invalid Locator value '{Element.Attributes["Locator", "http://schemas.microsoft.com/XML-Document-Transform"].Value}', must be Match, Condition or Xpath");
            }

            return new Locator() { LocatorType = LocatorType, LocatorXPath = LocatorXPath };
        }


        /// <summary>
        /// Build an XPath expression that matches the specified transform element's 
        /// specified attributes.
        /// </summary>
        /// <param name="Element">The transform element who's attributes are to be matched</param>
        /// <param name="AttribtuesToMatch">The list of attribute names to match</param>
        /// <returns>An XPath expression, [att='value'], to do the actual matching</returns>
        private static string MatchLocatorXPath(XmlElement Element, string[] AttribtuesToMatch)
        {
            //
            // Start with the XPath for the element
            //
            var XPathBuilder = new StringBuilder("/" + Element.Name);

            //
            // Add a predicate that matches the specific attributes
            //
            XPathBuilder.Append("[");
            var First = true;
            foreach (var AttributeName in AttribtuesToMatch)
            {
                //
                // Add the logical operand between the previous attribute predicate and this predicate
                //
                if (!First)
                {
                    XPathBuilder.Append(" and ");
                }

                //
                // Ensure the attribute to match exists on the Locator element
                //
                if (Element.Attributes[AttributeName] == null)
                {
                    throw new ArgumentException($"Match Locator specified a nonexistant attribute: '{AttributeName}'");
                }

                //
                // Add the predicate
                //
                XPathBuilder.AppendFormat("@{0}='{1}'", AttributeName, Element.Attributes[AttributeName].Value);

                //
                // Note that we now need to add logical operands
                //
                First = false;
            }

            //
            // Finish the XPath
            //
            XPathBuilder.Append("]");

            //
            // Return the complete XPath
            //
            return XPathBuilder.ToString();
        }

    }
}
