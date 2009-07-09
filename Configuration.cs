using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JawTek.Web.Utility.HTML;

namespace JawTek.Web.Config
{
    class WebAppConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("paths", IsRequired = true)]
        public WebAppConfigurationPathCollection Paths
        {
            get
            {
                return this["paths"] as WebAppConfigurationPathCollection;
            }
        }

        [ConfigurationProperty("static", IsRequired = false)]
        public WebAppConfigurationStaticPathCollection StaticPaths
        {
            get
            {
                return this["static"] as WebAppConfigurationStaticPathCollection;
            }
        }

        [ConfigurationProperty("urlrewrite", IsRequired = false)]
        public WebAppConfigurationRewriteCollection UrlRewrites
        {
            get
            {
                return this["urlrewrite"] as WebAppConfigurationRewriteCollection;
            }
        }

        [ConfigurationProperty("usedyn", DefaultValue = true)]
        public bool UseDyn
        {
            get
            {
                return Convert.ToBoolean(this["usedyn"]);
            }
        }

        [ConfigurationProperty("utility", IsRequired = false)]
        public WebAppUtilityConfiguration Utility
        {
            get
            {
                return this["utility"] as WebAppUtilityConfiguration;
            }
        }

        public static WebAppConfiguration GetConfig()
        {
            return ConfigurationManager.GetSection("webAppConfig") as WebAppConfiguration;
        }
    }

    class WebAppUtilityConfiguration : ConfigurationElement
    {
        [ConfigurationProperty("html",IsRequired = false)]
        public WebAppHTMLConfiguration HTML
        {
            get
            {
                return this["html"] as WebAppHTMLConfiguration;
            }
        }
    }

    class WebAppHTMLConfiguration : ConfigurationElement
    {
        [ConfigurationProperty("indentstring", IsRequired = false, DefaultValue="\t")]
        public string IndentString
        {
            get
            {
                return this["indentstring"] as string;
            }
        }

        [ConfigurationProperty("charset", IsRequired = false, DefaultValue = "utf-8")]
        public string CharSet
        {
            get
            {
                return this["charset"] as string;
            }
        }

        [ConfigurationProperty("doctype", IsRequired = false, DefaultValue = DocType.XHTML1)]
        public DocType DocType
        {
            get
            {
                return (DocType)this["doctype"];
            }
        }

        [ConfigurationProperty("mode", IsRequired = false, DefaultValue = Mode.Transitional)]
        public Mode Mode
        {
            get
            {
                return (Mode)this["mode"];
            }
        }
    }

    class WebAppConfigurationPath : ConfigurationElement
    {
        [ConfigurationProperty("path", IsRequired = true)]
        public string Path
        {
            get
            {
                return this["path"] as string;
            }
        }

        [ConfigurationProperty("handler", IsRequired = true)]
        public IWebApp Handler
        {
            get
            {
                return this["handler"] as IWebApp;
            }
        }
    }

    class WebAppConfigurationPathCollection : ConfigurationElementCollection
    {
        public WebAppConfigurationPath this[object key]
        {
            get
            {
                return base.BaseGet(key) as WebAppConfigurationPath;
            }
        }
        protected override ConfigurationElement CreateNewElement()
        {
            return new WebAppConfigurationPath();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WebAppConfigurationPath)element).Path;
        }

        public List<WebAppConfigurationPath> ToList()
        {
            object[] keys = base.BaseGetAllKeys();
            List<WebAppConfigurationPath> l = new List<WebAppConfigurationPath>();
            foreach (object key in keys)
            {
                l.Add((WebAppConfigurationPath)base.BaseGet(key));
            }
            return l;
        }
    }

    class WebAppConfigurationStaticPath : ConfigurationElement
    {
        [ConfigurationProperty("path", IsRequired = true)]
        public string Path
        {
            get
            {
                return this["path"] as string;
            }
        }

        [ConfigurationProperty("staticdir", IsRequired = true)]
        public string StaticDir
        {
            get
            {
                return this["staticdir"] as string;
            }
        }

        [ConfigurationProperty("ext", IsRequired = false)]
        public string Ext
        {
            get
            {
                return this["ext"] as string;
            }
        }

        public string[] Exts
        {
            get
            {
                return this.Ext.Split(',');
            }
        }
    }

    class WebAppConfigurationStaticPathCollection : ConfigurationElementCollection
    {
        public WebAppConfigurationStaticPath this[object key]
        {
            get
            {
                return base.BaseGet(key) as WebAppConfigurationStaticPath;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new WebAppConfigurationStaticPath();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WebAppConfigurationStaticPath)element).Path;
        }

        public List<WebAppConfigurationStaticPath> ToList()
        {
            object[] keys = base.BaseGetAllKeys();
            List<WebAppConfigurationStaticPath> l = new List<WebAppConfigurationStaticPath>();
            foreach (object key in keys)
            {
                l.Add((WebAppConfigurationStaticPath)base.BaseGet(key));
            }
            return l;
        }
    }

    class WebAppConfigurationRewrite : ConfigurationElement
    {
        [ConfigurationProperty("inpath", IsRequired = true)]
        public string InPath
        {
            get
            {
                return this["inpath"] as string;
            }
        }

        [ConfigurationProperty("outpath", IsRequired = true)]
        public string OutPath
        {
            get
            {
                return this["outpath"] as string;
            }
        }
    }

    class WebAppConfigurationRewriteCollection : ConfigurationElementCollection
    {
        public WebAppConfigurationRewrite this[object key]
        {
            get
            {
                return this.BaseGet(key) as WebAppConfigurationRewrite;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new WebAppConfigurationRewrite();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((WebAppConfigurationRewrite)element).InPath;
        }

        public List<WebAppConfigurationRewrite> ToList()
        {
            object[] keys = this.BaseGetAllKeys();
            List<WebAppConfigurationRewrite> l = new List<WebAppConfigurationRewrite>();
            foreach (object key in keys)
            {
                l.Add((WebAppConfigurationRewrite)this.BaseGet(key));
            }
            return l;
        }
    }
}
