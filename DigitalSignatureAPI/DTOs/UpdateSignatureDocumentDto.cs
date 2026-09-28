namespace DigitalSignatureAPI.DTOs
{
    public class UpdateSignatureDocumentDto
    {
        public string SignedFileName { get; set; }
        public string SignedFullPath { get; set; }
        public string Status { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string SignatureResult { get; set; }
    }
}
