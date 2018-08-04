using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace System
{
    public static class StringExtensions
    {
        public static string AppendUrl(this string root, string url)
        {
            return Path.Combine(root, url).Replace(@"\", "/");
        }
    }
}
