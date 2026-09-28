using System.Text.Json.Serialization;

namespace DigitalSignatureAPI.DTOs
{
    public class SignatureApiRequestDto
    {
        [JsonPropertyName("requestId")]
        public Guid RequestId { get; set; }

        [JsonPropertyName("documentIds")]
        public List<int> DocumentIds { get; set; } = new List<int>();
    }
}
