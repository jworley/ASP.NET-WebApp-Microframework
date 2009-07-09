using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Security;

namespace JawTek.Web
{
    class WebAppProcessor: IWebAppGet, IWebAppPost, REST.IWebAppPut, REST.IWebAppDelete
    {
        #region IWebAppGet Members

        public void get(params string[] args)
        {
            IWebAppGet wa = this.webApp as IWebAppGet;
            if (wa != null)
            {
                this.CheckAllow();
                if (this.CheckAuth("get"))
                    wa.get(args);
            }
            else { this.respondError(); }
        }

        #endregion

        #region IWebAppPost Members

        public void post(params string[] args)
        {
            IWebAppPost wa = this.webApp as IWebAppPost;
            if (wa != null)
            {
                this.CheckAllow();
                if (this.CheckAuth("post"))
                    wa.post(args);
            }
            else { this.respondError(); }
        }

        #endregion

        #region IWebAppPut Members

        public void put(params string[] args)
        {
            REST.IWebAppPut wa = this.webApp as REST.IWebAppPut;
            if (wa != null)
            {
                this.CheckAllow();
                if (this.CheckAuth("put"))
                    wa.put(args);
            }
        }

        #endregion

        #region IWebAppDelete Members

        public void delete(params string[] args)
        {
            REST.IWebAppDelete wa = this.webApp as REST.IWebAppDelete;
            if (wa != null)
            {
                this.CheckAllow();
                if(this.CheckAuth("delete"))
                    wa.delete(args);
            }
        }

        #endregion

        #region IWebApp Members

        public HttpRequest Request
        {
            get
            {
                return this.webApp.Request;
            }
        }

        public HttpResponse Response
        {
            get
            {
                return this.webApp.Response;
            }
        }

        #endregion

        private IWebApp webApp;

        public WebAppProcessor(IWebApp webapp)
        {
            try
            {
                this.webApp = webapp;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not instantiate WebApp", ex);
            }
        }

        private void respondError()
        {
            this.Response.StatusCode = 403;
        }

        private void CheckAllow()
        {
            if (this.webApp is IHTTPAllow)
            {
                IHTTPAllow wa = this.webApp as IHTTPAllow;
                this.Response.AppendHeader("Allow", wa.Allow());
            }
        }

        private bool CheckAuth(string method)
        {
            WebAppLoginRequiredAttribute[] attribs = this.webApp.GetType()
                .GetMethod(method).GetCustomAttributes(typeof(WebAppLoginRequiredAttribute)
                , false) as WebAppLoginRequiredAttribute[];
            if (attribs.Length > 0 && !Request.IsAuthenticated)
            {
                FormsAuthentication.RedirectToLoginPage("msg=Login+required+for+this+action");
                return false;
            }
            else
            {
                WebAppRoleRequiredAttribute[] attr = this.webApp.GetType()
                    .GetMethod(method).GetCustomAttributes(typeof(WebAppRoleRequiredAttribute),
                    false) as WebAppRoleRequiredAttribute[];
                bool inRole = false;
                HttpContext c = HttpContext.Current;
                foreach (WebAppRoleRequiredAttribute i in attr)
                {
                    inRole = ((inRole) || (c.User.IsInRole(i.Role)));
                }
                if (!inRole && attr.Length > 0)
                {
                    FormsAuthentication.RedirectToLoginPage("msg=" + Uri.EscapeDataString("You don't have permission to perform this action"));
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

    }
}
