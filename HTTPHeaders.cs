using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JawTek.Web.Utility;
using System.Reflection;

namespace JawTek.Web
{
    public enum HTTPHeaders
    {
        [StringValue("allow")]
        Allow,
        [StringValue("content-encoding")]
        ContentEncoding,
        [StringValue("content-length")]
        ContentLength,
        [StringValue("content-type")]
        ContentType,
        [StringValue("date")]
        Date,
        [StringValue("expires")]
        Expires,
        [StringValue("last-modified")]
        LastModified,
        [StringValue("location")]
        Location,
        [StringValue("refresh")]
        Refresh,
        [StringValue("set-cookie")]
        SetCookie,
        [StringValue("www-authenticate")]
        WWWAuthenticate
    }

    public static class EnumExtensions
    {
        /// <summary>
        /// Will get the string value for a given enums value, this will
        /// only work if you assign the StringValue attribute to
        /// the items in your enum.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetStringValue(this Enum value)
        {
            // Get the type
            Type type = value.GetType();

            // Get fieldinfo for this type
            FieldInfo fieldInfo = type.GetField(value.ToString());

            // Get the stringvalue attributes
            StringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(
                typeof(StringValueAttribute), false) as StringValueAttribute[];

            // Return the first if there was a match.
            return attribs.Length > 0 ? attribs[0].StringValue : null;
        }

        public static Enum FromStringValue(this Enum me, string value)
        {
            Type type = me.GetType();

            var fieldInfo = type.GetFields();
            Dictionary<FieldInfo, string> dict = new Dictionary<FieldInfo, string>();
            foreach (var i in fieldInfo)
            {
                var attribs = i.GetCustomAttributes(
                    typeof(StringValueAttribute), false) as StringValueAttribute[];
                string s = attribs.Length > 0 ? attribs[0].StringValue : "";
                dict.Add(i, s);
            }
            var a = dict.Where(kvp => kvp.Value.ToLower() == value.ToLower()).Select(k => k.Key).First();
            if (a != null)
            {
                return (Enum)Enum.Parse(type, a.Name);
            }
            else
            {
                return null;
            }
        }
    }
}
