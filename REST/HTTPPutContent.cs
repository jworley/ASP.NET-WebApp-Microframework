using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Web;
using System.IO;

namespace JawTek.Web.REST
{
    public class HTTPPutContent
    {
        private byte[] _bytes;
        private string _string;
        private XDocument _xml;

        public byte[] Bytes { get { return this._bytes; } }
        public string String { get { return this._string; } }
        public XDocument XML { get { return this._xml; } }

        private HTTPPutContent()
        {
        }

        public static HTTPPutContent FromRequest(HttpRequest request)
        {
            Stream s = request.InputStream;
            HTTPPutContent content = new HTTPPutContent();
            content._bytes = new byte[s.Length];
            s.Read(content._bytes, 0, (int)s.Length);
            try
            {
                content._string = request.ContentEncoding.GetString(content._bytes);
            }
            catch (Exception) { }
            if (!String.IsNullOrEmpty(content._string))
            {
                try
                {
                    content._xml = XDocument.Parse(content._string);
                }
                catch (Exception) { }
            }
            return content;
        }
    }
}
