using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DigitalSignature.Models
{
    public  class SignatureApiRequest
    {
        [JsonProperty("requestId")]
        public Guid RequestId { get; set; }

        [JsonProperty("documentIds")]
        public List<int> DocumentIds { get; set; }
    }
}
