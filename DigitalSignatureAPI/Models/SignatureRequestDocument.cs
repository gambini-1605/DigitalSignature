namespace DigitalSignatureAPI.Models
{
    public class SignatureRequestDocument
    {
        public int Id { get; set; }
        public Guid RequestId { get; set; }
        public int TocId { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public string SignedFileName { get; set; }
        public string SignedFullPath { get; set; }
        public string Status { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public string SignatureResult { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        // Propiedad de navegación
        public virtual SignatureRequest SignatureRequest { get; set; }
    }
}
