using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Core.InternalModels
{
    internal class ACCKEY
    {
        public string account { get; set; }

        public JWK newKey { get; set; }
    }
}
