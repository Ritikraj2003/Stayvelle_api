using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Models.DTOs;
using Stayvelle.Query;

namespace Stayvelle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBooking _bookingRepository;

        public BookingController(IBooking bookingRepository)
        {
            _bookingRepository = bookingRepository;
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

        // GET: api/Booking/room/{roomId}/{roomNumber}
        [HttpGet("room/{roomId}/{roomNumber}")]
        public async Task<ActionResult<BookingModel>> GetBookingByRoom(int roomId, string roomNumber)
        {
            var response = await _bookingRepository.GetBookingByRoomAsync(roomId, roomNumber);
            if (!response.Success || response.Data == null)
            {
                return NotFound(new { message = response.Message ?? $"Booking for Room ID {roomId} and Number {roomNumber} not found" });
            }
            return Ok(response.Data);
        }

        // POST: api/Booking
        [HttpPost]
        public async Task<ActionResult<BookingModel>> CreateBooking([FromForm] CreateBookingDTO createBookingDTO)
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

            // Business logic delegated to Repository
            var bookingResponse = await _bookingRepository.CreateBookingAsync(createBookingDTO);
            
            if (!bookingResponse.Success || bookingResponse.Data == null)
            {
                // Determine if it was a Not Found or Bad Request based on message roughly, 
                // but usually Bad Request for business rule violations
                if (bookingResponse.Message.Contains("not found"))
                {
                    return NotFound(new { message = bookingResponse.Message });
                }
                return BadRequest(new { message = bookingResponse.Message });
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
            // Delete booking (Logic for resetting room status is in Repository)
            var result = await _bookingRepository.DeleteBookingAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Booking with ID {id} not found or could not be deleted" });
            }

            return Ok(new { message = $"Booking with ID {id} deleted successfully" });
        }
        // POST: api/Booking/InsertDataByBookingId
        [HttpPost("InsertDataByBookingId")]
        public async Task<ActionResult> InsertDataByBookingId([FromBody] List<AddBookingServiceDto> addServiceDtos)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _bookingRepository.AddServiceToBookingAsync(addServiceDtos);
            
            if (!response.Success)
            {
                if (response.Message.Contains("not found"))
                {
                    return NotFound(new { message = response.Message });
                }
                return BadRequest(new { message = response.Message });
            }

            return Ok(new { message = response.Message, data = response.Data });
        }
    }
}

