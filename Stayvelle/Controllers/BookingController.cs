using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Query;

namespace Stayvelle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBooking _bookingRepository;
        private readonly IRoom _roomRepository;
        private readonly ApplicationDbContext _context;

        public BookingController(IBooking bookingRepository, IRoom roomRepository, ApplicationDbContext context)
        {
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
            _context = context;
        }

        // GET: api/Booking
        [HttpGet]
        public async Task<ActionResult<List<BookingModel>>> GetAllBookings()
        {
            var response = await _bookingRepository.GetAllBookingsAsync();
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message });
            }
            return Ok(response.Data);
        }

        // GET: api/Booking/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingModel>> GetBooking(int id)
        {
            var response = await _bookingRepository.GetBookingByIdAsync(id);
            if (!response.Success || response.Data == null)
            {
                return NotFound(new { message = response.Message ?? $"Booking with ID {id} not found" });
            }
            return Ok(response.Data);
        }

        // GET: api/Booking/guest/{guestId}
        [HttpGet("guest/{guestId}")]
        public async Task<ActionResult<BookingModel>> GetBookingByGuestId(int guestId)
        {
            var response = await _bookingRepository.GetBookingByGuestIdAsync(guestId);
            if (!response.Success || response.Data == null)
            {
                return NotFound(new { message = response.Message ?? $"Booking for guest ID {guestId} not found" });
            }
            return Ok(response.Data);
        }

        // GET: api/Booking/phone/{phoneNumber}
        [HttpGet("phone/{phoneNumber}")]
        public async Task<ActionResult<BookingModel>> GetBookingByGuestPhone(string phoneNumber)
        {
            var response = await _bookingRepository.GetBookingByGuestPhoneAsync(phoneNumber);
            if (!response.Success || response.Data == null)
            {
                return NotFound(new { message = response.Message ?? $"Booking for phone number {phoneNumber} not found" });
            }
            return Ok(response.Data);
        }

        // POST: api/Booking
        [HttpPost]
        public async Task<ActionResult<BookingModel>> CreateBooking([FromBody] CreateBookingDTO createBookingDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate guests
            if (createBookingDTO.Guests == null || !createBookingDTO.Guests.Any())
            {
                return BadRequest(new { message = "At least one guest is required" });
            }

            // Check if at least one guest is primary
            if (!createBookingDTO.Guests.Any(g => g.IsPrimary))
            {
                return BadRequest(new { message = "At least one guest must be marked as primary" });
            }

            // Validate room exists and is available
            var roomResponse = await _roomRepository.GetRoomByIdAsync(createBookingDTO.RoomId);
            if (!roomResponse.Success || roomResponse.Data == null)
            {
                return NotFound(new { message = "Room not found" });
            }

            var room = roomResponse.Data;
            if (room.RoomStatus != "Available")
            {
                return BadRequest(new { message = $"Room is not available. Current status: {room.RoomStatus}" });
            }

            // Create booking - ensure all DateTime values are UTC
            var checkInDate = createBookingDTO.CheckInDate.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(createBookingDTO.CheckInDate, DateTimeKind.Utc) 
                : createBookingDTO.CheckInDate.ToUniversalTime();
            
            var checkOutDate = createBookingDTO.CheckOutDate.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(createBookingDTO.CheckOutDate, DateTimeKind.Utc) 
                : createBookingDTO.CheckOutDate.ToUniversalTime();

            var booking = new BookingModel
            {
                RoomId = createBookingDTO.RoomId,
                CheckInDate = checkInDate,
                ActualCheckInTime=DateTime.UtcNow,
                CheckOutDate = checkOutDate,
                NumberOfGuests = createBookingDTO.NumberOfGuests,
                BookingStatus = "Booked",
                CreatedBy = "system", // You can get this from auth service
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = string.Empty,
                ModifiedOn = null
            };

            // Create guests
            foreach (var guestDto in createBookingDTO.Guests)
            {
                var guest = new GuestDetailsModel
                {
                    GuestName = guestDto.GuestName,
                    Age = guestDto.Age,
                    Gender = guestDto.Gender,
                    GuestPhone = guestDto.GuestPhone,
                    GuestEmail = guestDto.GuestEmail,
                    IdProof = guestDto.IdProof,
                    IdProofImagePath = guestDto.IdProofImage, // Store base64 for now
                    IsPrimary = guestDto.IsPrimary,
                    CreatedBy = "system", // You can get this from auth service
                    CreatedOn = DateTime.UtcNow,
                    ModifiedBy = string.Empty,
                    ModifiedOn = null
                };
                booking.Guests.Add(guest);
            }

            // Create booking
            var bookingResponse = await _bookingRepository.CreateBookingAsync(booking);
            if (!bookingResponse.Success || bookingResponse.Data == null)
            {
                return BadRequest(new { message = bookingResponse.Message });
            }

            // Update room status to "Occupied"
            room.RoomStatus = "Occupied";
            var updateRoom = new RoomModel
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                Price = room.Price,
                MaxOccupancy = room.MaxOccupancy,
                Floor = room.Floor,
                NumberOfBeds = room.NumberOfBeds,
                AcType = room.AcType,
                BathroomType = room.BathroomType,
                RoomStatus = "Occupied", // Change status
                RoomType = room.RoomType,
                IsActive = room.IsActive,
                Description = room.Description,
                IsTv = room.IsTv,
                Images = room.Images
            };

            var roomUpdateResponse = await _roomRepository.UpdateRoomAsync(room.Id, updateRoom);
            if (!roomUpdateResponse.Success)
            {
                // Log warning but don't fail the booking
                Console.WriteLine($"Warning: Failed to update room status for room {room.Id}");
            }

            return CreatedAtAction(nameof(GetBooking), new { id = bookingResponse.Data.BookingId }, bookingResponse.Data);
        }

        // PUT: api/Booking/5
        [HttpPut("{id}")]
        public async Task<ActionResult<BookingModel>> UpdateBooking(int id, [FromBody] BookingModel booking)
        {
            var existingBookingResponse = await _bookingRepository.GetBookingByIdAsync(id);
            if (!existingBookingResponse.Success || existingBookingResponse.Data == null)
            {
                return NotFound(new { message = existingBookingResponse.Message ?? $"Booking with ID {id} not found" });
            }

            booking.BookingId = id;
            booking.ModifiedOn = DateTime.UtcNow;

            var updatedBookingResponse = await _bookingRepository.UpdateBookingAsync(id, booking);
            if (!updatedBookingResponse.Success || updatedBookingResponse.Data == null)
            {
                return BadRequest(new { message = updatedBookingResponse.Message ?? $"Error updating booking with ID {id}" });
            }

            return Ok(updatedBookingResponse.Data);
        }

        // POST: api/Booking/5/checkin
        [HttpPost("{id}/checkin")]
        public async Task<ActionResult<BookingModel>> CheckIn(int id)
        {
            var response = await _bookingRepository.CheckInAsync(id);
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message ?? $"Error checking in booking {id}" });
            }
            return Ok(response.Data);
        }

        // POST: api/Booking/5/checkout
        [HttpPost("{id}/checkout")]
        public async Task<ActionResult<BookingModel>> CheckOut(int id)
        {
            var response = await _bookingRepository.CheckOutAsync(id);
            if (!response.Success || response.Data == null)
            {
                return BadRequest(new { message = response.Message ?? $"Error checking out booking {id}" });
            }
            return Ok(response.Data);
        }

        // DELETE: api/Booking/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBooking(int id)
        {
            // Get booking to find room
            var bookingResponse = await _bookingRepository.GetBookingByIdAsync(id);
            if (!bookingResponse.Success || bookingResponse.Data == null)
            {
                return NotFound(new { message = $"Booking with ID {id} not found" });
            }

            var booking = bookingResponse.Data;

            // Delete booking
            var result = await _bookingRepository.DeleteBookingAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Booking with ID {id} not found" });
            }

            // Update room status back to "Available" if booking was deleted
            if (booking.RoomId > 0)
            {
                var roomResponse = await _roomRepository.GetRoomByIdAsync(booking.RoomId);
                if (roomResponse.Success && roomResponse.Data != null)
                {
                    var room = roomResponse.Data;
                    var updateRoom = new RoomModel
                    {
                        Id = room.Id,
                        RoomNumber = room.RoomNumber,
                        Price = room.Price,
                        MaxOccupancy = room.MaxOccupancy,
                        Floor = room.Floor,
                        NumberOfBeds = room.NumberOfBeds,
                        AcType = room.AcType,
                        BathroomType = room.BathroomType,
                        RoomStatus = "Available", // Change back to Available
                        RoomType = room.RoomType,
                        IsActive = room.IsActive,
                        Description = room.Description,
                        IsTv = room.IsTv,
                        Images = room.Images
                    };
                    await _roomRepository.UpdateRoomAsync(room.Id, updateRoom);
                }
            }

            return Ok(new { message = $"Booking with ID {id} deleted successfully" });
        }
    }
}

