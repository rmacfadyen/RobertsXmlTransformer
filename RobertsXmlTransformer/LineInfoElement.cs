using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace RobertsXmlTransformer
{
    /// <summary>
    /// Implementation of XmlElement that implements IXmlLineInfo
    /// </summary>    
    class LineInfoElement : XmlElement, IXmlLineInfo
    {
        public int LineNumber { get; }
        public int LinePosition { get; }


        internal LineInfoElement(string prefix, string localname, string nsURI, XmlDocument doc, IXmlLineInfo LineInfo)
            : base(prefix, localname, nsURI, doc)
        {
            if (LineInfo != null)
            {
                LineNumber = LineInfo.LineNumber;
                LinePosition = LineInfo.LinePosition;
            }
        }


        public bool HasLineInfo()
        {
            return true;
        }
    }
}
