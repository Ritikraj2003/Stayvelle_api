using Microsoft.EntityFrameworkCore;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Query;

namespace Stayvelle.RepositoryImpl
{
    public class BookingRepository : IBooking
    {
        private readonly ApplicationDbContext _context;
        private readonly IHousekeepingTask _housekeepingTaskRepository;

        public BookingRepository(ApplicationDbContext context, IHousekeepingTask housekeepingTaskRepository)
        {
            _context = context;
            _housekeepingTaskRepository = housekeepingTaskRepository;
        }

        // Create
        public async Task<Response<BookingModel>> CreateBookingAsync(BookingModel booking)
        {
            Response<BookingModel> response = new Response<BookingModel>();
            try
            {
                // Validate room exists
                var room = await _context.RoomModel.FindAsync(booking.RoomId);
                if (room == null)
                {
                    response.Success = false;
                    response.Message = "Room not found";
                    response.Data = null;
                    return response;
                }

                // Validate check-in and check-out dates
                if (booking.CheckOutDate <= booking.CheckInDate)
                {
                    response.Success = false;
                    response.Message = "Check-out date must be after check-in date";
                    response.Data = null;
                    return response;
                }

                // Set CommonModel fields if not already set
                if (string.IsNullOrEmpty(booking.CreatedBy))
                {
                    booking.CreatedBy = "system";
                }
                if (booking.CreatedOn == default(DateTime))
                {
                    booking.CreatedOn = DateTime.UtcNow;
                }
                else
                {
                    // Ensure CreatedOn is UTC
                    booking.CreatedOn = booking.CreatedOn.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(booking.CreatedOn, DateTimeKind.Utc) 
                        : booking.CreatedOn.ToUniversalTime();
                }
                
                // Ensure CheckInDate and CheckOutDate are UTC
                booking.CheckInDate = booking.CheckInDate.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(booking.CheckInDate, DateTimeKind.Utc) 
                    : booking.CheckInDate.ToUniversalTime();
                
                booking.CheckOutDate = booking.CheckOutDate.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(booking.CheckOutDate, DateTimeKind.Utc) 
                    : booking.CheckOutDate.ToUniversalTime();
                
                // Ensure nullable DateTime fields are UTC if set
                if (booking.ActualCheckInTime.HasValue)
                {
                    booking.ActualCheckInTime = booking.ActualCheckInTime.Value.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(booking.ActualCheckInTime.Value, DateTimeKind.Utc) 
                        : booking.ActualCheckInTime.Value.ToUniversalTime();
                }
                
                if (booking.ActualCheckOutTime.HasValue)
                {
                    booking.ActualCheckOutTime = booking.ActualCheckOutTime.Value.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(booking.ActualCheckOutTime.Value, DateTimeKind.Utc) 
                        : booking.ActualCheckOutTime.Value.ToUniversalTime();
                }
                
                booking.BookingStatus = booking.BookingStatus ?? "Booked";

                // Set CommonModel fields for guests
                foreach (var guest in booking.Guests)
                {
                    if (string.IsNullOrEmpty(guest.CreatedBy))
                    {
                        guest.CreatedBy = "system";
                    }
                    if (guest.CreatedOn == default(DateTime))
                    {
                        guest.CreatedOn = DateTime.UtcNow;
                    }
                    else
                    {
                        // Ensure CreatedOn is UTC
                        guest.CreatedOn = guest.CreatedOn.Kind == DateTimeKind.Unspecified 
                            ? DateTime.SpecifyKind(guest.CreatedOn, DateTimeKind.Utc) 
                            : guest.CreatedOn.ToUniversalTime();
                    }
                }

                // Add booking and guests
                _context.BookingModel.Add(booking);
                await _context.SaveChangesAsync();

                // Reload booking with related data
                var savedBooking = await _context.BookingModel
                    .Include(b => b.Room)
                    .Include(b => b.Guests)
                    .FirstOrDefaultAsync(b => b.BookingId == booking.BookingId);

                response.Success = true;
                response.Message = "Booking created successfully";
                response.Data = savedBooking ?? booking;

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                // Get inner exception message if available for more details
                var errorMessage = ex.InnerException != null 
                    ? $"{ex.Message} - Inner: {ex.InnerException.Message}" 
                    : ex.Message;
                response.Message = errorMessage;
                response.Data = null;
                
                // Log full exception for debugging
                Console.WriteLine($"Error creating booking: {ex}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException}");
                }
                
                return response;
            }
        }

