using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.ComponentModel;
using System.Reflection;
using JawTek.Web.Utility.CSS;
using System.Data.Linq.Mapping;
using System.Text.RegularExpressions;
using JawTek.Web.Utility.Javascript;

namespace JawTek.Web.Utility.HTML
{
    public static class HTMLUtility
    {
        private static string[] selfClosingTags = new string[] { "area", "base", "basefont", "br", 
            "hr", "input", "img", "link", "meta" };
        private const string indentString = "\t";
        private const string defaultCharSet = "utf-8";
        private const string str_indentstring = "JawTek.Web.Utility.HTML.IndentString";
        private const string str_defaultChar = "JawTek.Web.Utility.HTML.DefaultCharSet";


        public static string IndentString
        {
            get
            {
                object indent = HttpContext.Current.Items[HTMLUtility.str_indentstring];
                string indentSTR = String.Empty;
                if (indent != null)
                    indentSTR = indent.ToString();
                return (String.IsNullOrEmpty(indentSTR))
                    ? HTMLUtility.indentString : indentSTR;
            }
            set
            {
                if (!HttpContext.Current.Items.Contains(HTMLUtility.str_indentstring))
                {
                    HttpContext.Current.Items.Add(HTMLUtility.str_indentstring, value);
                }
                else
                {
                    HttpContext.Current.Items[HTMLUtility.str_indentstring] = value;
                }
            }
        }
        public static string DefaultCharSet
        {
            get
            {
                object encode = HttpContext.Current.Items[HTMLUtility.str_defaultChar];
                string encodeSTR = String.Empty;
                if(encode!=null)
                    encodeSTR = encode.ToString();
                return (String.IsNullOrEmpty(encodeSTR))
                    ? HTMLUtility.defaultCharSet : encodeSTR;
            }
            set
            {
                if (!HttpContext.Current.Items.Contains(str_defaultChar))
                {
                    HttpContext.Current.Items.Add(HTMLUtility.str_defaultChar, value);
                }
                else
                {
                    HttpContext.Current.Items[str_defaultChar] = value;
                }
            }
        }
        public static string[] SelfClosingTags
        {
            get { return HTMLUtility.selfClosingTags; }
        }
        public static string HTMLEncode(string html)
        {
            char[] chars = HttpUtility.HtmlEncode(html).ToCharArray();
            StringBuilder response = new StringBuilder(html.Length + (int)(html.Length * 0.1));

            foreach (char c in chars)
            {
                int v = Convert.ToInt32(c);
                if (v > 127)
                {
                    response.AppendFormat("&{0};", v);
                }
                else
                {
                    response.Append(c);
                }
            }
            return response.ToString();
        }
        public static string ResolveURL(string url)
        {
            if (!String.IsNullOrEmpty(url) && (url.StartsWith("~")))
            {
                return (HttpContext.Current.Request.ApplicationPath +
                        url.Substring(1)).Replace("//", "/");
            }
            return url;
        }
        public static string RemoveHTML(string html)
        {
            string pattern = @"<(.|\n)*?>";
            return Regex.Replace(html, pattern, string.Empty);
        }

        public static string StripHTML(this string me)
        {
            return HTMLUtility.RemoveHTML(me);
        }

        public static HTMLEntity.HTMLEntityCollection BuildCollection<T>(IEnumerable<T> collection, Func<T, HTMLEntity> aggregate)
        {
            HTMLEntity.HTMLEntityCollection coll = new HTMLEntity.HTMLEntityCollection();
            foreach (var item in collection)
            {
                coll.Add(aggregate(item));
            }
            return coll;
        }

    }

    [TypeConverter(typeof(HTMLEntityTypeConvertor))]
    public class HTMLEntity
    {
        protected Dictionary<string, string> properties = new Dictionary<string, string>();
        protected string tag = "";
        protected HTMLEntityCollection children;
        protected HTMLEntity parent;

        public HTMLEntity Parent { get { return this.parent; } }

        public HTMLEntityCollection Children { get { return this.children; } }

        public string Tag { get { return this.tag; } }

        public HTMLEntity(string tag) : this(tag, (HTMLEntity)null) { }
        public HTMLEntity(string tag, HTMLEntity child)
        {
            this.tag = tag;
            this.children = new HTMLEntityCollection(this);
            if (child != null)
            {
                this.children.Add(child);
            }
        }
        public HTMLEntity(string tag, string child) : this(tag, new TextNode(child)) { }

