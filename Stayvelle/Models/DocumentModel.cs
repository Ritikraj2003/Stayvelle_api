using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Stayvelle.Models
{
    public class DocumentModel : CommonModel
    {
        [Key]
        public int DocumentId { get; set; }

        public string EntityType { get; set; } = string.Empty; // USER / ROOM / GUEST

        public int EntityId { get; set; } // UsersModel.Id / RoomModel.Id / GuestDetailsModel.GuestId

        public string DocumentType { get; set; } = string.Empty; // USER_PROFILE / ROOM_IMAGE / ID_PROOF

        public string FileName { get; set; } = string.Empty;
        public string? Description { get; set; } // adhar ,pan  , voterid card /other
        public string FilePath { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
    public class DocumentDto
    {
        public int DocumentId { get; set; }
        public string EntityType { get; set; } = string.Empty; // USER / ROOM / GUEST
        public int EntityId { get; set; } // UsersModel.Id / RoomModel.Id / GuestDetailsModel.GuestId
        public string DocumentType { get; set; } = string.Empty; // USER_PROFILE / ROOM_IMAGE / ID_PROOF
        public string FileName { get; set; } = string.Empty;
        public string? Description { get; set; } // adhar ,pan  , voterid card /other
        public string? FilePath { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public IFormFile? File { get; set; } // Renamed to PascalCase to match usage
    }
}
