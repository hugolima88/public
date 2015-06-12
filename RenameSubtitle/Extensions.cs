using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RenameSubtitle
{
    public static class Extensions
    {
        public static bool EndsWithAny(this string text, string[] array)
        {
            foreach(string s in array)
            {
                if (text.EndsWith(s))
                    return true;
            }

            return false;
        }
    }
}
