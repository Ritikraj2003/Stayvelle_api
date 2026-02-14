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
        public string? AccessPin { get; set; } // Stores user phone number as pin
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
        public ICollection<BookingServiceModel> BookingServices { get; set; } = new List<BookingServiceModel>();
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

    public class BookingDetailDto
    {
        public int BookingId { get; set; }
        public string Status { get; set; }
        public RoomDto Room { get; set; }
        public List<GuestDto> Guests { get; set; } = new();
        public List<ServiceDto> Services { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }

    public class RoomDto
    {
        public string RoomNumber { get; set; }
        public string? RoomQrToken { get; set; }
        public decimal Price { get; set; }
    }

    public class GuestDto
    {
        public int GuestId { get; set; }
        public string Name { get; set; }
        public bool Primary { get; set; }
    }

    public class ServiceDto
    {
        public string Name { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
    }

    public class AddBookingServiceDto
    {
        public int BookingId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceCategory { get; set; }
        public string SubCategory { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public int Quantity { get; set; }
        public DateTime ServiceDate { get; set; }
        public string? ServiceStatus { get; set; }
    }
    
    public class CreateBookingDTO
    {
        public int RoomId { get; set; }
        public string? RoomNumber { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public List<CreateGuestDto> Guests { get; set; } = new List<CreateGuestDto>();
        public List<BookingServiceDto>? BookingServcies { get; set; } = new List<BookingServiceDto>();
    }

    public class CreateGuestDto
    {
        public string GuestName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string GuestPhone { get; set; } = string.Empty;
        public string? GuestEmail { get; set; }
        public bool IsPrimary { get; set; }
       public List<DocumentDto> Documents { get; set; } = new List<DocumentDto>();
    }
}

