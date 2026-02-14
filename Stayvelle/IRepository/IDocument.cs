using Stayvelle.Models;
using Stayvelle.Models;

namespace Stayvelle.IRepository
{
    public interface IDocument
    {
        Task<Response<List<DocumentDto>>> GetDocumentsAsync(string entityType, int entityId);
    }
}