        // Read - Get All Bookings
        public async Task<Response<List<BookingModel>>> GetAllBookingsAsync()
        {
            var response = new Response<List<BookingModel>>();
            try
            {
                var bookings = await _context.BookingModel
                    .Include(b => b.Room)
                    .Include(b => b.Guests)
                    .OrderByDescending(b => b.CreatedOn)
                    .ToListAsync();

                response.Success = true;
                response.Message = "success";
                response.Data = bookings;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        // Read - Get Booking By Id
        public async Task<Response<BookingModel?>> GetBookingByIdAsync(int bookingId)
        {
            var response = new Response<BookingModel?>();
            try
            {
                var booking = await _context.BookingModel
                    .Include(b => b.Room)
                    .Include(b => b.Guests)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    response.Success = false;
                    response.Message = "Booking not found";
                    response.Data = null;
                    return response;
                }

                response.Success = true;
                response.Message = "Success";
                response.Data = booking;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        // Read - Get Booking By Guest Id
        public async Task<Response<BookingModel?>> GetBookingByGuestIdAsync(int guestId)
        {
            var response = new Response<BookingModel?>();
            try
            {
                var guest = await _context.GuestDetailsModel
                    .Include(g => g.Booking)
                        .ThenInclude(b => b!.Room)
                    .Include(g => g.Booking)
                        .ThenInclude(b => b!.Guests)
                    .FirstOrDefaultAsync(g => g.GuestId == guestId);

                if (guest == null || guest.Booking == null)
                {
                    response.Success = false;
                    response.Message = "Guest or booking not found";
                    response.Data = null;
                    return response;
                }

                response.Success = true;
                response.Message = "Success";
                response.Data = guest.Booking;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        // Read - Get Booking By Guest Phone
        public async Task<Response<BookingModel?>> GetBookingByGuestPhoneAsync(string phoneNumber)
        {
            var response = new Response<BookingModel?>();
            try
            {
                var guest = await _context.GuestDetailsModel
                    .Include(g => g.Booking)
                        .ThenInclude(b => b!.Room)
                    .Include(g => g.Booking)
                        .ThenInclude(b => b!.Guests)
                    .FirstOrDefaultAsync(g => g.GuestPhone == phoneNumber);

                if (guest == null || guest.Booking == null)
                {
                    response.Success = false;
                    response.Message = "Guest or booking not found";
                    response.Data = null;
                    return response;
                }

                response.Success = true;
                response.Message = "Success";
                response.Data = guest.Booking;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        // Update
        public async Task<Response<BookingModel?>> UpdateBookingAsync(int bookingId, BookingModel booking)
        {
            var response = new Response<BookingModel?>();
            try
            {
                var existingBooking = await _context.BookingModel
                    .Include(b => b.Guests)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (existingBooking == null)
                {
                    response.Success = false;
                    response.Message = "Booking not found";
                    response.Data = null;
                    return response;
                }

                // Update booking properties - ensure all DateTime values are UTC
                existingBooking.RoomId = booking.RoomId;
                
                existingBooking.CheckInDate = booking.CheckInDate.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(booking.CheckInDate, DateTimeKind.Utc) 
                    : booking.CheckInDate.ToUniversalTime();
                
                existingBooking.CheckOutDate = booking.CheckOutDate.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(booking.CheckOutDate, DateTimeKind.Utc) 
                    : booking.CheckOutDate.ToUniversalTime();
                
                if (booking.ActualCheckInTime.HasValue)
                {
                    existingBooking.ActualCheckInTime = booking.ActualCheckInTime.Value.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(booking.ActualCheckInTime.Value, DateTimeKind.Utc) 
                        : booking.ActualCheckInTime.Value.ToUniversalTime();
                }
                else
                {
                    existingBooking.ActualCheckInTime = null;
                }
                
                if (booking.ActualCheckOutTime.HasValue)
                {
                    existingBooking.ActualCheckOutTime = booking.ActualCheckOutTime.Value.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(booking.ActualCheckOutTime.Value, DateTimeKind.Utc) 
                        : booking.ActualCheckOutTime.Value.ToUniversalTime();
                }
                else
                {
                    existingBooking.ActualCheckOutTime = null;
                }
                
                existingBooking.NumberOfGuests = booking.NumberOfGuests;
                existingBooking.BookingStatus = booking.BookingStatus;
                existingBooking.ModifiedBy = booking.ModifiedBy ?? existingBooking.ModifiedBy;
                existingBooking.ModifiedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Reload with includes
                await _context.Entry(existingBooking)
                    .Reference(b => b.Room)
                    .LoadAsync();
                await _context.Entry(existingBooking)
                    .Collection(b => b.Guests)
                    .LoadAsync();

                response.Success = true;
                response.Message = "Booking updated successfully";
                response.Data = existingBooking;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error while updating booking: " + ex.Message;
                response.Data = null;
                return response;
            }
        }

        // Check-in operation
        public async Task<Response<BookingModel?>> CheckInAsync(int bookingId)
        {
            var response = new Response<BookingModel?>();
            try
            {
                var booking = await _context.BookingModel
                    .Include(b => b.Room)
                    .Include(b => b.Guests)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    response.Success = false;
                    response.Message = "Booking not found";
                    response.Data = null;
                    return response;
                }

                // Set actual check-in time
                booking.ActualCheckInTime = DateTime.UtcNow;
                booking.BookingStatus = "CheckedIn";
                booking.ModifiedOn = DateTime.UtcNow;

                // Update room status to "Occupied" if not already
                if (booking.Room != null && booking.Room.RoomStatus != "Occupied")
                {
                    booking.Room.RoomStatus = "Occupied";
                }

                await _context.SaveChangesAsync();

                response.Success = true;
                response.Message = "Check-in successful";
                response.Data = booking;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error during check-in: " + ex.Message;
                response.Data = null;
                return response;
            }
        }

        // Check-out operation
        public async Task<Response<BookingModel?>> CheckOutAsync(int bookingId)
        {
            var response = new Response<BookingModel?>();
            try
            {
                var booking = await _context.BookingModel
                    .Include(b => b.Room)
                    .Include(b => b.Guests)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    response.Success = false;
                    response.Message = "Booking not found";
                    response.Data = null;
                    return response;
                }

                // Set actual check-out time
                booking.ActualCheckOutTime = DateTime.UtcNow;
                booking.BookingStatus = "CheckedOut";
                booking.ModifiedOn = DateTime.UtcNow;

                // Update room status to "Maintenance"
                if (booking.Room != null)
                {
                    booking.Room.RoomStatus = "Maintenance";
                }

                await _context.SaveChangesAsync();

                // Create housekeeping task automatically
                if (booking.Room != null)
                {
                    var housekeepingTask = new HousekeepingTask
                    {
                        RoomId = booking.RoomId,
                        BookingId = bookingId,
                        TaskType = "Cleaning",
                        TaskStatus = "Pending",
                        CreatedBy = "system",
                        CreatedOn = DateTime.UtcNow
                    };

                    var taskResponse = await _housekeepingTaskRepository.CreateHousekeepingTaskAsync(housekeepingTask);
                    if (!taskResponse.Success)
                    {
                        // Log warning but don't fail the checkout
                        Console.WriteLine($"Warning: Failed to create housekeeping task for booking {bookingId}: {taskResponse.Message}");
                    }
                }

                response.Success = true;
                response.Message = "Check-out successful";
                response.Data = booking;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error during check-out: " + ex.Message;
                response.Data = null;
                return response;
            }
        }

        // Delete
        public async Task<bool> DeleteBookingAsync(int bookingId)
        {
            try
            {
                var booking = await _context.BookingModel
                    .Include(b => b.Guests)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    return false;
                }

                // Delete guests first
                _context.GuestDetailsModel.RemoveRange(booking.Guests);
                
                // Delete booking
                _context.BookingModel.Remove(booking);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

