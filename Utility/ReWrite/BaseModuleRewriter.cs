using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace JawTek.Web.Utility.ReWrite
{
    public abstract class BaseModuleRewriter : IHttpModule 
    {
        #region IHttpModule Members

        public void Dispose() { }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            this.Rewrite(app.Request.Path, app);
        }
        #endregion


        public abstract void Rewrite(string requestedPath, HttpApplication app);
    }
}
