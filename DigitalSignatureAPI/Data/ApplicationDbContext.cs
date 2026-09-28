using DigitalSignatureAPI.Models;
using Microsoft.EntityFrameworkCore;


namespace DigitalSignatureAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<SignatureRequest> SignatureRequests { get; set; }
        public DbSet<SignatureRequestDocument> SignatureRequestDocuments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapeo a los nombres reales de las tablas en SQL
            modelBuilder.Entity<SignatureRequest>().ToTable("tbl_signature_request");
            modelBuilder.Entity<SignatureRequestDocument>().ToTable("tbl_signature_request_document");

            // Configuración de la llave foránea por RequestId
            modelBuilder.Entity<SignatureRequestDocument>()
                .HasOne(d => d.SignatureRequest)
                .WithMany(r => r.Documents)
                .HasPrincipalKey(r => r.RequestId)
                .HasForeignKey(d => d.RequestId);
        }
    }
}
