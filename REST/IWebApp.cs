using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JawTek.Web;

namespace JawTek.Web.REST
{
    public interface IWebAppPut:IWebApp
    {
        void put(params string[] args);
    }

    public interface IWebAppDelete:IWebApp
    {
        void delete(params string[] args);
    }
}
