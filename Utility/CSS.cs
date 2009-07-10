using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.IO;
using System.Web;

namespace JawTek.Web.Utility.CSS
{
    /// <summary>
    /// Utility class with static methods to help with CSS.
    /// </summary>
    public static class CSSUtility
    {
        /// <summary>
        /// Generates CSSRuleCollection containing Eric Meyer's CSS Reset 
        /// found at http://meyerweb.com/eric/thoughts/2007/05/01/reset-reloaded/
        /// </summary>
        /// <returns>CSSRuleCollection containing CSS Reset</returns>
        public static CSSRuleCollection GetCSSReset()
        {
            CSSRuleCollection c = new CSSRuleCollection();
            CSSRule main = new CSSRule();
            main.Selector = "html, body, div, span, applet, object, iframe, " +
                "h1, h2, h3, h4, h5, h6, p, blockquote, pre, " +
                "a, abbr, acronym, address, big, cite, code, " +
                "del, dfn, em, font, img, ins, kbd, q, s, samp, " +
                "small, strike, strong, sub, sup, tt, var, " +
                "dl, dt, dd, ol, ul, li, " +
                "fieldset, form, label, legend, " +
                "table, caption, tbody, tfoot, thead, tr, th, td";
            main.Properties.Add("margin", "0")
                .Add("padding", "0")
                .Add("border", "0")
                .Add("outline", "0")
                .Add("font-weight","inherit")
                .Add("font-style","inherit")
                .Add("font-family", "inherit")
                .Add("font-size", "100%")
                .Add("vertical-align", "baseline")
                .Add("background", "transparent");
            CSSRule focus = new CSSRule(":focus");
            focus.Properties.Add("outline", "0");
            CSSRule body = new CSSRule("body");
            body.Properties.Add("line-height", "1")
                .Add("color", "black")
                .Add("background", "white");
            CSSRule list = new CSSRule("ol, ul");
            list.Properties.Add("list-style", "none");
            CSSRule table = new CSSRule("table");
            table.Properties.Add("border-collapse", "separate")
                .Add("border-spacing", "0");
            CSSRule caption = new CSSRule("caption, th, td");
            caption.Properties.Add("text-align", "left")
                .Add("font-weight", "normal");
            CSSRule bqBefore = new CSSRule("blockquote:before, blockquote:after, " +
                "q:before, q:after");
            bqBefore.Properties.Add("content", "''");
            CSSRule blockqoute = new CSSRule("blockquote, q");
            blockqoute.Properties.Add("quotes", "'' ''");
            c.Add(main)
                .Add(focus)
                .Add(body)
                .Add(list)
                .Add(table)
                .Add(caption)
                .Add(bqBefore)
                .Add(blockqoute);
            return c;
        }
    }

    /// <summary>
    /// Represents a single CSS rule consisting 
    /// of a selector and properties.
    /// </summary>
    public class CSSRule
    {
        private string selector = "";
        /// <summary>
        /// The CSS selector to match on.
        /// </summary>
        public string Selector
        {
            get { return selector; }
            set { selector = value; }
        }
        private CSSPropertyCollection properties = new CSSPropertyCollection();
        /// <summary>
        /// CSS Properties to apply to matched HTML Element. 
        /// Represented as a <see cref="CSSPropertyCollection">CSSPropertyCollection</see> object.
        /// </summary>
        public CSSPropertyCollection Properties
        {
            get { return this.properties; }
            set { this.properties = value; }
        }
        /// <summary>
        /// Creates a blank CSSRule object.
        /// </summary>
        public CSSRule() { }
        /// <summary>
        /// Creates a CSSRule object providing only the 
        /// CSS selector.
        /// </summary>
        /// <param name="selector">CSS selector to match on.</param>
        public CSSRule(string selector)
        {
            this.selector = selector;
        }
        /// <summary>
        /// Create a CSSRule object providing a 
        /// CSS selector and a collection 
        /// of properties.
        /// </summary>
        /// <param name="selector">CSS selector to match on.</param>
        /// <param name="properties">Collection of CSS properties 
        /// to apply to matched HTML Element. 
        /// Represented as a <see cref="CSSPropertyCollection">CSSPropertyCollection</see> object.</param>
        public CSSRule(string selector, CSSPropertyCollection properties)
            : this(selector)
        {
            if (properties != null)
            {
                this.properties = properties;
            }
        }
        /// <summary>
        /// Gets the string representation of the CSSRule object.
        /// </summary>
        /// <returns>String representation of the CSSRule object.</returns>
        public override string ToString()
        {
            return this.ToString(0);
        }

