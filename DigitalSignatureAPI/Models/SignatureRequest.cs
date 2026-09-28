namespace DigitalSignatureAPI.Models
{
    public class SignatureRequest
    {
        public int Id { get; set; }
        public Guid RequestId { get; set; }
        public string Status { get; set; }
        public int IdUsuario { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public DateTime? DateFinished { get; set; }

        public virtual ICollection<SignatureRequestDocument> Documents { get; set; }
    }
}
