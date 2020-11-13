using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace RobertsXmlTransformer
{
    /// <summary>
    /// Implementation of XmlDocument that implements IXmlLineInfo for elements. Used
    /// so error reporting can specify what line of a transform caused a problem. This
    /// is needed because the stock XmlDocument implementation doesn't track line
    /// numbers (which is really annoying when trying to generate a nice error message).
    /// </summary>
    class LineInfoXmlDocument : XmlDocument
    {
        private XmlReader reader;

        public override void Load(XmlReader xml)
        {
            reader = xml;
            base.Load(reader);
        }

        public override XmlElement CreateElement(string prefix, string localname, string namespaceuri)
        {
            return new LineInfoElement(prefix, localname, namespaceuri, this, reader as IXmlLineInfo);
        }
    }
}
