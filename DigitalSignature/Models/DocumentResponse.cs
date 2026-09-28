using Newtonsoft.Json;

namespace DigitalSignature.Models
{
    public class DocumentResponse
    {
        [JsonProperty("TocId")]
        public int TocId { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("FileName")]
        public string FileName { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("FullPath")]
        public string FullPath { get; set; }

        [JsonProperty("SignedFullPath")]
        public string SignedFullPath { get; set; }
    }
}