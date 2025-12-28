using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stayvelle.Models
{
    public class BookingModel :CommonModel
    {
        [Key]
        public int BookingId { get; set; }

        // 🔗 Room relation
        [Required]
        public int RoomId { get; set; }

        // Navigation property
        [ForeignKey("RoomId")]
        public RoomModel? Room { get; set; }

        // Reservation dates (planned)
        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        // ⏱ Actual stay timer
        public DateTime? ActualCheckInTime { get; set; }   // timer start
        public DateTime? ActualCheckOutTime { get; set; }  // timer stop

        public int NumberOfGuests { get; set; }

        // Hold / Booked / CheckedIn / CheckedOut / Cancelled
        [MaxLength(20)]
        public string BookingStatus { get; set; } = "Booked";

        // Audit
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedOn { get; set; }

        // Navigation
        public ICollection<GuestDetailsModel> Guests { get; set; } = new List<GuestDetailsModel>();
    }
}

