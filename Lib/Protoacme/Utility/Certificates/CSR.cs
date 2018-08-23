using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Protoacme.Core.Utilities;
using Protoacme.Models;

namespace Protoacme.Utility.Certificates
{
    public class CSR : SerializableBase<CSR>
    {
        private byte[] _bytes;
        private RSAParameters _rsaParameters;

        public byte[] Bytes
        {
            get
            {
                return _bytes;
            }
            set
            {
                _bytes = value;
            }
        }

        [JsonIgnore]
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
            set
            {
                _rsaParameters = value;
            }
        }

        public CSR() { }

        public CSR(byte[] bytes, RSAParameters rsaParameters)
        {
            _bytes = bytes;
            _rsaParameters = rsaParameters;
        }
    }
}
