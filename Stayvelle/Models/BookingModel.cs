using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stayvelle.Models
{
    public class BookingModel :CommonModel
    {
        [Key]
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public RoomModel? Room { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string? RoomNumber { get; set; } // Snapshot of room number at time of booking
        public DateTime? ActualCheckInTime { get; set; }   // timer start
        public DateTime? ActualCheckOutTime { get; set; }  // timer stop
        public int NumberOfGuests { get; set; }
        // Hold / Booked / CheckedIn / CheckedOut / Cancelled
        public string BookingStatus { get; set; } = "Booked";

        // Navigation
        public ICollection<GuestDetailsModel> Guests { get; set; } = new List<GuestDetailsModel>();
        //public ICollection<BookingService> BookingServcies { get; set; } = new List<BookingService>();
    }

    public class BookingServiceDto
    {
        public int BookingServiceId { get; set; }   // PK
        public int BookingId { get; set; }           // FK → BookingModel.BookingId
        public int ServiceId { get; set; }           // FK → ServiceModel.ServiceId
        public int Quantity { get; set; }
        public DateTime ServiceDate { get; set; }
        public decimal ServicePriceAtThatTime { get; set; }
        public string ServiceStatus { get; set; }    // Requested / Completed / Cancelled
    }
}

