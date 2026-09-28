using System.Text.Json.Serialization;

namespace DigitalSignatureAPI.DTOs
{
    public class SignatureApiResponseDto
    {
        [JsonPropertyName("requestId")]
        public Guid RequestId { get; set; }

        [JsonPropertyName("DateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonPropertyName("Status")]
        public string Status { get; set; } = string.Empty;
        
        [JsonPropertyName("documents")]
        public List<DocumentResponseDto> Documents { get; set; } = new List<DocumentResponseDto>();
    }

    public class DocumentResponseDto
    {
        public int TocId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string FullPath { get; set; }
        public string SignedFullPath { get; set; }
    }

}
