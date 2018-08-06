using System;
using System.Collections.Generic;
using System.Text;
using Protoacme.Core.Utilities;

namespace Protoacme.Utility.Certificates
{
    public class CSR
    {
        private readonly byte[] _bytes;

        public byte[] Bytes
        {
            get
            {
                return _bytes;
            }
        }

        public string Base64UrlEncoded
        {
            get
            {
                return Base64Tool.Encode(_bytes);
            }
        }

        internal CSR(byte[] bytes)
        {
            _bytes = bytes;
        }
    }
}
