using Newtonsoft.Json;

namespace DigitalSignature.Models
{
    public class UpdateSignatureDocumentResponse
    {
        // Datos que arma el cliente a partir de la respuesta HTTP
        [JsonIgnore]
        public bool Success { get; set; }

        [JsonIgnore]
        public int StatusCode { get; set; }

        // Cuerpo devuelto por la API (ajústalo a lo que retorne tu controller)
        [JsonProperty("message")]
        public string Message { get; set; }

        // Cuerpo crudo, útil para depurar si la API devuelve otro formato
        [JsonIgnore]
        public string RawBody { get; set; }
    }
}