        public HTMLEntity SetProperty(string name, string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                this.RemoveProperty(name);
            }
            else
            {
                if (this.properties.ContainsKey(name))
                {
                    this.RemoveProperty(name);
                }
                this.properties.Add(name, value);
            }
            return this;
        }

        public string GetProperty(string name)
        {
            if (this.properties.ContainsKey(name))
            {
                return this.properties[name];
            }
            else
            {
                return null;
            }
        }

        public void RemoveProperty(string name)
        {
            if (this.properties.ContainsKey(name))
            {
                this.properties.Remove(name);
            }
        }

        public string[] Classes
        {
            get
            {
                string classes = this.GetProperty("class");
                if (String.IsNullOrEmpty(classes))
                {
                    return new string[] { };
                }
                else
                {
                    return classes.Split(' ');
                }
            }
            set
            {
                string classes = value.Aggregate((x, y) => x + " " + y);
                this.SetProperty("class", classes);
            }
        }

        public string ID
        {
            get
            {
                return this.GetProperty("id");
            }
            set
            {
                this.SetProperty("id", value);
            }
        }

        public static string ResolveURL(string url)
        {
            return HTMLUtility.ResolveURL(url);
        }

        public HTMLEntity Up()
        {
            if (this.parent != null)
            {
                return this.parent.Up();
            }
            else
            {
                return this;
            }
        }

        public override string ToString()
        {
            return this.ToString(0);
        }
        public virtual string ToString(int indentLevel)
        {
            if (this.tag == "html")
            {
                if (JavascriptUtility.ContainsScripts)
                {
                    var body = (from t in this.Children
                                where t.tag == "body"
                                select t).FirstOrDefault();
                    if (body == null)
                    {
                        body = new HTMLEntity("body");
                        this.Children.Add(body);
                    }
                    body.Children.Add(JavascriptUtility.GetScript());
                }
            }
            string indent = "";
            for (int i = 0; i < indentLevel; i++)
            {
                indent += HTMLUtility.IndentString;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}<{1}", new object[] { indent, this.tag });
            string attr = this.properties.Aggregate<KeyValuePair<string, string>, string>("",
                (x, y) => x += " " + y.Key + "='" + y.Value + "'");
            sb.Append(attr);
            if (this.children.Count == 0 && HTMLUtility.SelfClosingTags.Contains(this.tag))
            {
                sb.Append(" />");
            }
            else
            {
                sb.Append(">");
                sb.Append(this.children.ToString(indentLevel));
                if (indentLevel * this.children.Count > 0)
                {
                    sb.AppendFormat("{1}</{0}>", new object[] { this.tag, indent });
                }
                else
                {
                    sb.AppendFormat("</{0}>", this.tag);
                }
            }
            return sb.ToString();
        }
        public class HTMLEntityCollection : IChainableCollection<HTMLEntity>
        {
            private List<HTMLEntity> collection = new List<HTMLEntity>();
            private HTMLEntity parent;

            #region IChainableCollection<HTMLEntity> Members

            public IChainableCollection<HTMLEntity> Add(HTMLEntity item)
            {
                item.parent = this.parent;
                collection.Add(item);
                return this;
            }

            public IChainableCollection<HTMLEntity> Clear()
            {
                collection.Clear();
                return this;
            }

            public IChainableCollection<HTMLEntity> CopyTo(HTMLEntity[] array, int arrayindex)
            {
                this.collection.CopyTo(array, arrayindex);
                return this;
            }

            #endregion

            #region ICollection<HTMLEntity> Members

            void ICollection<HTMLEntity>.Add(HTMLEntity item)
            {
                this.Add(item);
            }

            void ICollection<HTMLEntity>.Clear()
            {
                this.Clear();
            }

            public bool Contains(HTMLEntity item)
            {
                return this.collection.Contains(item);
            }

            void ICollection<HTMLEntity>.CopyTo(HTMLEntity[] array, int arrayIndex)
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

            public bool Remove(HTMLEntity item)
            {
                return this.collection.Remove(item);
            }

            #endregion

