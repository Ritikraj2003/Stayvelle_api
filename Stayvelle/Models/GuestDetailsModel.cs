using Stayvelle.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Stayvelle.Models
{
    public class GuestDetailsModel : CommonModel
    {
        [Key]
        public int GuestId { get; set; }

        // 🔗 Booking relation
        [Required]
        public int BookingId { get; set; }

        [ForeignKey("BookingId")]
        [JsonIgnore] // Prevent circular reference during JSON serialization
        public BookingModel? Booking { get; set; }

        [Required]
        [MaxLength(100)]
        public string GuestName { get; set; } = string.Empty;

        public int Age { get; set; }

        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(15)]
        public string GuestPhone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? GuestEmail { get; set; }

        // Aadhaar / PAN / Passport etc.
    
        public bool IsPrimary { get; set; }


        [NotMapped]
        public List<DocumentModel> Documents { get; set; } = new();
    }
}
