using System;
using System.Collections.Generic;
using System.Text;

namespace RobertsXmlTransformer
{
    /// <summary>
    /// The types of locators. An Implied locator is based on matching
    /// element names. A Match locator is based on matching the specified
    /// element attributes. A Condition locator is based on the supplied
    /// XPath condition predicate. An XPath location is based on the 
    /// supplied XPath.
    /// </summary>
    enum LocatorTypes
    {
        Implied,
        Match,
        Condition,
        XPath
    }

}
