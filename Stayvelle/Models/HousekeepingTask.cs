using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stayvelle.Models
{
    public class HousekeepingTask : CommonModel
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [MaxLength(50)]
        public string TaskType { get; set; } = "Cleaning";

        [MaxLength(50)]
        public string TaskStatus { get; set; } = "Pending"; // Pending | Assigned | InProgress | Completed

        public string? RoomImage { get; set; } // JSON string or comma-separated image paths/URLs

        public int? AssignedToUserId { get; set; }

        // Navigation properties
        [ForeignKey("RoomId")]
        public RoomModel? Room { get; set; }

        [ForeignKey("BookingId")]
        public BookingModel? Booking { get; set; }
    }
}

