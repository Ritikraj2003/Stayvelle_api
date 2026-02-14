using Microsoft.EntityFrameworkCore;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Models;

namespace Stayvelle.RepositoryImpl
{
    public class DocumentRepository : IDocument
    {
        private readonly ApplicationDbContext _context;

        public DocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<List<DocumentDto>>> GetDocumentsAsync(string entityType, int entityId)
        {
            var response = new Response<List<DocumentDto>>();
            try
            {
                var documents = await _context.DocumentModel
                    .Where(d => d.EntityType == entityType && d.EntityId == entityId)
                    .Select(d => new DocumentDto
                    {
                        DocumentId = d.DocumentId,
                        DocumentType = d.DocumentType,
                        FilePath = d.FilePath,
                        IsPrimary = d.IsPrimary,
                        FileName = d.FileName,
                        Description = d.Description
                    })
                    .ToListAsync();

                response.Success = true;
                response.Data = documents;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
        }
    }
}
