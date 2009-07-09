using System;
using System.Web;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JawTek.Web
{
    [TypeConverter(typeof(IWebAppConvertor))]
    public interface IWebApp
    {
        HttpRequest Request
        {
            get;
        }
        HttpResponse Response
        {
            get;
        }
    }

    public class WebAppBase : IWebApp
    {
        #region IWebApp Members

        public HttpRequest Request
        {
            get { return Context.Request; }
        }

        public HttpResponse Response
        {
            get { return Context.Response; }
        }

        public HttpContext Context
        {
            get { return HttpContext.Current; }
        }

        #endregion
    }


    public interface IWebAppGet : IWebApp
    {
        void get(params string[] args);
    }

    public interface IWebAppPost : IWebApp
    {
        void post(params string[] args);
    }

    class IWebAppConvertor : TypeConverter
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

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }
            else
            {
                return base.CanConvertTo(context, destinationType);
            }
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value.GetType() == typeof(string))
            {
                string s = (string)value;
                AppDomain c = AppDomain.CurrentDomain;
                Type t = null;
                foreach (Assembly i in c.GetAssemblies())
                {
                    t = i.GetType(s, false);
                    if (t != null)
                    {
                        break;
                    }
                }
                if (t == null)
                {
                    return null;
                }
                else
                {
                    return (IWebApp)Activator.CreateInstance(t);
                }
            }
            else
            {
                return base.ConvertFrom(context, culture, value);
            }
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                Type t = value.GetType();
                return t.AssemblyQualifiedName;
            }
            else
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }

}
