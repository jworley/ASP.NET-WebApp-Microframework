using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using JawTek.Web.Config;
using System.Text.RegularExpressions;

namespace JawTek.Web.Utility.ReWrite
{
    class WebAppURLRewriter : BaseModuleRewriter
    {
        public override void Rewrite(string requestedPath, HttpApplication app)
        {
            var rules = WebAppConfiguration.GetConfig().UrlRewrites.ToList();
            foreach (var i in rules)
            {
                string lookFor = "^" + RewriterUtils.ResolveUrl(app.Context.Request.ApplicationPath, i.InPath) + "$";
                Regex re = new Regex(lookFor, RegexOptions.IgnoreCase);
                if (re.IsMatch(requestedPath))
                {
                    string sendTo = RewriterUtils.ResolveUrl(app.Context.Request.ApplicationPath,
                        re.Replace(requestedPath, i.OutPath));
                    RewriterUtils.RewriteUrl(app.Context, sendTo);
                    break;
                }
            }
        }
    }
}