            #region IEnumerable<HTMLEntity> Members

            public IEnumerator<HTMLEntity> GetEnumerator()
            {
                return ((IEnumerable<HTMLEntity>)this.collection).GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)this.collection).GetEnumerator();
            }

            #endregion

            public IChainableCollection<HTMLEntity> Insert(int index, HTMLEntity item)
            {
                this.collection.Insert(index, item);
                return this;
            }

            public HTMLEntityCollection(HTMLEntity parent)
            {
                this.parent = parent;
            }

            public HTMLEntityCollection() : this(null) { }

            public override string ToString()
            {
                return this.ToString(0);
            }

            public string ToString(int indentLevel)
            {
                bool first = true;
                StringBuilder sb = new StringBuilder();
                foreach (HTMLEntity h in this)
                {
                    TextNode s = h as TextNode;
                    if (s != null && indentLevel == 0)
                    {
                        sb.AppendFormat("{0}", new object[] { s.ToString() });
                    }
                    else
                    {
                        if (first)
                        {
                            sb.Append("\n");
                            first = false;
                        }
                        sb.AppendFormat("{0}\n", h.ToString(indentLevel + 1));
                    }
                }
                return sb.ToString();
            }
        }
    }

    public class HTMLEntityTypeConvertor : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            else
            {
                return base.CanConvertFrom(context, sourceType);
            }
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value.GetType() == typeof(string))
            {
                string s = (string)value;
                return new TextNode(s);
            }
            else
            {
                return base.ConvertFrom(context, culture, value);
            }
        }
    }

    public class TextNode : HTMLEntity
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string ID { get { return null; } set { } }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string[] Classes { get { return null; } set { } }
        private StringBuilder text = new StringBuilder();

        public StringBuilder Text
        {
            get { return this.text; }
            set { this.text = value; }
        }

        public TextNode() : base("") { }
        public TextNode(string text)
            : this()
        {
            this.text = new StringBuilder(text);
        }

        public override string ToString()
        {
            return this.ToString(0);
        }
        public override string ToString(int indentLevel)
        {
            string indent = "";
            for (int i = 0; i < indentLevel; i++)
            {
                indent += HTMLUtility.IndentString;
            }
            return indent + this.Text.ToString();
        }
    }

    public class A : HTMLEntity
    {
        public string Href
        {
            get
            {
                return this.GetProperty("href");
            }
            set
            {
                this.SetProperty("href", A.ResolveURL(value));
            }
        }

        public A(string href) : this(href, (HTMLEntity)null) { }
        public A(string href, string child) : this(href, new TextNode(child)) { }
        public A(string href, HTMLEntity child)
            : base("a", child)
        {
            this.Href = href;
        }

        public A AddProperty(string name, string value)
        {
            return (A)base.SetProperty(name, value);
        }
    }

    public enum InputType
    {
        Button,
        Checkbox,
        File,
        Hidden,
        Image,
        Password,
        Radio,
        Reset,
        Submit,
        Text
    }

    public class Input : HTMLEntity
    {
        public InputType Type
        {
            get
            {
                InputType t = (InputType)Enum.Parse(typeof(InputType), this.GetProperty("type"), true);
                return t;
            }
            set
            {
                string t = Enum.GetName(typeof(InputType), (object)value).ToLower();
                this.SetProperty("type", t);
            }
        }

        public string Name
        {
            get
            {
                return this.GetProperty("name");
            }
            set
            {
                this.SetProperty("name", value);
            }
        }

        public string Value
        {
            get
            {
                return this.GetProperty("value");
            }
            set
            {
                this.SetProperty("value", value);
            }
        }

        public Input(InputType type) : this(type, null) { }
        public Input(InputType type, string name) : this(type, name, null) { }
        public Input(InputType type, string name, string value)
            : base("input")
        {
            this.Type = type;
            this.Name = name;
            this.Value = value;
        }
    }

    public class Label : HTMLEntity
    {
        private Input input;

        public Input For
        {
            get
            {
                return this.input;
            }
            set
            {
                this.input = value;
                this.SetProperty("for", this.input.ID);
            }
        }

        public Label(Input inputFor) : this(inputFor, null) { }
        public Label(Input inputFor, string text)
            : base("label", text)
        {
            this.For = inputFor;
        }
    }

    public class TextArea : HTMLEntity
    {
        public Nullable<int> Cols
        {
            get
            {
                string cols = this.GetProperty("cols");
                int c = 0;
                Nullable<int> r = new Nullable<int>();
                if (int.TryParse(cols, out c))
                {
                    r = c;
                }
                return r;
            }
            set
            {
                if (!value.HasValue)
                {
                    this.RemoveProperty("cols");
                }
                else
                {
                    this.SetProperty("cols", value.Value.ToString());
                }
            }
        }

        public Nullable<int> Rows
        {
            get
            {
                string rows = this.GetProperty("rows");
                int r = 0;
                Nullable<int> c = new Nullable<int>();
                if (int.TryParse(rows, out r))
                {
                    c = r;
                }
                return c;
            }
            set
            {
                if (!value.HasValue)
                {
                    this.RemoveProperty("rows");
                }
                else
                {
                    this.SetProperty("rows", value.Value.ToString());
                }
            }
        }

        public string Name
        {
            get
            {
                return this.GetProperty("name");
            }
            set
            {
                this.SetProperty("name", value);
            }
        }

        public TextArea() : this(new Nullable<int>(),new Nullable<int>(), null, null) { }
        public TextArea(string name) : this(new Nullable<int>(), new Nullable<int>(), null, name) { }
        public TextArea(HTMLEntity child) : this(new Nullable<int>(), new Nullable<int>(), child, null) { }
        public TextArea(HTMLEntity child, string name) : this(new Nullable<int>(), new Nullable<int>(), child, name) { }
        public TextArea(Nullable<int> cols, Nullable<int> rows) : this(cols, rows, null, null) { }
        public TextArea(Nullable<int> cols, Nullable<int> rows, string name) : this(cols, rows, null, name) { }
        public TextArea(Nullable<int> cols, Nullable<int> rows, HTMLEntity child) : this(cols, rows, child, null) { }
        public TextArea(Nullable<int> cols, Nullable<int> rows, HTMLEntity child, string name)
            : base("textarea", child)
        {
            this.Cols = cols;
            this.Rows = rows;
            this.Name = name;
        }
        public override string ToString(int indentLevel)
        {
            Nullable<int> c = this.Cols;
            if (!c.HasValue)
            {
                this.Cols = 20;
            }
            Nullable<int> r = this.Rows;
            if (!r.HasValue)
            {
                this.Rows = 2;
            }
            string retval = base.ToString(indentLevel);
            this.Cols = c;
            this.Rows = r;
            return retval;
        }
    }

    public class Form : HTMLEntity
    {
        public string Method
        {
            get
            {
                return this.GetProperty("method");
            }
            set
            {
                this.SetProperty("method", value);
            }
        }

        public string Action
        {
            get
            {
                return this.GetProperty("action");
            }
            set
            {
                this.SetProperty("action", HTMLUtility.ResolveURL(value));
            }
        }

        public Form(string method) : this(method, null) { }
        public Form(string method, string action) : this(method, action, null) { }
        public Form(string method, string action, HTMLEntity child)
            : base("form", child)
        {
            this.Method = method;
            this.Action = action;
        }

        public override string ToString(int indentLevel)
        {
            string a = this.Action;
            if (String.IsNullOrEmpty(a))
            {
                HttpRequest req = HttpContext.Current.Request;
                string query = req.QueryString.ToString();
                this.Action = req.AppRelativeCurrentExecutionFilePath + ((String.IsNullOrEmpty(query)) ? "" : "?" + query);
            }
            string r = base.ToString(indentLevel);
            this.Action = a;
            return r;
        }
    }

    public class HTML : HTMLEntity
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string ID { get { return null; } set { } }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string[] Classes { get { return null; } set { } }

        public string XMLNS
        {
            get
            {
                return this.GetProperty("xmlns");
            }
            set
            {
                this.SetProperty("xmlns", value);
            }
        }

        public HTML() : this("http://www.w3.org/1999/xhtml") { }
        public HTML(string xmlns) : this(null, xmlns) { }
        public HTML(HTMLEntity child) : this(child, "http://www.w3.org/1999/xhtml") { }
        public HTML(HTMLEntity child, string xmlns)
            : base("html", child)
        {
            this.XMLNS = xmlns;
        }

        public override string ToString(int indentLevel)
        {
            return "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" " +
                "\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\n" +
                base.ToString(indentLevel);
        }
    }

    public class Head : HTMLEntity
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string ID { get { return null; } set { } }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string[] Classes { get { return null; } set { } }

        public Head() : this(null) { }
        public Head(HTMLEntity child)
            : base("head", child)
        {
            this.AddMeta("generator", "JawTek WebApp Framework");
            this.AddMeta(HTTPHeaders.ContentType, "text/html;charset=" + HTMLUtility.DefaultCharSet);
        }

        public Head AddTitle(string title)
        {
            foreach (var i in this.Children.ToArray().Where(x => x as Title != null))
            {
                this.Children.Remove(i);
            }
            this.Children.Add(new Title(title));
            return this;
        }

        public Head AddMeta(HTTPHeaders httpEquiv, string child)
        {
            this.AddMeta(new Meta(httpEquiv, child));
            return this;
        }
        public Head AddMeta(string name, string child)
        {
            this.AddMeta(new Meta(name, child));
            return this;
        }
        private void AddMeta(Meta meta)
        {
            this.children.Add(meta);
        }

        public Head AddLink(string href) { return this.AddLink(href, null); }
        public Head AddLink(string href, string type) { return this.AddLink(href, type, null); }
        public Head AddLink(string href, string type, string media) { return this.AddLink(href, type, media, null); }
        public Head AddLink(string href, string type, string media, string rel) { return this.AddLink(href, type, media, rel, null); }
        public Head AddLink(string href, string type, string media, string rel, string charset)
        {
            Link l = new Link(href, type, media, rel, charset);
            this.Children.Add(l);
            return this;
        }
    }

    public class Title : HTMLEntity
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string ID { get { return null; } set { } }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string[] Classes { get { return null; } set { } }

        public Title() : this((HTMLEntity)null) { }
        public Title(HTMLEntity child) : base("title", child) { }
        public Title(string child) : this(new TextNode(child)) { }
    }

    public class Meta : HTMLEntity
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string ID { get { return null; } set { } }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string[] Classes { get { return null; } set { } }

        public string Name
        {
            get
            {
                return this.GetProperty("name");
            }
            set
            {
                this.SetProperty("name", value);
            }
        }

        public string Content
        {
            get
            {
                return this.GetProperty("content");
            }
            set
            {
                this.SetProperty("content", value);
            }
        }

        public Nullable<HTTPHeaders> HttpEquiv
        {
            get
            {
                string s = this.GetProperty("http-equiv");
                if (!String.IsNullOrEmpty(s))
                {
                    return (HTTPHeaders)(new HTTPHeaders()).FromStringValue(s);
                }
                else
                {
                    return null;
                }
            }
            set
            {
                this.SetProperty("http-equiv", value.GetStringValue());
            }
        }


        public Meta(HTTPHeaders httpEquiv, string content)
            : base("meta")
        {
            this.HttpEquiv = httpEquiv;
            this.Content = content;
        }
        public Meta(string name, string content)
            : base("meta")
        {
            this.Name = name;
            this.Content = content;
        }

        public override string ToString()
        {
            this.children.Clear();
            return base.ToString();
        }
    }

    public class Link : HTMLEntity
    {
        public string CharSet
        {
            get
            {
                return this.GetProperty("charset");
            }
            set
            {
                this.SetProperty("charset", value);
            }
        }
        public string Href
        {
            get
            {
                return this.GetProperty("href");
            }
            set
            {
                this.SetProperty("href", Link.ResolveURL(value));
            }
        }
        public string Media
        {
            get
            {
                return this.GetProperty("media");
            }
            set
            {
                this.SetProperty("media", value);
            }
        }
        public string Rel
        {
            get { return this.GetProperty("rel"); }
            set { this.SetProperty("rel", value); }
        }
        public string Type
        {
            get { return this.GetProperty("type"); }
            set { this.SetProperty("type", value); }
        }

        public override string ToString()
        {
            this.children.Clear();
            return base.ToString();
        }

        public Link(string href) : this(href, null) { }
        public Link(string href, string type) : this(href, type, null) { }
        public Link(string href, string type, string media) : this(href, type, media, null) { }
        public Link(string href, string type, string media, string rel) : this(href, type, media, rel, null) { }
        public Link(string href, string type, string media, string rel, string charset)
            : base("link")
        {
            this.Href = href;
            this.Type = type;
            this.Media = media;
            this.Rel = rel;
            this.CharSet = charset;
        }
    }

    public class Style : HTMLEntity
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string ID { get { return null; } set { } }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new string[] Classes { get { return null; } set { } }

        private CSSRuleCollection cssRules = new CSSRuleCollection();
        public CSSRuleCollection CssRules
        {
            get { return this.cssRules; }
            set { this.cssRules = value; }
        }

        public string Type
        {
            get { return this.GetProperty("type"); }
            set { this.SetProperty("type", value); }
        }

        public string Media
        {
            get { return this.GetProperty("media"); }
            set { this.SetProperty("media", value); }
        }

        public Style() : this("") { }
        public Style(CSSRuleCollection rules) : this("", rules) { }
        public Style(string media) : this(media, "text/css") { }
        public Style(string media, string type) : this(media, new CSSRuleCollection(), type) { }
        public Style(string media, CSSRuleCollection rules) : this(media, rules, "text/css") { }
        public Style(string media, CSSRuleCollection rules, string type)
            : base("style")
        {
            this.Media = media;
            this.Type = type;
            if (rules != null)
            {
                this.cssRules = rules;
            }
        }

        public override string ToString(int indentLevel)
        {
            string s = this.CssRules.ToString(indentLevel + 1).Trim();
            this.Children.Clear().Add(new TextNode(s));
            return base.ToString(indentLevel);
        }
    }

    public class Div : HTMLEntity
    {
        public Div() : this(null) { }
        public Div(HTMLEntity child) : base("div", child) { }
    }

    public class Select : HTMLEntity
    {
        public string Name
        {
            get
            {
                return this.GetProperty("name");
            }
            set
            {
                this.SetProperty("name", value);
            }
        }
        public bool Disabled
        {
            get
            {
                return this.GetProperty("disabled") != null;
            }
            set
            {
                this.SetProperty("disabled", (value) ? "disabled" : null);
            }
        }
        public bool Multiple
        {
            get
            {
                return this.GetProperty("multiple") != null;
            }
            set
            {
                this.SetProperty("multiple", (value) ? "multiple" : null);
            }
        }
        public Nullable<int> Size
        {
            get
            {
                string size = this.GetProperty("size");
                int s = 0;
                Nullable<int> r = new Nullable<int>();
                if (int.TryParse(size, out s))
                {
                    r = s;
                }
                return r;
            }
            set
            {
                if (!value.HasValue)
                {
                    this.RemoveProperty("size");
                }
                else
                {
                    this.SetProperty("size", value.Value.ToString());
                }
            }
        }
        public Select() : base("select") { }
        public Select(string name)
            : this()
        {
            this.Name = name;
        }

        public static void AddOption(Select select, string display, string value)
        {
            Option op = new Option(display, value);
            select.Children.Add(op);
        }

        public Select AddOption(string display, string value)
        {
            Select.AddOption(this, display, value);
            return this;
        }

        public static Option GetOption(Select select, string value)
        {
            Option op = select.Children.First(i => (i is Option) ? (i as Option).Value == value : false) as Option;
            return op;
        }

        public Option GetOption(string value)
        {
            return Select.GetOption(this, value);
        }

    }
    public class Option : HTMLEntity
    {
        public string Value
        {
            get
            {
                return this.GetProperty("value");
            }
            set
            {
                this.SetProperty("value", value);
            }
        }
        public bool Selected
        {
            get
            {
                return this.GetProperty("selected") != null;
            }
            set
            {
                this.SetProperty("selected", (value) ? "selected" : null);
            }
        }
        public bool Disabled
        {
            get
            {
                return this.GetProperty("disabled") != null;
            }
            set
            {
                this.SetProperty("disabled", (value) ? "disabled" : null);
            }
        }

        public Option(string display, string value)
            : base("option", display)
        {
            this.Value = value;
        }
        public Option(string display, string value, bool selected)
            : this(display, value)
        {
            this.Selected = selected;
        }
    }

    public class Script : HTMLEntity
    {
        private JavascriptCollection _code;

        public new string ID { get { return null; } set { } }
        public new string[] Classes { get { return null; } set { } }

        public string Type
        {
            get { return this.GetProperty("type"); }
            set { this.SetProperty("type", value); }
        }

        public string CharSet
        {
            get { return this.GetProperty("charset"); }
            set { this.SetProperty("charset", value); }
        }

        public string Src
        {
            get { return this.GetProperty("src"); }
            set { this.SetProperty("src", value); }
        }

        public JavascriptCollection Code
        {
            get { return this._code; }
            set { this._code = value; }
        }

        public Script() : this(string.Empty, string.Empty) { }
        public Script(string src) : this(src, string.Empty) { }
        public Script(string src, string charset)
            : base("script")
        {
            this.Src = src;
            this.CharSet = charset;
            this._code = new JavascriptCollection();
        }
        public Script(JavascriptCollection code)
            : base("script")
        {
            this._code = code;
        }

        public override string ToString(int indentLevel)
        {
            string type = this.Type;
            this.Type = (String.IsNullOrEmpty(type)) ? "text/javascript" : type;
            string src = this.Src;
            this.Src = (String.IsNullOrEmpty(src)) ? String.Empty : HTMLUtility.ResolveURL(src);
            this.Children.Clear();

            if (this.Code.Count > 0)
            {
                string indent = "";
                for (int i = 0; i < indentLevel + 1; i++)
                {
                    indent += HTMLUtility.IndentString;
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("//<![CDATA[");
                sb.AppendLine(indent + this.Code.ToString(indentLevel + 1));
                sb.AppendLine(indent + "//]]>");
                this.Children.Add(new TextNode(sb.ToString().Trim()));
            }
            
            string result = base.ToString(indentLevel);
            this.Type = type;
            this.Src = src;
            return result;
        }
    }

    public enum InsertOptions
    {
        Before,
        After
    }

    public static class HTMLCollectionExtensions
    {
        public static IChainableCollection<HTMLEntity> Insert(this IChainableCollection<HTMLEntity> me,
            int index, HTMLEntity item)
        {
            if (me is HTMLEntity.HTMLEntityCollection)
            {
                HTMLEntity.HTMLEntityCollection c = me as HTMLEntity.HTMLEntityCollection;
                c.Insert(index, item);
            }
            return me;
        }

        public static IChainableCollection<HTMLEntity> AddCollection(this IChainableCollection<HTMLEntity> me,
            IChainableCollection<HTMLEntity> add)
        {
            return me.AddCollection(add, InsertOptions.After);
        }
        public static IChainableCollection<HTMLEntity> AddCollection(this IChainableCollection<HTMLEntity> me,
            IChainableCollection<HTMLEntity> add, InsertOptions location)
        {
            var items = (location == InsertOptions.Before) ? add.Reverse().ToArray() : add.ToArray();
            foreach (var item in items)
            {
                switch (location)
                {
                    case InsertOptions.Before:
                        me.Insert(0, item);
                        break;
                    case InsertOptions.After:
                        me.Add(item);
                        break;
                    default:
                        break;
                }
            }
            return me;
        }

    }
    public class GenerateFormOptions
    {
        public List<string> IgnoredFields = new List<string>();
        public List<string> HiddenFields = new List<string>();
        public object Value = null;

        public GenerateFormOptions() : this(null, new string[0], new string[0]) { }
        public GenerateFormOptions(object value) : this(value, new string[0], new string[0]) { }
        public GenerateFormOptions(string[] ignoredFields, string[] hiddenFields) : this(null, ignoredFields, hiddenFields) { }
        public GenerateFormOptions(object value, string[] ignoredFields, string[] hiddenFields)
        {
            this.Value = value;
            this.IgnoredFields = new List<string>();
            this.HiddenFields = new List<string>();
            foreach (string iF in ignoredFields)
            {
                this.IgnoredFields.Add(iF);
            }
            foreach (string hF in hiddenFields)
            {
                this.HiddenFields.Add(hF);
            }
        }
    }

    public static class FormUtility
    {
        public static HTMLEntity.HTMLEntityCollection GenerateFormFields(Type type)
        {
            return FormUtility.GenerateFormFields(type, new GenerateFormOptions());
        }
        public static HTMLEntity.HTMLEntityCollection GenerateFormFields(Type type, GenerateFormOptions options)
        {
            HTMLEntity.HTMLEntityCollection coll = new HTMLEntity.HTMLEntityCollection();
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                CaseInsensitiveComparer cisc = new CaseInsensitiveComparer();
                if (options.IgnoredFields.Any(s => s.ToLower() == prop.Name.ToLower()))
                    continue;
                if (!prop.CanWrite)
                    continue;
                if (prop.GetCustomAttributes(typeof(AssociationAttribute), true).Length > 0)
                    continue;
                var att = prop.GetCustomAttributes(typeof(ColumnAttribute), true)
                    .FirstOrDefault() as ColumnAttribute;
                InputType fieldType = InputType.Text;
                if (options.HiddenFields.Any(h => h.ToLower() == prop.Name.ToLower()))
                    fieldType = InputType.Hidden;
                if ((att != null) && (fieldType != InputType.Hidden))
                {
                    if (att.DbType.ToLower().IndexOf("text") > -1)
                    {
                        var label = new HTMLEntity("span", FormUtility.DispayPropName(prop.Name));
                        var input = new TextArea(type.Name + "_" + prop.Name);
                        input.ID = input.Name;
                        if (options.Value != null)
                        {
                            string val = prop.GetValue(options.Value, null) as string;
                            if(!String.IsNullOrEmpty(val))
                                input.Children.Add(new TextNode(val));
                        }
                        coll.Add(label).Add(input);
                        continue;
                    }

                    if (att.DbType.ToLower().IndexOf("bit") > -1)
                        fieldType = InputType.Checkbox;
                }
                Input inp = new Input(fieldType, type.Name + "_" + prop.Name);
                inp.ID = inp.Name;
                if (options.Value != null)
                {
                    string val = prop.GetValue(options.Value, null).ToString();
                    if (!String.IsNullOrEmpty(val))
                        inp.Value = val;
                }
                if (fieldType != InputType.Hidden)
                {
                    Label lab = new Label(inp, FormUtility.DispayPropName(prop.Name));
                    coll.Add(lab).Add(inp);
                }
                else
                {
                    coll.Add(inp);
                }
            }
            return coll;
        }

        private static string DispayPropName(string name)
        {
            string n = name;
            for (int i = 1; i < n.Length; i++)
            {
                char l = n[i];
                if (l.ToString() == l.ToString().ToUpper())
                {
                    n = n.Insert(i, " ");
                    i++;
                }
            }
            n = Regex.Replace(n, @"_", " ");
            n = Regex.Replace(n,@"\s+"," ");
            n = Regex.Replace(n, @"\b(\w)", 
                delegate(Match m) 
                { 
                    return m.Groups[1].Value.ToUpper(); 
                });
            return n.Trim();
        }

        public static T FormToObject<T>() where T : new()
        {
            var Req = HttpContext.Current.Request;
            var type = typeof(T);
            var con = type.GetConstructor(Type.EmptyTypes);
            var objct = (T)con.Invoke(new Object[0]);
            var formFields = Req.Form.AllKeys.Where(n => n.StartsWith(type.Name));
            foreach (var item in formFields)
            {
                string field = item.Substring(type.Name.Length + 1);
                var prop = type.GetProperty(field);
                try
                {
                    prop.SetValue(objct, Req.Form[item], null);
                }
                catch
                {
                    StringConverter conv = new StringConverter();
                    if (conv.CanConvertTo(prop.PropertyType))
                    {
                        var obj = conv.ConvertFromString(Req.Form[item]);
                        prop.SetValue(objct, obj, null);
                    }
                    else
                    {
                        try
                        {
                            var ctor = prop.PropertyType.GetConstructor(new Type[] { typeof(string) });
                            if (ctor != null)
                            {
                                var obj = ctor.Invoke(new object[] { Req.Form[item] });
                                prop.SetValue(objct, obj, null);
                            }
                            else
                            {
                                var parse = prop.PropertyType.GetMethod("Parse", new Type[] { typeof(string) });
                                if (parse != null)
                                {
                                    var obj = parse.Invoke(null, new object[] { Req.Form[item] });
                                    prop.SetValue(objct, obj, null);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            return objct;
        }

    }
}



