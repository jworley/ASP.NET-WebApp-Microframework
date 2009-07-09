using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JawTek.Web
{
    interface IHTTPAllow
    {
        /// <summary>
        /// Tells the framework what HTTP verbs this handler accepts
        /// </summary>
        /// <returns>Comma delimited string of HTTP verbs.</returns>
        string Allow();
    }
}
