/*
 * THIS FILE IS A COPY FROM ACMESharpCore. ALL CREDIT FOR THE BASE64TOOL SHOULD GO TO ebekker/ACMESharp
 * See https://github.com/ebekker/ACMESharp for the full project.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Core.Utilities
{
    public static class Base64Tool
    {
        public static string Encode(string raw, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            return Encode(encoding.GetBytes(raw));
        }

        public static string Encode(byte[] raw)
        {
            string enc = Convert.ToBase64String(raw);
            enc = enc.Split('=')[0];
            enc = enc.Replace('+', '-');
            enc = enc.Replace('/', '_');
            return enc;
        }

        public static byte[] Decode(string enc)
        {
            string raw = enc;
            raw = raw.Replace('-', '+');
            raw = raw.Replace('_', '/');
            switch (raw.Length % 4)
            {
                case 0: break;
                case 2: raw += "=="; break;
                case 3: raw += "="; break;
                default:
                    throw new System.Exception("Illegal base64url string!");
            }
            return Convert.FromBase64String(raw);
        }

        public static string DecodeToString(string enc, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            return encoding.GetString(Decode(enc));
        }
    }
}
