using Stayvelle.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stayvelle.Models
{
    public class RoomModel : CommonModel
    {
        [Key]
        public int Id { get; set; }
        public string? RoomNumber { get; set; }
        public string? RoomQrToken { get; set; } // Unique token for QR code
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
        
        [NotMapped]
        public List<DocumentDto> Documents { get; set; } = new();

    }

    public class CreateRoomDTO : CommonModel
    {
        [Required(ErrorMessage = "RoomNumber is required")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "MaxOccupancy is required")]
        [Range(1, int.MaxValue, ErrorMessage = "MaxOccupancy must be at least 1")]
        public int MaxOccupancy { get; set; }

        [Required(ErrorMessage = "Floor is required")]
        public string Floor { get; set; } = string.Empty;

        [Required(ErrorMessage = "NumberOfBeds is required")]
        [RegularExpression("^[1-3]$", ErrorMessage = "NumberOfBeds must be '1', '2', or '3'")]
        public string NumberOfBeds { get; set; } = string.Empty;

        [Required(ErrorMessage = "AcType is required")]
        [RegularExpression("^(AC|Non-AC)$", ErrorMessage = "AcType must be 'AC' or 'Non-AC'")]
        public string AcType { get; set; } = string.Empty;

        [Required(ErrorMessage = "BathroomType is required")]
        [RegularExpression("^(Attached|Separate)$", ErrorMessage = "BathroomType must be 'Attached' or 'Separate'")]
        public string BathroomType { get; set; } = string.Empty;

        [Required(ErrorMessage = "RoomStatus is required")]
        [RegularExpression("^(Available|Blocked|Maintenance)$", ErrorMessage = "RoomStatus must be 'Available', 'Blocked', or 'Maintenance'")]
        public string RoomStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "RoomType is required")]
        [RegularExpression("^(Single|Double|Deluxe)$", ErrorMessage = "RoomType must be 'Single', 'Double', or 'Deluxe'")]
        public string RoomType { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Optional fields
        public string? Description { get; set; }

        public bool IsTv { get; set; } = false;

        //public List<string>? Images { get; set; } // List of base64 strings or image URLs

        public List<DocumentDto> Documents { get; set; } = new();
    }

    public class UpdateRoomDTO
    {
        public string? RoomNumber { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0")]
        public decimal? Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "MaxOccupancy must be at least 1")]
        public int? MaxOccupancy { get; set; }

        public string? Floor { get; set; }

        [RegularExpression("^[1-3]$", ErrorMessage = "NumberOfBeds must be '1', '2', or '3'")]
        public string? NumberOfBeds { get; set; }

        [RegularExpression("^(AC|Non-AC)$", ErrorMessage = "AcType must be 'AC' or 'Non-AC'")]
        public string? AcType { get; set; }

        [RegularExpression("^(Attached|Separate)$", ErrorMessage = "BathroomType must be 'Attached' or 'Separate'")]
        public string? BathroomType { get; set; }

        [RegularExpression("^(Available|Blocked|Maintenance)$", ErrorMessage = "RoomStatus must be 'Available', 'Blocked', or 'Maintenance'")]
        public string? RoomStatus { get; set; }

        [RegularExpression("^(Single|Double|Deluxe)$", ErrorMessage = "RoomType must be 'Single', 'Double', or 'Deluxe'")]
        public string? RoomType { get; set; }

        public bool? IsActive { get; set; }

        // Optional fields
        public string? Description { get; set; }

        public bool? IsTv { get; set; }

        //public List<string>? Images { get; set; } // List of base64 strings or image URLs
        // If null: preserves existing images
        // If provided: updates images (empty list = remove all images)
        
        public string ModifiedBy { get; set; } = string.Empty;

        public List<DocumentDto>? Documents { get; set; }
    }
}

