using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Protoacme.Core.Utilities;

namespace Protoacme.Utility.Certificates
{
    public class CSR
    {
        private readonly byte[] _bytes;
        private readonly RSAParameters _rsaParameters;

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

        public RSAParameters RSAParameters
        {
            get
            {
                return _rsaParameters;
            }
        }

        internal CSR(byte[] bytes, RSAParameters rsaParameters)
        {
            _bytes = bytes;
            _rsaParameters = rsaParameters;
        }
    }
}
