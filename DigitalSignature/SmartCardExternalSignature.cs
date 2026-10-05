using iTextSharp.text.pdf.security;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace DigitalSignature
{
    public class SmartCardExternalSignature : IExternalSignature
    {
        private readonly X509Certificate2 certificate;
        private readonly string hashAlgorithm;

        public SmartCardExternalSignature(
            X509Certificate2 certificate,
            string hashAlgorithm)
        {
            if (certificate == null)
                throw new ArgumentNullException("certificate");

            this.certificate = certificate;
            this.hashAlgorithm = hashAlgorithm;
        }

        /// <summary>
        /// Algoritmo hash utilizado para la firma.
        /// </summary>
        public string GetHashAlgorithm()
        {
            return hashAlgorithm;
        }

        /// <summary>
        /// Algoritmo de cifrado/firma.
        /// </summary>
        public string GetEncryptionAlgorithm()
        {
            return "RSA";
        }

        /// <summary>
        /// Firma los datos utilizando la clave privada
        /// del certificado.
        ///
        /// Importante:
        /// Utiliza GetRSAPrivateKey() y NO Certificate.PrivateKey.
        /// Esto permite trabajar con certificados asociados
        /// a CSP/KSP/smart cards/tokens.
        /// </summary>
        public byte[] Sign(byte[] message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            using (RSA rsa = certificate.GetRSAPrivateKey())
            {
                if (rsa == null)
                {
                    throw new CryptographicException(
                        "No se pudo obtener la clave privada RSA.");
                }

                HashAlgorithmName algorithm;

                switch (hashAlgorithm.ToUpperInvariant())
                {
                    case "SHA-256":
                        algorithm = HashAlgorithmName.SHA256;
                        break;

                    case "SHA-384":
                        algorithm = HashAlgorithmName.SHA384;
                        break;

                    case "SHA-512":
                        algorithm = HashAlgorithmName.SHA512;
                        break;

                    default:
                        throw new NotSupportedException(
                            "Algoritmo hash no soportado: " +
                            hashAlgorithm);
                }

                // Esta operación puede provocar que
                // CryptoIDE/IAM/Windows solicite el PIN.
                return rsa.SignData(
                    message,
                    algorithm,
                    RSASignaturePadding.Pkcs1);
            }
        }
    }
}
