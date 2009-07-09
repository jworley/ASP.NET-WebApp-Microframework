using System;
using System.Web;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using System.Web.SessionState;
namespace JawTek.Web
{
    public class WebAppHandlerFactory : IHttpHandlerFactory
    {
        #region IHttpHandlerFactory Members

        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            Config.WebAppConfiguration w = Config.WebAppConfiguration.GetConfig();
            string u = context.Request.AppRelativeCurrentExecutionFilePath.Substring(1);
            if (u.Contains(".ashx"))
                u = (String.IsNullOrEmpty(context.Request.PathInfo)) ? "/" : context.Request.PathInfo;
            var staticdirs = w.StaticPaths.ToList();
            foreach (var item in staticdirs)
            {
                Regex r = new Regex("^" + item.Path + "(.*)$");
                if (r.IsMatch(u))
                {
                    Match m = r.Match(u);
                    string file = m.Groups[1].Value;
                    return new WebAppStaticHandler("~" + item.StaticDir + file);
                }
            }
            if (w.UseDyn)
            {
                Regex r = new Regex("^/dyn/");
                if (r.IsMatch(u))
                {
                    string path = "~" + u;
                    return new WebAppStaticHandler(path);
                }
            }
            List<Config.WebAppConfigurationPath> l = w.Paths.ToList();
            foreach (Config.WebAppConfigurationPath item in l)
            {
                Regex r = new Regex("^" + item.Path + "$");
                if (r.IsMatch(u) && (item.Handler != null))
                {
                    List<string> args = new List<string>();
                    Match m = r.Match(u);
                    for (int i = 0; i < m.Groups.Count; i++)
                    {
                        args.Add(m.Groups[i].Value);
                    }
                    return new WebAppHandler(item.Handler, requestType, args.ToArray());
                }
            }
            return null;
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
        }

        #endregion

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    /// <summary>
    /// For using the WebApp Framework in a shared hosting environment
    /// </summary>
    public class WebAppSharedHandler : IHttpHandler, IRequiresSessionState
    {

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            IHttpHandler h = new WebAppHandlerFactory().GetHandler(context, context.Request.RequestType,
                context.Request.Path, context.Request.PhysicalPath);
            if (h != null)
                h.ProcessRequest(context);
        }

        #endregion
    }

    public class WebAppHandler : IHttpHandler, IRequiresSessionState
    {
        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            WebAppProcessor we = new WebAppProcessor(this.webApp);
            switch (this.method)
            {
                case "GET":
                    we.get(this.args);
                    break;
                case "POST":
                    we.post(this.args);
                    break;
                case "PUT":
                    we.put(this.args);
                    break;
                case "DELETE":
                    we.delete(this.args);
                    break;
                default:
                    context.Response.StatusCode = 403;
                    break;
            }
        }

        #endregion

        private IWebApp webApp;
        private string method;
        private string[] args;

        public WebAppHandler(IWebApp webapp, string method, params string[] args)
        {
            this.webApp = webapp;
            this.method = method.ToUpper();
            this.args = args;
        }
    }

    public class WebAppStaticHandler : IHttpHandler
    {

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string file = context.Server.MapPath(this.filename);
            if (File.Exists(file))
            {
                string mime = "application/octetstream";
                string ext = System.IO.Path.GetExtension(file).ToLower();

                Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
                if (rk != null && rk.GetValue("Content Type") != null)
                {
                    mime = rk.GetValue("Content Type").ToString();
                }

                context.Response.ContentType = mime;
                byte[] b = File.ReadAllBytes(file);
                context.Response.OutputStream.Write(b, 0, b.Length);
            }
            else
            {
                context.Response.StatusCode = 404;
            }
        }

        #endregion

        private string filename;

        public WebAppStaticHandler(string filename)
        {
            this.filename = filename;
        }
    }
}
