using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JawTek.Web.REST
{
    public class WebAppRESTBase : WebAppBase, IHTTPAllow
    {

        #region IHTTPAllow Members

        public string Allow()
        {
            List<string> verbs = new List<string>();
            verbs.Add("HEAD");
            if (this is IWebAppGet)
                verbs.Add("GET");
            if (this is IWebAppPost)
                verbs.Add("POST");
            if (this is IWebAppPut)
                verbs.Add("PUT");
            if (this is IWebAppDelete)
                verbs.Add("DELETE");
            string allow = "";
            verbs.ForEach(v => allow += String.IsNullOrEmpty(allow) ? v : ", " + v);
            return allow;
        }

        #endregion
    }
}
