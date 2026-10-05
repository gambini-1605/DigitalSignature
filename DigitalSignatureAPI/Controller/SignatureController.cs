using DigitalSignatureAPI.Data;
using DigitalSignatureAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignatureAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class SignatureController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SignatureController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("documents")]
        public async Task<IActionResult> GetSignatureStatus([FromBody] SignatureApiRequestDto request)
        {
            // Validamos que el request no sea nulo
            if (request == null || request.DocumentIds == null)
            {
                return BadRequest("El formato del request es inválido.");
            }

            // Consultamos la base de datos trayendo la cabecera y sus documentos asociados
            var signatureRequest = await _context.SignatureRequests
            .Include(r => r.Documents)
            .FirstOrDefaultAsync(r => r.RequestId == request.RequestId);

            if (signatureRequest == null)
            {
                return NotFound($"No se encontró la solicitud con el RequestId: {request.RequestId}");
            }

            // Mapeamos el resultado al DTO de respuesta
            var response = new SignatureApiResponseDto
            {
                RequestId = signatureRequest.RequestId,
                DateCreated = signatureRequest.DateCreated,
                Status = signatureRequest.Status,
                // Filtramos los documentos para devolver solo los solicitados (según TocId)
                Documents = signatureRequest.Documents
                    .Where(doc => request.DocumentIds.Contains(doc.TocId))
                    .Select(doc => new DocumentResponseDto
                    {
                        TocId = doc.TocId,
                        Name = doc.Name ?? string.Empty,
                        FileName = doc.FileName ?? string.Empty,
                        Status = doc.Status,
                        FullPath = doc.FullPath ?? string.Empty,
                        SignedFullPath = doc.SignedFullPath ?? string.Empty
                    }).ToList()
            };

            return Ok(response);
        }

        [HttpPut("document/{tocId}")]
        public async Task<IActionResult> UpdateSignatureDocument(int tocId, [FromBody] UpdateSignatureDocumentDto dto)
        {
            // Buscar el registro por su Id (identificador principal de la tabla)
            var document = await _context.SignatureRequestDocuments.FirstOrDefaultAsync(d => d.TocId == tocId);

            if (document == null)
            {
                return NotFound(new { message = "El documento con el ID especificado no existe." });
            }

            // Actualizar únicamente los campos requeridos
            //document.SignedFileName = dto.SignedFileName;
            document.SignedFullPath = dto.SignedFullPath;
            document.Status = dto.Status;
            document.ErrorCode = dto.ErrorCode;
            document.ErrorMessage = dto.ErrorMessage;
            document.SignatureResult = dto.SignatureResult;

            // Asignar la fecha y hora actual a DateUpdated
            document.DateUpdated = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "Error al actualizar la base de datos.", error = ex.Message });
            }

            return Ok(new { message = "Documento actualizado exitosamente.", data = document });
        }
    }
}
