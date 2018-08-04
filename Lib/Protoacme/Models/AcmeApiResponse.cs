using Protoacme.Core.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Protoacme.Models
{
    public class AcmeApiResponse<TData> : AcmeApiResponse
    {
        public TData Data { get; set; }
    }

    public class AcmeApiResponse
    {
        public AcmeApiResponseStatus Status { get; set; }

        public string Nonce { get; set; }

        public string Message { get; set; }
    }
}
