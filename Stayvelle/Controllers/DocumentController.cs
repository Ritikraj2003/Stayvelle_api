using Microsoft.AspNetCore.Mvc;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Models.DTOs;

namespace Stayvelle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocument _documentRepository;

        public DocumentsController(IDocument documentRepository)
        {
            _documentRepository = documentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetDocuments([FromQuery] string entityType, [FromQuery] int entityId)
        {
            if (string.IsNullOrEmpty(entityType) || entityId <= 0)
            {
                return BadRequest("Entity Type and Valid Entity Id are required.");
            }

            var response = await _documentRepository.GetDocumentsAsync(entityType, entityId);
            if (response.Success)
            {
                return Ok(response.Data);
            }
            return BadRequest(response);
        }
    }
}
