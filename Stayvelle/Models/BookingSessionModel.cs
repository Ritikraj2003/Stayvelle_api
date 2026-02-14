using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stayvelle.Models
{
    public class BookingSessionModel
    {
        [Key]
        public int SessionId { get; set; }
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
