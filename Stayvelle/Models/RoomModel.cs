using System.ComponentModel.DataAnnotations;

namespace Stayvelle.Models
{
    public class RoomModel : CommonModel
    {
        [Key]
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public decimal Price { get; set; }
        public int MaxOccupancy { get; set; }
        public string Floor { get; set; }
        public string NumberOfBeds { get; set; }
        public string ACType { get; set; } // Changed from AcType
        public string BathroomType { get; set; }
        public string RoomStatus { get; set; } 
        public string RoomType { get; set; } 
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public bool IsTv { get; set; } = false;
        
        // Context: Keeping this to avoid data loss, but new DocumentModel should be used for images.
        public string? Images { get; set; } 
    }
}

