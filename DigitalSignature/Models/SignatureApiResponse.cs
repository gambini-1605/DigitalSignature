using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DigitalSignature.Models
{
    public class SignatureApiResponse
    {
        [JsonProperty("requestId")]
        public Guid RequestId { get; set; }

        [JsonProperty("DateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("documents")]
        public List<DocumentResponse> Documents { get; set; }
    }
}
