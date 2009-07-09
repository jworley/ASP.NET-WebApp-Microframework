using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using JawTek.Web.Utility.Javascript.Tools;
using System.Collections;
using JawTek.Web.Utility.HTML;
using System.Security.Cryptography;
using System.IO;

namespace JawTek.Web.Utility.Javascript
{
    public static class JavascriptUtility
    {
        private const string str_includeinline = "JawTek.Web.Utility.Javascript.IncludeInline";
        private const string str_scriptCollection = "JawTek.Web.Utility.Javascript.ScriptCollection";

        public static bool IncludeInline
        {
            get
            {
                object inline = HttpContext.Current.Items[JavascriptUtility.str_includeinline];
                return (inline == null) ? true : (bool)inline;
            }
            set
            {
                HttpContext.Current.Items.Add(JavascriptUtility.str_includeinline, value);
            }
        }

        private static JavascriptCollection ScriptCollection
        {
            get
            {
                object coll = HttpContext.Current.Items[JavascriptUtility.str_scriptCollection];
                return (coll == null) ? new JavascriptCollection() : coll as JavascriptCollection;
            }
            set
            {
                if (HttpContext.Current.Items.Contains(JavascriptUtility.str_scriptCollection))
                {
                    HttpContext.Current.Items[JavascriptUtility.str_scriptCollection] = value;
                }
                else
                {
                    HttpContext.Current.Items.Add(JavascriptUtility.str_scriptCollection, value);
                }
            }
        }

        public static bool ContainsScripts
        {
            get { return JavascriptUtility.ScriptCollection.Count > 0; }
        }

        public static Script GetScript()
        {
            return GetScript(ScriptCollection, IncludeInline);
        }

        public static Script GetScript(JavascriptCode code)
        {
            return GetScript(code, IncludeInline);
        }

        public static Script GetScript(JavascriptCode code, bool isInline)
        {
            Script script = new Script();
            if (isInline)
            {
                script.Code.Add(code);
            }
            else
            {
                string mdf = code.GetMD5HashCode();
                mdf += ".js";
                string path = HttpContext.Current.Server.MapPath("~/dyn/js/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = Path.Combine(path, mdf);
                if (!File.Exists(path))
                {
                    StreamWriter sw = File.CreateText(path);
                    sw.Write(code.ToString());
                    sw.Close();
                }
                script.Src = "~/dyn/js/" + mdf;
            }
            return script;
        }

        public static void RegisterCode(JavascriptCode code)
        {
            JavascriptCollection coll = JavascriptUtility.ScriptCollection;
            coll.Add(code);
            JavascriptUtility.ScriptCollection = coll;
        }

        public static void UnRegisterCode(JavascriptCode code)
        {
            JavascriptCollection coll = JavascriptUtility.ScriptCollection;
            coll.Remove(code);
            JavascriptUtility.ScriptCollection = coll;
        }

        public static void ClearCode()
        {
            JavascriptUtility.ScriptCollection = new JavascriptCollection();
        }
    }

    public abstract class JavascriptCode
    {
        public void RegisterCode()
        {
            JavascriptUtility.RegisterCode(this);
        }

        public void UnRegisterCode()
        {
            JavascriptUtility.UnRegisterCode(this);
        }

        public override string ToString()
        {
            return this.ToString(0);
        }

        public string ToString(int indentLevel)
        {
            Javascriptformatter formatter = new Javascriptformatter(indentLevel);
            return formatter.Format(this.GetCode());
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

        public abstract string GetCode();
    }

    public class CodeBlock : JavascriptCode
    {
        private string _code;
        public string Code
        {
            get { return this._code; }
            set { this._code = value; }
        }

        public CodeBlock() { }
        public CodeBlock(string code) { this._code = code; }

        public override string GetCode()
        {
            return this.Code;
        }
    }

    public class Function : JavascriptCode
    {
        private string _name;
        private string[] _params;
        private CodeBlock _codeBlock;

        public string Name
        {
            get { return this._name; }
            set { this._name = value; }
        }

        public string[] Params
        {
            get { return this._params; }
            set { this._params = value; }
        }

        public CodeBlock CodeBlock
        {
            get { return this._codeBlock; }
            set { this._codeBlock = value; }
        }

        public Function() : this(string.Empty, new CodeBlock(), new string[0]) { }
        public Function(string name, params string[] parameters) : this(name, new CodeBlock(), parameters) { }
        public Function(string name, CodeBlock codeBlock) : this(name, codeBlock, new string[0]) { }
        public Function(string name, CodeBlock codeBlock, params string[] parameters)
        {
            this._name = name;
            this._codeBlock = codeBlock;
            this._params = parameters;
        }

        public override string GetCode()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("function");
            sb.Append((String.IsNullOrEmpty(this._name)) ? String.Empty : " " + this._name);
            string paramList = string.Empty;
            if (this._params.Length > 0)
                paramList = this._params.Aggregate((a, s) => (a + "," + s).TrimStart(','));
            sb.Append("(" + paramList + "){\n");
            sb.Append(this._codeBlock.GetCode());
            sb.Append("}");
            return sb.ToString();
        }
    }

    public class JavascriptCollection : JavascriptCode, IChainableCollection<JavascriptCode>
    {
        List<JavascriptCode> coll = new List<JavascriptCode>();

        #region IChainableCollection<JavascriptCode> Members

        public IChainableCollection<JavascriptCode> Add(JavascriptCode item)
        {
            coll.Add(item);
            return this;
        }

        public IChainableCollection<JavascriptCode> Clear()
        {
            coll.Clear();
            return this;
        }

        public IChainableCollection<JavascriptCode> CopyTo(JavascriptCode[] array, int arrayindex)
        {
            coll.CopyTo(array, arrayindex);
            return this;
        }

        #endregion

        #region ICollection<JavascriptCode> Members

        void ICollection<JavascriptCode>.Add(JavascriptCode item)
        {
            this.Add(item);
        }

        void ICollection<JavascriptCode>.Clear()
        {
            this.Clear();
        }

        public bool Contains(JavascriptCode item)
        {
            return coll.Contains(item);
        }

        void ICollection<JavascriptCode>.CopyTo(JavascriptCode[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.coll.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(JavascriptCode item)
        {
            return coll.Remove(item);
        }

        #endregion

        #region IEnumerable<JavascriptCode> Members

        public IEnumerator<JavascriptCode> GetEnumerator()
        {
            return ((IEnumerable<JavascriptCode>)coll).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)coll).GetEnumerator();
        }

        #endregion

        public override string GetCode()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in this)
            {
                sb.AppendLine(item.GetCode());
            }
            return sb.ToString();
        }
    }
}
