using Stayvelle.Models;
using Stayvelle.Query;

namespace Stayvelle.IRepository
{
    public interface IBooking
    {
        // Create
        Task<Response<BookingModel>> CreateBookingAsync(BookingModel booking);

        // Read
        Task<Response<List<BookingModel>>> GetAllBookingsAsync();
        Task<Response<BookingModel?>> GetBookingByIdAsync(int bookingId);
        Task<Response<BookingModel?>> GetBookingByGuestIdAsync(int guestId);
        Task<Response<BookingModel?>> GetBookingByGuestPhoneAsync(string phoneNumber);

        // Update
        Task<Response<BookingModel?>> UpdateBookingAsync(int bookingId, BookingModel booking);

        // Check-in and Check-out operations
        Task<Response<BookingModel?>> CheckInAsync(int bookingId);
        Task<Response<BookingModel?>> CheckOutAsync(int bookingId);

        // Delete
        Task<bool> DeleteBookingAsync(int bookingId);
    }
}

