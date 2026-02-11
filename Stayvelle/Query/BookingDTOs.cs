using Stayvelle.Models;
using Stayvelle.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Stayvelle.Query
{
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

