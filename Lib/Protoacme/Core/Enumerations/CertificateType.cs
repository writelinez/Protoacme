using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Core.Enumerations
{
    public class CertificateType
    {
        private CertificateType() { }

        public string Value { get; private set; }

        /// <summary>
        /// File extension(s): .CER
        /// </summary>
        public static CertificateType Cert
        {
            get
            {
                return new CertificateType()
                {
                    Value = "pkix-cert"
                };
            }
        }

        /// <summary>
        /// File extension(s): .CRL
        /// </summary>
        public static CertificateType Crl
        {
            get
            {
                return new CertificateType()
                {
                    Value = "pkix-crl"
                };
            }
        }
    }
}
