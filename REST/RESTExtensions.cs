using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace JawTek.Web.REST
{
    public static class RESTExtensions
    {
        public static HTTPPutContent GetPutContent(this HttpRequest me)
        {
            return HTTPPutContent.FromRequest(me);
        }
    }
}
