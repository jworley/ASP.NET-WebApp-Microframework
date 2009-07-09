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
    public static class CSSUtility
    {
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

    public class CSSRule
    {
        private string selector = "";
        public string Selector
        {
            get { return selector; }
            set { selector = value; }
        }
        private CSSPropertyCollection properties = new CSSPropertyCollection();
        public CSSPropertyCollection Properties
        {
            get { return this.properties; }
            set { this.properties = value; }
        }
        public CSSRule() { }
        public CSSRule(string selector)
        {
            this.selector = selector;
        }
        public CSSRule(string selector, CSSPropertyCollection properties)
            : this(selector)
        {
            if (properties != null)
            {
                this.properties = properties;
            }
        }
        public override string ToString()
        {
            return this.ToString(0);
        }

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

    public class CSSRuleCollection : IChainableCollection<CSSRule>
    {
        private List<CSSRule> collection = new List<CSSRule>();

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

        public IChainableCollection<CSSRule> Clear()
        {
            this.collection.Clear();
            return this;
        }

        public IChainableCollection<CSSRule> CopyTo(CSSRule[] array, int arrayindex)
        {
            this.collection.CopyTo(array, arrayindex);
            return this;
        }

        #endregion

        #region ICollection<CSSRule> Members

        void ICollection<CSSRule>.Add(CSSRule item)
        {
            this.Add(item);
        }

        void ICollection<CSSRule>.Clear()
        {
            this.Clear();
        }

        public bool Contains(CSSRule item)
        {
            return this.collection.Contains(item);
        }
        public bool ContainsSelector(string selector)
        {
            return this.collection.Any(c => c.Selector == selector);
        }

        void ICollection<CSSRule>.CopyTo(CSSRule[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.collection.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(CSSRule item)
        {
            return this.collection.Remove(item);
        }

        #endregion

        #region IEnumerable<CSSRule> Members

        public IEnumerator<CSSRule> GetEnumerator()
        {
            return ((ICollection<CSSRule>)this.collection).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.collection).GetEnumerator();
        }

        #endregion

        public override string ToString()
        {
            return this.ToString(0);
        }

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

    public class CSSProperty
    {
        private Regex urlUnit = new Regex("url\\(['\"]?(.*)['\"]?\\)");
        private string property = "";
        private string value = "";
        public string Property
        {
            get { return this.property; }
            set { this.property = value; }
        }
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

        public CSSProperty() { }
        public CSSProperty(string property, string value) { this.Property = property; this.Value = value; }

        public override string ToString()
        {
            return this.Property + ": " + this.Value + ";";
        }
    }

    public class CSSPropertyCollection : IChainableCollection<CSSProperty>
    {
        private List<CSSProperty> collection = new List<CSSProperty>();

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

        public IChainableCollection<CSSProperty> Clear()
        {
            this.collection.Clear();
            return this;
        }

        public IChainableCollection<CSSProperty> CopyTo(CSSProperty[] array, int arrayindex)
        {
            this.collection.CopyTo(array, arrayindex);
            return this;
        }

        #endregion

        #region ICollection<CSSProperty> Members

        void ICollection<CSSProperty>.Add(CSSProperty item)
        {
            this.collection.Add(item);
        }

        void ICollection<CSSProperty>.Clear()
        {
            this.Clear();
        }

        public bool Contains(CSSProperty item)
        {
            return this.Contains(item);
        }

        public bool Contains(string property)
        {
            return this.collection.Any(p => p.Property == property);
        }

        void ICollection<CSSProperty>.CopyTo(CSSProperty[] array, int arrayIndex)
        {
            this.collection.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.collection.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(CSSProperty item)
        {
            return this.collection.Remove(item);
        }

        #endregion

        #region IEnumerable<CSSProperty> Members

        public IEnumerator<CSSProperty> GetEnumerator()
        {
            return ((ICollection<CSSProperty>)this.collection).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.collection).GetEnumerator();
        }

        #endregion

        public override string ToString()
        {
            return this.ToString(0);
        }

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

    public static class CSSExtensions
    {
        public static IChainableCollection<CSSProperty> Add(this IChainableCollection<CSSProperty> me, string property, string value)
        {
            return me.Add(new CSSProperty(property, value));
        }

        public static IChainableCollection<CSSRule> Add(this IChainableCollection<CSSRule> me, string selector, CSSProperty property)
        {
            CSSPropertyCollection p = new CSSPropertyCollection();
            p.Add(property);
            return me.Add(new CSSRule(selector, p));
        }

        public static HTML.Style CreateStyle(this CSSRuleCollection me)
        {
            HTML.Style style = new JawTek.Web.Utility.HTML.Style();
            style.CssRules = me;
            return style;
        }

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
