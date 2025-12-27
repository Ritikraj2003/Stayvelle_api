using System.ComponentModel.DataAnnotations;

namespace Stayvelle.Models
{
    public class RoomModel : CommonModel
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public decimal Price { get; set; }
        public int MaxOccupancy { get; set; }
        public string Floor { get; set; } 
        public string NumberOfBeds { get; set; }
        public string AcType { get; set; } 
        public string BathroomType { get; set; }
        public string RoomStatus { get; set; } 
        public string RoomType { get; set; } 
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public bool IsTv { get; set; } = false;
        public string? Images { get; set; } // JSON string or comma-separated image paths/URLs

    }
}

