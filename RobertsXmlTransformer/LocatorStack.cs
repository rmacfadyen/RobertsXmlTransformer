using System;
using System.Collections.Generic;
using System.Text;

namespace RobertsXmlTransformer
{
    class LocatorStack
    {
        private readonly List<Locator> _Stack = new List<Locator>();

        public void Push(Locator item)
        {
            _Stack.Add(item);
        }

        public void Pop()
        {
            _Stack.RemoveAt(_Stack.Count - 1);
        }

        public bool IsEmpty()
        {
            return _Stack.Count == 0;
        }

        /// <summary>
        /// Get the currently active locator. If there isn't one explicitly set 
        /// for the current transform element walk the stack of known locators.
        /// </summary>
        /// <param name="Element">The transfrom element that needs a locator</param>
        /// <returns>The full and complete XPath to select the appropriate element(s) 
        /// in the target document</returns>
        public string GetCurrentLocatorXPath()
        {
            StringBuilder ThisLocatorXPath;

            //
            // Check upwards through the stack for an XPath locator
            //
            for (var i = _Stack.Count - 1; i >= 0; i -= 1)
            {
                var ThisLocator = _Stack[i];

                if (ThisLocator.LocatorType == LocatorTypes.XPath)
                {
                    ThisLocatorXPath = new StringBuilder(ThisLocator.LocatorXPath);
                    for (var j = i + 1; j < _Stack.Count; j += 1)
                    {
                        ThisLocatorXPath.Append(_Stack[j].LocatorXPath);
                    }

                    return ThisLocatorXPath.ToString();
                }
            }

            //
            // No XPath locator so the stack can be handled as a simple concatenation
            //
            ThisLocatorXPath = new StringBuilder();
            foreach (var Locator in _Stack)
            {
                if (!string.IsNullOrEmpty(Locator.LocatorXPath))
                {
                    ThisLocatorXPath.Append(Locator.LocatorXPath);
                }
            }

            return ThisLocatorXPath.ToString();
        }
    }
}
