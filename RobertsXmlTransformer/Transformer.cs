using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.XPath;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
// ReSharper disable PossibleNullReferenceException


namespace RobertsXmlTransformer
{
    /// <summary>
    /// 
    /// </summary>
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public class Transformer
    {
        private XmlDocument _TargetDocument;
        private XmlElement _CurrentContext;
        private readonly LocatorStack _LocatorXPathStack = new LocatorStack();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TargetDocument"></param>
        /// <param name="XmlTransform"></param>
        public void ApplyTransform(XmlDocument TargetDocument, string XmlTransform)
        {
            //
            // Save the target document
            //
            _TargetDocument = TargetDocument;
            _TargetDocument.PreserveWhitespace = true;

            //
            // Load the transform
            //
            var TransformDocument = new LineInfoXmlDocument {PreserveWhitespace = true};
            TransformDocument.LoadXml(XmlTransform);

            //
            // Handle the top level element
            //
            ProcessElement(TransformDocument.DocumentElement);
        }


        /// <summary>
        /// Handle a transform document element.
        /// </summary>
        /// <param name="Element">The element to start with. All descendants of this element may be
        /// visited (depends on whether an xdt:transform is found)</param>
        private void ProcessElement(XmlElement Element)
        {
            //
            // Note the current context (for error messages)
            //
            _CurrentContext = Element;

            //
            // Parse the transform instruction (if any)
            //
            Transform CurrentTransform = null;

            if (Element.Attributes["Transform", "http://schemas.microsoft.com/XML-Document-Transform"] != null)
            {
                //
                // Fishout the transform details
                //
                try
                {
                    CurrentTransform = Transform.Parse(Element.Attributes["Transform", "http://schemas.microsoft.com/XML-Document-Transform"].Value);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException(FormatExceptionMessage(e.Message));
                }
            }

            //
            // Figure out the locator for this transform element 
            //
            Locator ThisLocator;

            if (CurrentTransform != null
                && (CurrentTransform.TransformType == TransformTypes.InsertAfter || CurrentTransform.TransformType == TransformTypes.InsertBefore))
            {
                ThisLocator = new Locator() { LocatorType = LocatorTypes.XPath, LocatorXPath = CurrentTransform.TransformContent };
            }
            else
            {
                if (Element.Attributes["Locator", "http://schemas.microsoft.com/XML-Document-Transform"] != null)
                {
                    //
                    // Parse the locator
                    //
                    try
                    {
                        ThisLocator = Locator.Parse(Element, Element.Attributes["Locator", "http://schemas.microsoft.com/XML-Document-Transform"].Value);
                    }
                    catch (ArgumentException e)
                    {
                        throw new ArgumentException(FormatExceptionMessage(e.Message));
                    }
                }
                else
                {
                    if (CurrentTransform == null)
                    {
                        //
                        // A transform wasn't specified so just add a an implied locator for this element name
                        //
                        ThisLocator = new Locator() { LocatorType = LocatorTypes.Implied, LocatorXPath = "/" + Element.Name };
                    }
                    else
                    {
                        //
                        // A transform without an immediate locator doesn't 
                        //
                        if (CurrentTransform.TransformType != TransformTypes.Insert)
                        {
                            ThisLocator = new Locator() { LocatorType = LocatorTypes.Implied, LocatorXPath = "/" + Element.Name };
                        }
                        else
                        {
                            ThisLocator = new Locator() { LocatorType = LocatorTypes.Implied, LocatorXPath = "" };
                        }
                    }
                }
            }

            //
            // If this is the first locator and it's xpath isn't "rooted" make sure it is
            //
            if (_LocatorXPathStack.IsEmpty() && !ThisLocator.LocatorXPath.StartsWith("//"))
            {
                ThisLocator.LocatorXPath = "/" + ThisLocator.LocatorXPath;
            }

            //
            // Add the locator to the stack of locators
            //
            _LocatorXPathStack.Push(ThisLocator);

            //
            // If this transform elment actually includes a transform then perform it
            //
            var ProcessChildElements = true;
            if (CurrentTransform != null)
            {
                //
                // Get the locator XPath to use with this transform
                //
                var CurrentXPath = _LocatorXPathStack.GetCurrentLocatorXPath();

                //
                // Apply the transform
                //
                try
                {
                    ProcessChildElements = ProcessTransform(Element, CurrentTransform, CurrentXPath);
                }
                catch (XPathException e)
                {
                    throw new ArgumentException(FormatExceptionMessage("The Locator XPath '{0}' for a Transform threw an exception ({1})", CurrentXPath, e.Message), e);
                }
            }

            //
            // If a transform was done that moved the transform element's children into the 
            // target doc then we don't need to look at any child elements.
            //
            if (ProcessChildElements)
            {
                foreach (XmlNode ChildNode in Element.ChildNodes)
                {
                    if (ChildNode.NodeType == XmlNodeType.Element)
                    {
                        ProcessElement((XmlElement)ChildNode);
                    }
                }
            }

            //
            // Remove the locator stack entry
            //
            _LocatorXPathStack.Pop();
        }


        /// <summary>
        /// Perform the specified transformation.
        /// </summary>
        /// <param name="Element">The current element from the transformation document. 
        /// For example &lt;something xdt:transform="insert"&gt;&lt;/something&gt; the
        /// element is &lt;something&gt; and it will be inserted into target document
        /// at the location determined by the CurrentXPath</param>
        /// <param name="Transform">The transformation to be performed</param>
        /// <param name="CurrentXPath">The XPath expression to select the elements the transform will be applied to</param>
        /// <returns>Return whether or not the child elements of this transform should be 
        /// processed for additional transforms/locators.</returns>
        private bool ProcessTransform(XmlElement Element, Transform Transform, string CurrentXPath)
        {
            XmlElement TargetElement;
            XmlElement SourceElement;

            switch (Transform.TransformType)
            {
                //
                // Remove and RemoveAll operates on a all matching elements for the current locator,
                // with Remove only removing the first element (remove all removes all)
                //
                case TransformTypes.RemoveAll:
                case TransformTypes.Remove:
                    foreach (XmlNode NodeToRemove in _TargetDocument.SelectNodes(CurrentXPath))
                    {
                        NodeToRemove.ParentNode.RemoveChild(NodeToRemove);

                        //
                        // RemoveAll vs Remove... remove only removes a single element
                        //
                        if (Transform.TransformType == TransformTypes.Remove)
                        {
                            break;
                        }
                    }

                    return false;

                case TransformTypes.Insert:
                    //
                    // Get the parent where the AppendChild will occur
                    //
                    TargetElement = _TargetDocument.SelectSingleNode(CurrentXPath) as XmlElement;
                    if (TargetElement == null)
                    {
                        throw new ArgumentException(FormatExceptionMessage(
                            "The Locator XPath '{0}' did not match an element in the target document, cannot apply Transform {1}",
                            CurrentXPath, Transform.TransformType));
                    }

                    //
                    // Do the insert
                    //
                    SourceElement = CloneElementAsNewTargetDocumentElement(Element);
                    InsertElement(SourceElement, TargetElement, InsertionType.Append);

                    return false;


                case TransformTypes.Replace:
                    //
                    // Get the target
                    //
                    TargetElement = GetTargetElement(CurrentXPath);

                    SourceElement = CloneElementAsNewTargetDocumentElement(Element);
                    if (TargetElement.ParentNode.NodeType == XmlNodeType.Document)
                    {
                        var Declaration = _TargetDocument.FirstChild as XmlDeclaration;
                        _TargetDocument.RemoveAll();
                        if (Declaration != null)
                        {
                            _TargetDocument.AppendChild(
                                _TargetDocument.CreateXmlDeclaration(Declaration.Version, Declaration.Encoding,
                                    Declaration.Standalone));
                        }

                        _TargetDocument.AppendChild(SourceElement);
                    }
                    else
                    {
                        TargetElement.ParentNode.InsertAfter(SourceElement, TargetElement);
                        TargetElement.ParentNode.RemoveChild(TargetElement);
                    }

                    return false;

                case TransformTypes.Comment:
                    //
                    // Get the target
                    //
                    TargetElement = GetTargetElement(CurrentXPath);

                    var Comment = _TargetDocument.CreateNode(XmlNodeType.Comment, null, null);
                    Comment.InnerText = Transform.TransformContent.Replace("\\r", "\r").Replace("\\n", "\n");
                    InsertElement(Comment, TargetElement, InsertionType.Before);
                    
                    return true;

                case TransformTypes.RemovePreviousComment:
                    //
                    // Get the target
                    //
                    TargetElement = GetTargetElement(CurrentXPath, true);
                    if (TargetElement != null)
                    {
                        var PreviousComment = GetComment(TargetElement, CommentDirection.Previous);
                        if (PreviousComment != null)
                        {
                            if (PreviousComment.PreviousSibling != null &&
                                NodeIsWhitespace(PreviousComment.PreviousSibling))
                            {
                                TargetElement.ParentNode.RemoveChild(PreviousComment.PreviousSibling);
                            }

                            if (NodeIsWhitespace(PreviousComment.NextSibling))
                            {
                                TargetElement.ParentNode.RemoveChild(PreviousComment.NextSibling);
                            }

                            TargetElement.ParentNode.RemoveChild(PreviousComment);
                        }
                    }

                    return true;

                case TransformTypes.RemoveFollowingComment:
                    //
                    // Get the target
                    //
                    TargetElement = GetTargetElement(CurrentXPath, true);
                    if (TargetElement != null)
                    {
                        var FollowingComment = GetComment(TargetElement, CommentDirection.Following);
                        if (FollowingComment != null)
                        {
                            if (NodeIsWhitespace(FollowingComment.PreviousSibling))
                            {
                                TargetElement.ParentNode.RemoveChild(FollowingComment.PreviousSibling);
                            }

                            if (FollowingComment.NextSibling != null &&
                                NodeIsWhitespace(FollowingComment.NextSibling))
                            {
                                TargetElement.ParentNode.RemoveChild(FollowingComment.NextSibling);
                            }

                            TargetElement.ParentNode.RemoveChild(FollowingComment);
                        }
                    }

                    return true;

                case TransformTypes.InsertBefore:
                    //
                    // Get the target
                    //
                    TargetElement = GetTargetElement(CurrentXPath);

                    SourceElement = CloneElementAsNewTargetDocumentElement(Element);
                    InsertElement(SourceElement, TargetElement, InsertionType.Before);
                    
                    return true;

                case TransformTypes.InsertAfter:
                    //
                    // Get the target
                    //
                    TargetElement = GetTargetElement(CurrentXPath);

                    SourceElement = CloneElementAsNewTargetDocumentElement(Element);
                    InsertElement(SourceElement, TargetElement, InsertionType.After);
                    
                    return true;

                case TransformTypes.RemoveAttributes:
                    //
                    // Get the target
                    //
                    TargetElement = GetTargetElement(CurrentXPath);

                    foreach (var AttributeName in Transform.TransformAttributes)
                    {
                        if (TargetElement.Attributes[AttributeName] != null)
                        {
                            TargetElement.Attributes.Remove(TargetElement.Attributes[AttributeName]);
                        }
                    }

                    return true;

                case TransformTypes.SetAttributes:
                default:
                    //
                    // Get the target
                    //
                    TargetElement = GetTargetElement(CurrentXPath);

                    //
                    // If a list of attribute names was not provide copy all attributes present
                    //  - Except for any xdt:Transform or xdt:Locator attributes
                    //
                    if (Transform.TransformAttributes == null || Transform.TransformAttributes.Count == 0)
                    {
                        foreach (XmlAttribute Attribute in Element.Attributes)
                        {
                            if (!(Attribute.NamespaceURI ==
                                  "http://schemas.microsoft.com/XML-Document-Transform"
                                  && (string.Compare(Attribute.LocalName, "Transform",
                                          StringComparison.OrdinalIgnoreCase) == 0
                                      || string.Compare(Attribute.LocalName, "Locator",
                                          StringComparison.OrdinalIgnoreCase) == 0)))
                            {
                                SetAttribute(TargetElement, Attribute.Name, Attribute.Value);
                            }
                        }
                    }
                    else
                    {
                        //
                        // Copy the specified attrbutes
                        //
                        foreach (var AttributeName in Transform.TransformAttributes)
                        {
                            if (Element.Attributes[AttributeName] == null)
                            {
                                throw new ArgumentException(FormatExceptionMessage(
                                    "Transform SetAtttibutes specifies a nonexistant attribute: '{0}'",
                                    AttributeName));
                            }

                            SetAttribute(TargetElement, AttributeName, Element.Attributes[AttributeName].Value);
                        }
                    }

                    return true;
            }
        }

        private XmlElement GetTargetElement(string CurrentXPath, bool Optional = false)
        {
            //
            // If an element was not found we're done
            // 
            if (!(_TargetDocument.SelectSingleNode(CurrentXPath) is XmlElement TargetElement))
            {
                if (!Optional)
                {
                    throw new ArgumentException(
                        FormatExceptionMessage(
                            "The Locator XPath '{0}' did not match an element in the target document, cannot apply transform",
                            CurrentXPath
                        )
                    );
                }

                return null;
            }

            return TargetElement;
        }


        /// <summary>
        /// Check if a node is either Whitespace or SignificantWhitespace. When a transform
        /// adds whitespace it is added as SignificantWhitespace... which should be treated
        /// the same as regular whitespace by later transforms.
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        private bool NodeIsWhitespace(XmlNode Node)
        {
            return (Node.NodeType == XmlNodeType.Whitespace || Node.NodeType == XmlNodeType.SignificantWhitespace);
        }


        /// <summary>
        /// The type of insertion to perform
        /// </summary>
        enum InsertionType
        {
            Before,
            After,
            Append
        }


        /// <summary>
        /// Insert an element with appropriate whitespace
        /// </summary>
        /// <param name="ElementToInsert"></param>
        /// <param name="InsertionPoint"></param>
        /// <param name="InsertionType"></param>
        private void InsertElement(XmlNode ElementToInsert, XmlElement InsertionPoint, InsertionType InsertionType)
        {
            XmlNode Whitespace = null;
            XmlNode Leading = null;
            XmlNode Trailing = null;

            //
            // Determine what whitespace is required
            //  - Must be done before the actual insertion
            //  - Append is handled slightly differently... it needs both a leading
            //    and trailing whitespace.
            //
            if (InsertionType != InsertionType.Append)
            {
                //
                //  Whatever whitespace exists prior to the insertion point should be
                //  duplicated after the new element is add.
                //
                if (InsertionPoint.PreviousSibling != null && NodeIsWhitespace(InsertionPoint.PreviousSibling))
                {
                    Whitespace = _TargetDocument.CreateSignificantWhitespace(GetWhitespaceBetweenElements(InsertionPoint));
                }
            }
            else
            {
                //
                //  Whatever whitespace exists prior to the insertion point should be added
                //  without the linebreak before the element, and added with the linebreak 
                //  after the element.
                //
                if (InsertionPoint.ParentNode.PreviousSibling != null && NodeIsWhitespace(InsertionPoint.ParentNode.PreviousSibling))
                {
                    Leading = _TargetDocument.CreateSignificantWhitespace(GetWhitespaceBetweenElements(InsertionPoint.ParentNode).Replace("\r\n", string.Empty) + "  ");
                    Trailing = _TargetDocument.CreateSignificantWhitespace(GetWhitespaceBetweenElements(InsertionPoint.ParentNode) + "  ");
                }
            }

            //
            // Validate the insertion won't fail because the insertion point's parent is teh document object
            //
            if (InsertionType == InsertionType.Before || InsertionType == InsertionType.After)
            {
                if (InsertionPoint.ParentNode.NodeType == XmlNodeType.Document)
                {
                    throw new ArgumentException(FormatExceptionMessage("Cannot apply transform to root element"));
                }
            }

            //
            // Do the insertion
            //
            switch (InsertionType)
            {
                case InsertionType.Before:
                    InsertionPoint.ParentNode.InsertBefore(ElementToInsert, InsertionPoint);
                    break;
                case InsertionType.After:
                    InsertionPoint.ParentNode.InsertAfter(ElementToInsert, InsertionPoint);
                    break;
                case InsertionType.Append:
                    InsertionPoint.AppendChild(ElementToInsert);
                    break;
            }

            //
            // Ensure whitespace is handled such that the insertion is indented to correct level
            //
            if (InsertionType != InsertionType.Append)
            {
                //
                //  Whatever whitespace exists prior to the insertion point should be
                //  duplicated after the new element is add.
                //
                if (Whitespace != null)
                {
                    if (InsertionType == InsertionType.Before)
                    {
                        InsertionPoint.ParentNode.InsertAfter(Whitespace, ElementToInsert);
                    }
                    else
                    {
                        InsertionPoint.ParentNode.InsertBefore(Whitespace, ElementToInsert);
                    }
                }
            }
            else
            {
                //
                // Appends may require either leading or trailing whitespace (or both)
                //
                if (Leading != null)
                {
                    InsertionPoint.InsertBefore(Leading, ElementToInsert);
                }
                if (Trailing != null)
                {
                    InsertionPoint.InsertAfter(Trailing, ElementToInsert);
                }
            }
        }


        /// <summary>
        /// Which direction to look for a comment
        /// </summary>
        enum CommentDirection
        {
            Previous,
            Following
        }

        /// <summary>
        /// Locate the previous comment ignoring any whitespace
        /// </summary>
        /// <param name="Element"></param>
        /// <returns></returns>
        private XmlComment GetComment(XmlElement Element, CommentDirection Direction)
        {
            XmlNode Node = Element;

            //
            // Move to the first non-whitespace node
            //
            do
            {
                Node = (Direction == CommentDirection.Previous ? Node.PreviousSibling : Node.NextSibling);
            }
            while (Node != null && NodeIsWhitespace(Node));


            //
            // Only return the node if it is a comment
            //
            if (Node != null && Node.NodeType == XmlNodeType.Comment)
            {
                return Node as XmlComment;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Get the whitespace between the specified element and its previous sibling
        /// </summary>
        /// <param name="Element"></param>
        /// <returns></returns>
        private string GetWhitespaceBetweenElements(XmlNode Node)
        {
            var Whitespace = string.Empty;
            while (Node.PreviousSibling != null && NodeIsWhitespace(Node.PreviousSibling))
            {
                Whitespace = Node.PreviousSibling.InnerText + Whitespace;

                Node = Node.PreviousSibling;
            }

            return Whitespace;
        }


        /// <summary>
        /// Set an attribute on the target element
        /// </summary>
        /// <param name="TargetElement"></param>
        /// <param name="AttributeName"></param>
        /// <param name="AttributeValue"></param>
        private void SetAttribute(XmlElement TargetElement, string AttributeName, string AttributeValue)
        {
            if (TargetElement.Attributes[AttributeName] != null)
            {
                TargetElement.Attributes[AttributeName].Value = AttributeValue;
            }
            else
            {
                var NewAttribute = _TargetDocument.CreateAttribute(AttributeName);
                NewAttribute.Value = AttributeValue;
                TargetElement.Attributes.Append(NewAttribute);
            }
        }



        /// <summary>
        /// Copy a transform element into the target as a new element. Do not copy
        /// the xdt:Transform/xdt:Locator attributes or a xmlns:xdt declaration
        /// </summary>
        /// <param name="Element">The element to be copied</param>
        /// <returns>A new element ready for insertion into the target XmlDocument</returns>
        private XmlElement CloneElementAsNewTargetDocumentElement(XmlElement Element)
        {
            //
            // Make a copy
            //
            var Clone = _TargetDocument.ImportNode(Element, true) as XmlElement;

            //
            // Strip out any xdt attributes
            //
            RemoveXdtAttributes(Clone);
            
            //
            // Return the new element ready for insertion into the target document
            //
            return Clone;
        }


        /// <summary>
        /// Ensure an element and its children do not have any xdt attributes
        /// </summary>
        /// <param name="Element">The element to remove any xtd attributes from. The child elements are also processed.</param>
        private void RemoveXdtAttributes(XmlElement Element)
        {
            //
            // Remove the transform attribute if present
            //
            if (Element.Attributes["Transform", "http://schemas.microsoft.com/XML-Document-Transform"] != null)
            {
                Element.Attributes.RemoveNamedItem("Transform", "http://schemas.microsoft.com/XML-Document-Transform");
            }

            //
            // Remove the locator attribute if present
            //
            if (Element.Attributes["Locator", "http://schemas.microsoft.com/XML-Document-Transform"] != null)
            {
                Element.Attributes.RemoveNamedItem("Locator", "http://schemas.microsoft.com/XML-Document-Transform");
            }

            //
            // Ensure any namespace declaration for xdt is removed (note that
            // the name of the actual namespace is not checked... in general
            // it ought to be xdt... but could in fact be anything the transform
            // document author decided).
            //
            foreach (XmlAttribute Attribute in Element.Attributes)
            {
                if (Attribute.Prefix == "xmlns" && Attribute.Value == "http://schemas.microsoft.com/XML-Document-Transform")
                {
                    Element.Attributes.Remove(Attribute);
                    break;
                }
            }

            //
            // Do all child elements
            //
            foreach (XmlNode Child in Element.ChildNodes)
            {
                if (Child.NodeType == XmlNodeType.Element)
                {
                    RemoveXdtAttributes((XmlElement)Child);
                }
            }
        }


        /// <summary>
        /// Format an exception message and include the current context location
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="Values"></param>
        /// <returns></returns>
        private string FormatExceptionMessage(string Message, params object[] Values)
        {
            //
            // Add the current line/column details if they are available
            //
            var FileLocation = string.Empty;
            if (_CurrentContext is IXmlLineInfo info)
            {
                if (info.HasLineInfo())
                {
                    FileLocation =
                        $"line {info.LineNumber}, column {info.LinePosition}";
                }
            }

            //
            // Get the context element as a nice clean string (no inner
            // content, no namespace declaration)
            //
            var ThisElement = _CurrentContext.OuterXml;
            if (!string.IsNullOrEmpty(_CurrentContext.InnerXml))
            {
                ThisElement = ThisElement.Replace(_CurrentContext.InnerXml, string.Empty);
            }

            var XmlNamespace = new Regex(@"\s+xmlns:(.*?)=""http://schemas\.microsoft\.com/XML-Document-Transform""");
            ThisElement = XmlNamespace.Replace(ThisElement, string.Empty);
            ThisElement = ThisElement.Replace($"</{_CurrentContext.Name}>", string.Empty);

            //
            // Return the nicely formatted message
            //
            return
                string.Format(Message, Values)
                +
                $" on element {ThisElement} ({FileLocation})";
        }
    }
}