        /// <summary>
        /// Gets the string representation of the CSSRule object
        /// indenting it to <paramref name="indentLevel"/> places.
        /// </summary>
        /// <param name="indentLevel">Number of places to indent to.</param>
        /// <returns>String representation of the CSSRule object.</returns>
        public string ToString(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; i++)
            {
                indent += HTML.HTMLUtility.IndentString;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("{0}{1} ", new object[] { indent, this.Selector }) + "{\n");
            sb.Append(this.Properties.ToString(indentLevel + 1));
            sb.Append(String.Format("{0}", indent) + "}\n");
            return sb.ToString();
        }
    }

    /// <summary>
    /// A collection of <see cref="CSSRule">CSSRule</see> objects.
    /// </summary>
    public class CSSRuleCollection : IChainableCollection<CSSRule>
    {
        private List<CSSRule> collection = new List<CSSRule>();

        /// <summary>
        /// Retrieves <see cref="CSSRule">CSSRule</see> object from collection by CSS Selector.
        /// </summary>
        /// <param name="selector">CSS Selector to look for in collection.</param>
        /// <returns>A <see cref="CSSRule"/> object whose CSS Selector matches <paramref name="selector"/>.</returns>
        public CSSRule this[string selector]
        {
            get
            {
                return this.collection.Where(c => c.Selector == selector).First();
            }
            set
            {
                CSSRule c = this[selector];
                this.Remove(c);
                this.Add(value);
            }
        }

        #region IChainableCollection<CSSRule> Members

        /// <summary>
        /// Adds <see cref="CSSRule"/> object to collection.
        /// </summary>
        /// <param name="item"><see cref="CSSRule"/> object to add to collection.</param>
        /// <returns>Reference to collection.</returns>
        public IChainableCollection<CSSRule> Add(CSSRule item)
        {
            if (this.ContainsSelector(item.Selector))
            {
                CSSRule c = this[item.Selector];
                foreach (CSSProperty i in item.Properties)
                {
                    c.Properties.Add(i);
                }
            }
            else
            {
                this.collection.Add(item);
            }
            return this;
        }

        /// <summary>
        /// Clears all items from collection.
        /// </summary>
        /// <returns>Reference to collection.</returns>
        public IChainableCollection<CSSRule> Clear()
        {
            this.collection.Clear();
            return this;
        }

        /// <summary>
        /// Copies entire collection to a one-dimensional <see cref="CSSRule"/> array starting at <paramref name="arrayindex"/>.
        /// </summary>
        /// <param name="array">One-dimensional array of <see cref="CSSRule"/> that is the destination of the elements in the collection.</param>
        /// <param name="arrayindex">The zero-based index in array at which copying begins.</param>
        /// <returns>Reference to collection.</returns>
        public IChainableCollection<CSSRule> CopyTo(CSSRule[] array, int arrayindex)
        {
            this.collection.CopyTo(array, arrayindex);
            return this;
        }

        #endregion

        #region ICollection<CSSRule> Members

        /// <summary>
        /// Adds <see cref="CSSRule"/> object to collection.
        /// </summary>
        /// <param name="item"><see cref="CSSRule"/> object to add to collection.</param>
        /// <returns>Reference to collection.</returns>
        void ICollection<CSSRule>.Add(CSSRule item)
        {
            this.Add(item);
        }

        /// <summary>
        /// Clears all items from collection.
        /// </summary>
        /// <returns>Reference to collection.</returns>
        void ICollection<CSSRule>.Clear()
        {
            this.Clear();
        }

        /// <summary>
        /// Determines whether the collection contains the <paramref name="item"/>.
        /// </summary>
        /// <param name="item">Instance of <see cref="CSSRule"/> to locate in collection.</param>
        /// <returns>True if collection contains <paramref name="item"/>, False otherwise.</returns>
        public bool Contains(CSSRule item)
        {
            return this.collection.Contains(item);
        }
        /// <summary>
        /// Determines whether the collection contains a <see cref="CSSRule"/> 
        /// with a selector that matches <paramref name="selector"/>.
        /// </summary>
        /// <param name="selector">CSS Selector to look for in collection.</param>
        /// <returns>True if collection contains a <see cref="CSSRule"/> 
        /// with a selector that matches <paramref name="selector"/>, False otherwise.</returns>
        public bool ContainsSelector(string selector)
        {
            return this.collection.Any(c => c.Selector == selector);
        }

        /// <summary>
        /// Copies entire collection to a one-dimensional <see cref="CSSRule"/> array starting at <paramref name="arrayindex"/>.
        /// </summary>
        /// <param name="array">One-dimensional array of <see cref="CSSRule"/> that is the destination of the elements in the collection.</param>
        /// <param name="arrayindex">The zero-based index in array at which copying begins.</param>
        /// <returns>Reference to collection.</returns>
        void ICollection<CSSRule>.CopyTo(CSSRule[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns the number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return this.collection.Count; }
        }

        /// <summary>
        /// Returns True if collection is read only, False otherwise.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes first occurrence of <paramref name="item"/> from the collection.
        /// </summary>
        /// <param name="item"><see cref="CSSRule"/> to remove from collection</param>
        /// <returns>True if <paramref name="item"/> is removed, False otherwise.</returns>
        public bool Remove(CSSRule item)
        {
            return this.collection.Remove(item);
        }

        #endregion

        #region IEnumerable<CSSRule> Members

        /// <summary>
        /// Returns enumerator that iterates through the collection.
        /// </summary>
        /// <returns>Enumerator that iterates through the collection.</returns>
        public IEnumerator<CSSRule> GetEnumerator()
        {
            return ((ICollection<CSSRule>)this.collection).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns enumerator that iterates through the collection.
        /// </summary>
        /// <returns>Enumerator that iterates through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.collection).GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Gets the string representation of the collection.
        /// </summary>
        /// <returns>String representation of the collection.</returns>
        public override string ToString()
        {
            return this.ToString(0);
        }

        /// <summary>
        /// Gets the string representation of the collection
        /// indenting it to <paramref name="indentLevel"/> places.
        /// </summary>
        /// <param name="indentLevel">Number of places to indent to.</param>
        /// <returns>String representation of the collection.</returns>
        public string ToString(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; i++)
            {
                indent += HTML.HTMLUtility.IndentString;
            }
            StringBuilder sb = new StringBuilder();
            foreach (CSSRule i in this)
            {
                sb.Append(i.ToString(indentLevel));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets a MD5 hash of collection.
        /// </summary>
        /// <returns>MD5 hash of collection.</returns>
        public string GetMD5HashCode()
        {
            MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(this.ToString());
            bs = x.ComputeHash(bs);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bs)
            {
                sb.Append(b.ToString("x2").ToLower());
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Represents a single CSS property containing a 
    /// property name and a value.
    /// </summary>
    public class CSSProperty
    {
        private Regex urlUnit = new Regex("url\\(['\"]?(.*)['\"]?\\)");
        private string property = "";
        private string value = "";
        
        /// <summary>
        /// CSS property this <see cref="CSSProperty"/> contains the value for.
        /// </summary>
        public string Property
        {
            get { return this.property; }
            set { this.property = value; }
        }
        
        /// <summary>
        /// The value of the CSS property this <see cref="CSSProperty"/> represents.
        /// </summary>
        public string Value
        {
            get { return this.value; }
            set
            {
                string prop = value.ToLower();
                if (urlUnit.IsMatch(prop))
                {
                    Match m = urlUnit.Match(prop);
                    this.value = "url('" + HTML.HTMLUtility.ResolveURL(m.Groups[1].Value) + "')";
                }
                else
                {
                    this.value = value;
                }
            }
        }

        /// <summary>
        /// Creates an empty instance of the <see cref="CSSProperty"/> class.
        /// </summary>
        public CSSProperty() { }
        /// <summary>
        /// Creates an instance of <see cref="CSSProperty"/> representing the CSS property
        /// "<paramref name="property"/>" with a value of "<paramref name="value"/>".
        /// </summary>
        /// <param name="property">The CSS property this <see cref="CSSProperty"/> represents.</param>
        /// <param name="value">The value for this CSS <paramref name="property"/>.</param>
        public CSSProperty(string property, string value) { this.Property = property; this.Value = value; }

        /// <summary>
        /// Returns a string representation of this <see cref="CSSProperty"/>.
        /// </summary>
        /// <returns>String representation of this <see cref="CSSProperty"/>.</returns>
        public override string ToString()
        {
            return this.Property + ": " + this.Value + ";";
        }
    }

    /// <summary>
    /// Collection of <see cref="CSSProperty"/> objects.
    /// </summary>
    public class CSSPropertyCollection : IChainableCollection<CSSProperty>
    {
        private List<CSSProperty> collection = new List<CSSProperty>();

        /// <summary>
        /// Retrieves a <see cref="CSSProperty"/> from the collection with a
        /// property equal to "<paramref name="property"/>".
        /// </summary>
        /// <param name="property">The CSS property you want to retrieve from the collection.</param>
        /// <returns>The <see cref="CSSProperty"/> from the collection with a
        /// property equal to "<paramref name="property"/>".</returns>
        public CSSProperty this[string property]
        {
            get
            {
                if (this.Contains(property))
                {
                    return this.collection.First(p => p.Property == property);
                }
                else
                {
                    return null;
                }
            }
        }

        #region IChainableCollection<CSSProperty> Members

        /// <summary>
        /// Adds the <see cref="CSSProperty"/> "<paramref name="item"/>" to the collection.
        /// </summary>
        /// <param name="item"><see cref="CSSProperty"/> to add to the collection.</param>
        /// <returns>Reference to the collection.</returns>
        public IChainableCollection<CSSProperty> Add(CSSProperty item)
        {
            if (this.Contains(item.Property))
            {
                this[item.Property].Value = item.Value;
            }
            else
            {
                this.collection.Add(item);
            }
            return this;
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        /// <returns>Reference to the collection.</returns>
        public IChainableCollection<CSSProperty> Clear()
        {
            this.collection.Clear();
            return this;
        }

        /// <summary>
        /// Copies entire collection to a one-dimensional <see cref="CSSProperty"/> array starting at <paramref name="arrayindex"/>.
        /// </summary>
        /// <param name="array">One-dimensional array of <see cref="CSSProperty"/> that is the destination of the elements in the collection.</param>
        /// <param name="arrayindex">The zero-based index in array at which copying begins.</param>
        /// <returns>Reference to collection.</returns>
        public IChainableCollection<CSSProperty> CopyTo(CSSProperty[] array, int arrayindex)
        {
            this.collection.CopyTo(array, arrayindex);
            return this;
        }

        #endregion
        #region ICollection<CSSProperty> Members

        /// <summary>
        /// Adds the <see cref="CSSProperty"/> "<paramref name="item"/>" to the collection.
        /// </summary>
        /// <param name="item"><see cref="CSSProperty"/> to add to the collection.</param>
        void ICollection<CSSProperty>.Add(CSSProperty item)
        {
            this.collection.Add(item);
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        void ICollection<CSSProperty>.Clear()
        {
            this.Clear();
        }

        /// <summary>
        /// Determines if the collection contains <see cref="CSSProperty"/> "<paramref name="item"/>"
        /// </summary>
        /// <param name="item"><see cref="CSSProperty"/> to locate in the collection</param>
        /// <returns>True if "<paramref name="item"/>" is in the collection, False otherwise.</returns>
        public bool Contains(CSSProperty item)
        {
            return this.Contains(item);
        }

        /// <summary>
        /// Determines if the collections contains an instance of <see cref="CSSProperty"/>
        /// with the CSS property, "<paramref name="property"/>".
        /// </summary>
        /// <param name="property">The CSS Property you want to locate in the collection</param>
        /// <returns>True if an instance of <see cref="CSSProperty"/>
        /// with the CSS property, "<paramref name="property"/>", False otherwise.</returns>
        public bool Contains(string property)
        {
            return this.collection.Any(p => p.Property == property);
        }

        /// <summary>
        /// Copies entire collection to a one-dimensional <see cref="CSSProperty"/> array starting at <paramref name="arrayindex"/>.
        /// </summary>
        /// <param name="array">One-dimensional array of <see cref="CSSProperty"/> that is the destination of the elements in the collection.</param>
        /// <param name="arrayindex">The zero-based index in array at which copying begins.</param>
        void ICollection<CSSProperty>.CopyTo(CSSProperty[] array, int arrayIndex)
        {
            this.collection.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns the number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return this.collection.Count; }
        }

        /// <summary>
        /// Returns True if collection is read only, False otherwise.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurencs of <paramref name="item"/> from the collection.
        /// </summary>
        /// <param name="item"><see cref="CSSProperty"/> to remove from the collection.</param>
        /// <returns>True if <paramref name="item"/> is removed, False otherwise.</returns>
        public bool Remove(CSSProperty item)
        {
            return this.collection.Remove(item);
        }

        #endregion

        #region IEnumerable<CSSProperty> Members

        /// <summary>
        /// Returns enumerator that iterates through the collection.
        /// </summary>
        /// <returns>Enumerator that iterates through the collection.</returns>
        public IEnumerator<CSSProperty> GetEnumerator()
        {
            return ((ICollection<CSSProperty>)this.collection).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns enumerator that iterates through the collection.
        /// </summary>
        /// <returns>Enumerator that iterates through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.collection).GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Gets the string representation of the collection.
        /// </summary>
        /// <returns>String representation of the collection.</returns>
        public override string ToString()
        {
            return this.ToString(0);
        }

        /// <summary>
        /// Gets the string representation of the collection
        /// indenting it to <paramref name="indentLevel"/> places.
        /// </summary>
        /// <param name="indentLevel">Number of places to indent to.</param>
        /// <returns>String representation of the collection.</returns>
        public string ToString(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; i++)
            {
                indent += HTML.HTMLUtility.IndentString;
            }
            StringBuilder sb = new StringBuilder();
            foreach (CSSProperty i in this)
            {
                sb.AppendFormat("{0}{1}\n", new object[] { indent, i.ToString() });
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Class contianing the extension methods for CSS namespace.
    /// </summary>
    public static class CSSExtensions
    {
        /// <summary>
        /// Adds a new <see cref="CSSProperty"/> to the collection
        /// using <paramref name="property"/> and <paramref name="value"/>.
        /// </summary>
        /// <param name="me">The collection being added to.</param>
        /// <param name="property">The CSS property</param>
        /// <param name="value">The value of the CSS property</param>
        /// <returns>Reference to the collection</returns>
        public static IChainableCollection<CSSProperty> Add(this IChainableCollection<CSSProperty> me, string property, string value)
        {
            return me.Add(new CSSProperty(property, value));
        }

        /// <summary>
        /// Adds a new <see cref="CSSRule"/> to the collection
        /// using <paramref name="selector"/> and <paramref name="property"/>
        /// </summary>
        /// <param name="me">The collection being added to.</param>
        /// <param name="selector">The CSS selector for the <see cref="CSSRule"/></param>
        /// <param name="property">A <see cref="CSSProperty"/> for the rule.</param>
        /// <returns>Reference to the collection</returns>
        public static IChainableCollection<CSSRule> Add(this IChainableCollection<CSSRule> me, string selector, CSSProperty property)
        {
            CSSPropertyCollection p = new CSSPropertyCollection();
            p.Add(property);
            return me.Add(new CSSRule(selector, p));
        }

        /// <summary>
        /// Creates a <see cref="Style"/> HTML tag contianing the CSS rules.
        /// </summary>
        /// <param name="me">The collection to build the style from.</param>
        /// <returns>A <see cref="Style"/> HTML tag contianing the CSS rules.</returns>
        public static HTML.Style CreateStyle(this CSSRuleCollection me)
        {
            HTML.Style style = new JawTek.Web.Utility.HTML.Style();
            style.CssRules = me;
            return style;
        }

        /// <summary>
        /// Creates a <see cref="Link"/> HTML tag referencing an
        /// external stylesheet containing the CSS rules.
        /// </summary>
        /// <param name="me">The collection to build the stylesheet from.</param>
        /// <returns>A <see cref="Link"/> HTML tag referencing an
        /// external stylesheet containing the CSS rules.</returns>
        public static HTML.Link CreateLink(this CSSRuleCollection me)
        {
            string mdf = me.GetMD5HashCode();
            mdf += ".css";
            string path = HttpContext.Current.Server.MapPath("~/dyn/css/");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = Path.Combine(path, mdf);
            HTML.Link link;
            if (File.Exists(path))
            {
                link = new HTML.Link("~/dyn/css/" + mdf);
            }
            else
            {
                StreamWriter sw = File.CreateText(path);
                sw.Write(me.ToString());
                sw.Close();
                link = new HTML.Link("~/dyn/css/" + mdf);
            }
            link.Rel = "stylesheet";
            link.Type = "text/css";
            return link;
        }
    }
}
