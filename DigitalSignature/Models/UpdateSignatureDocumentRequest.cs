using Newtonsoft.Json;

namespace DigitalSignature.Models
{
    public class UpdateSignatureDocumentRequest
    {
        [JsonProperty("signedFileName")]
        public string SignedFileName { get; set; }

        [JsonProperty("signedFullPath")]
        public string SignedFullPath { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty("signatureResult")]
        public string SignatureResult { get; set; }
    }
}