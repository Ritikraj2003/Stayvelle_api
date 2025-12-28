using System.ComponentModel.DataAnnotations;

namespace Stayvelle.Query
{
    public class CreateBookingDTO
    {
        [Required]
        public int RoomId { get; set; }

        public string? RoomNumber { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "NumberOfGuests must be at least 1")]
        public int NumberOfGuests { get; set; }

        [Required]
        public List<CreateGuestDTO> Guests { get; set; } = new List<CreateGuestDTO>();
    }

    public class CreateGuestDTO
    {
        [Required]
        [MaxLength(100)]
        public string GuestName { get; set; } = string.Empty;

        [Required]
        [Range(1, 150, ErrorMessage = "Age must be between 1 and 150")]
        public int Age { get; set; }

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string GuestPhone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? GuestEmail { get; set; }

        [Required]
        [MaxLength(50)]
        public string IdProof { get; set; } = string.Empty;

        public string? IdProofImage { get; set; } // base64 string

        public bool IsPrimary { get; set; } = false;
    }
}

