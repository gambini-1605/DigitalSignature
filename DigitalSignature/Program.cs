using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DigitalSignature
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Guid requestId = Guid.Empty;
            //List<int> documentIds = new List<int>();

            Guid requestId = Guid.Parse("7F3C2A91-8B54-4D21-A6E7-91C84F52B103");
            List<int> documentIds = new List<int>();
            documentIds.Add(100);
            documentIds.Add(200);
            documentIds.Add(201);

            // Validar que se recibieron los argumentos desde el proceso externo
            if (args != null && args.Length >= 2)
            {
                // El primer argumento es el Guid
                Guid.TryParse(args[0], out requestId);

                // El segundo argumento es la lista de IDs separados por comas (ej. "100,200,201")
                documentIds = args[1]
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();
            }

            // Iniciamos el formulario pasándole los parámetros capturados
            Application.Run(new FrmDigitalSignature(requestId, documentIds));
        }
    }
}
