using Stayvelle.Models;
using Stayvelle.Models;


namespace Stayvelle.IRepository
{
    public interface IBooking
    {
        // Create
        Task<Response<BookingModel>> CreateBookingAsync(CreateBookingDTO bookingDto);

        // Read
        Task<Response<List<BookingModel>>> GetAllBookingsAsync();
        Task<Response<BookingModel?>> GetBookingByIdAsync(int bookingId);
        Task<Response<BookingModel?>> GetBookingByGuestIdAsync(int guestId);
        Task<Response<BookingModel?>> GetBookingByGuestPhoneAsync(string phoneNumber);
        Task<Response<BookingModel?>> GetBookingByRoomAsync(int roomId, string roomNumber);

        // Update
        Task<Response<BookingModel?>> UpdateBookingAsync(int bookingId, BookingModel booking);

        // Check-in and Check-out operations
        Task<Response<BookingModel?>> CheckInAsync(int bookingId);
        Task<Response<BookingModel?>> CheckOutAsync(int bookingId);

        // Extended Details
        Task<Response<BookingDetailDto?>> GetBookingDetailsAsync(int bookingId);

        // Delete
        Task<bool> DeleteBookingAsync(int bookingId);

        // Add Service to Booking
        Task<Response<bool>> AddServiceToBookingAsync(List<AddBookingServiceDto> addServiceDtos);
    }
}

