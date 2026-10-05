using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.security;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using SysX509 = System.Security.Cryptography.X509Certificates;

namespace DigitalSignature
{
    public class Firma
    {
        public static void SignHashed(
        string Source,
        string Target,
        SysX509.X509Certificate2 Certificate,
        string Reason,
        string Location,
        bool AddVisibleSign,
        bool AddTimeStamp,
        string strTSA)
        {
            if (Certificate == null)
                throw new ArgumentNullException("Certificate");

            if (!File.Exists(Source))
                throw new FileNotFoundException(
                    "No existe el PDF de entrada.",
                    Source);

            // ==========================================
            // CERTIFICADO
            // ==========================================

            if (!Certificate.HasPrivateKey)
            {
                throw new CryptographicException(
                    "El certificado no tiene clave privada.");
            }

            // ==========================================
            // CERTIFICADO BOUNCY CASTLE
            // ==========================================

            X509CertificateParser objCP =
                new X509CertificateParser();

            X509Certificate[] objChain =
                new X509Certificate[]
                {
            objCP.ReadCertificate(
                Certificate.RawData)
                };

            // ==========================================
            // CRL
            // ==========================================

            IList<ICrlClient> crlList =
                new List<ICrlClient>();

            crlList.Add(
                new CrlClientOnline(objChain));

            PdfReader objReader = null;
            PdfStamper objStamper = null;
            FileStream outputStream = null;

            try
            {
                // ==========================================
                // PDF DE ENTRADA
                // ==========================================

                objReader =
                    new PdfReader(Source);

                // ==========================================
                // PDF DE SALIDA
                // ==========================================

                outputStream =
                    new FileStream(
                        Target,
                        FileMode.Create,
                        FileAccess.Write);

                objStamper =
                    PdfStamper.CreateSignature(
                        objReader,
                        outputStream,
                        '\0',
                        null,
                        true);

                // ==========================================
                // APARIENCIA
                // ==========================================

                PdfSignatureAppearance signatureAppearance =
                    objStamper.SignatureAppearance;

                signatureAppearance.Reason =
                    Reason;

                signatureAppearance.Location =
                    Location;

                // ==========================================
                // FIRMA VISIBLE
                // ==========================================

                if (AddVisibleSign)
                {
                    signatureAppearance.SetVisibleSignature(
                        new Rectangle(
                            100,
                            100,
                            300,
                            200),
                        1,
                        null);
                }

                // ==========================================
                // TSA / OCSP
                // ==========================================

                ITSAClient tsaClient = null;
                IOcspClient ocspClient = null;

                if (AddTimeStamp)
                {
                    ocspClient =
                        new OcspClientBouncyCastle();

                    tsaClient =
                        new TSAClientBouncyCastle(
                            strTSA);
                }

                // ==========================================
                // FIRMA EXTERNA
                // ==========================================
                //
                // IMPORTANTE:
                //
                // NO utilizar:
                //
                // X509Certificate2Signature
                //
                // porque esa implementación utiliza
                // Certificate.PrivateKey.
                //
                // Nuestra implementación utiliza:
                //
                // Certificate.GetRSAPrivateKey()
                //
                // y permite que CryptoIDE/IAM/Windows
                // gestione el PIN.
                //

                IExternalSignature externalSignature =
                    new SmartCardExternalSignature(
                        Certificate,
                        "SHA-256");

                // ==========================================
                // FIRMAR PDF
                // ==========================================
                //
                // En este punto iTextSharp solicitará
                // la firma criptográfica.
                //
                // CryptoIDE/IAM puede mostrar el diálogo
                // del PIN.
                //

                MakeSignature.SignDetached(
                    signatureAppearance,
                    externalSignature,
                    objChain,
                    crlList,
                    ocspClient,
                    tsaClient,
                    0,
                    CryptoStandard.CMS);
            }
            finally
            {
                // ==========================================
                // CERRAR RECURSOS
                // ==========================================

                if (objStamper != null)
                    objStamper.Close();

                if (objReader != null)
                    objReader.Close();

                if (outputStream != null)
                    outputStream.Close();
            }
        }

    }
}