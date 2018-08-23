using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Protoacme.Models
{
    public class AcmeAccount : SerializableBase<AcmeAccount>
    {
        public int Id { get; set; }

        public string KID { get; set; }

        public RSAParameters SecurityInfo { get; set; }

        public List<string> Contact { get; set; }
    }
}
