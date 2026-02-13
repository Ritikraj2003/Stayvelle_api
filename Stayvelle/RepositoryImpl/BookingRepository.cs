using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Models.DTOs;
using Stayvelle.Query;
using Stayvelle.Services;
using Microsoft.Extensions.Configuration;

namespace Stayvelle.RepositoryImpl
{
    public class BookingRepository : IBooking
    {
        private readonly ApplicationDbContext _context;
        private readonly IHousekeepingTask _housekeepingTaskRepository;
        private readonly IConfiguration _configuration;

        public BookingRepository(ApplicationDbContext context, IHousekeepingTask housekeepingTaskRepository, IConfiguration configuration)
        {
            _context = context;
            _housekeepingTaskRepository = housekeepingTaskRepository;
            _configuration = configuration;
        }

        // Create
        public async Task<Response<BookingModel>> CreateBookingAsync(CreateBookingDTO bookingDto)
        {
            Response<BookingModel> response = new Response<BookingModel>();
            
            // 1. Validate room exists
            var room = await _context.RoomModel.FindAsync(bookingDto.RoomId);
            if (room == null)
            {
                response.Success = false;
                response.Message = "Room not found";
                return response;
            }

            // 2. Business Rule: Check if room is available
            if (room.RoomStatus != "Available")
            {
                response.Success = false;
                response.Message = $"Room is not available. Current status: {room.RoomStatus}";
                return response;
            }

            // 3. Business Rule: Validate dates
            if (bookingDto.CheckOutDate <= bookingDto.CheckInDate)
            {
                response.Success = false;
                response.Message = "Check-out date must be after check-in date";
                return response;
            }

            // Ensure dates are UTC
            var checkInDate = bookingDto.CheckInDate.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(bookingDto.CheckInDate, DateTimeKind.Utc) 
                : bookingDto.CheckInDate.ToUniversalTime();
            
            var checkOutDate = bookingDto.CheckOutDate.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(bookingDto.CheckOutDate, DateTimeKind.Utc) 
                : bookingDto.CheckOutDate.ToUniversalTime();


            try
            {
                // 4. Insert Booking using Dapper
                var connection = _context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open) await connection.OpenAsync();

                var bookingParams = new
                {
                    RoomId = bookingDto.RoomId,
                    CheckInDate = checkInDate,
                    CheckOutDate = checkOutDate,
                    NumberOfGuests = bookingDto.NumberOfGuests,
                    BookingStatus = "Booked",
                    CreatedBy = "system",
                    CreatedOn = DateTime.UtcNow,
                    ActualCheckInTime = DateTime.UtcNow,
                    ActualCheckOutTime = (DateTime?)null,
                    RoomNumber = room.RoomNumber ?? bookingDto.RoomNumber 
                };

                int bookingId = await connection.ExecuteScalarAsync<int>(BookingQuery.InsertBooking, bookingParams);

                // 5. Insert Guests
                if (bookingDto.Guests != null)
                {
                    foreach (var guestDto in bookingDto.Guests)
                    {
                        var guestParams = new
                        {
                            BookingId = bookingId,
                            GuestName = guestDto.GuestName,
                            Age = guestDto.Age,
                            Gender = guestDto.Gender,
                            GuestPhone = guestDto.GuestPhone,
                            GuestEmail = guestDto.GuestEmail,
                            CreatedBy = "system",
                            IsPrimary= true,
                            CreatedOn = DateTime.UtcNow
                        };

                        int guestId = await connection.ExecuteScalarAsync<int>(BookingQuery.InsertGuest, guestParams);

                        // Insert Documents for this guest
                        if (guestDto.Documents != null && guestDto.Documents.Any())
                        {
                            foreach (var doc in guestDto.Documents)
                            {

                                string baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7252";
                                string filePath = await Uploads.UploadImage(doc.FileName, doc.File, doc.EntityType, baseUrl);
                                var docModel = new
                                {
                                    EntityType = "GUEST",
                                    EntityId = guestId,
                                    DocumentType = doc.DocumentType ?? "ID_PROOF",
                                    FileName = doc.FileName,
                                    Description = (string?)null,
                                    FilePath = filePath,
                                    IsPrimary = doc.IsPrimary,
                                    CreatedBy = "system",
                                    CreatedOn = DateTime.UtcNow
                                };

                                await connection.ExecuteScalarAsync<int>(BookingQuery.InsertDocument, docModel);
                            }
                        }
                    }
                }

                // 6. Insert Services
                if (bookingDto.BookingServcies != null)
                {
                    foreach (var serviceDto in bookingDto.BookingServcies)
                    {
                        // Fetch service entity for name
                        var serviceEntity = await connection.QueryFirstOrDefaultAsync<ServiceModel>(
                            "SELECT * FROM \"ServiceModel\" WHERE \"ServiceId\" = @ServiceId", 
                            new { ServiceId = serviceDto.ServiceId }
                        );

                        var serviceName = serviceEntity?.ServiceName ?? "Unknown Service";
                        
                        var serviceModel = new
                        {
                            BookingId = bookingId,
                            ServiceId = serviceDto.ServiceId,
                            ServiceName = serviceName, // Populated from DB
                            Price = serviceDto.ServicePriceAtThatTime,
                            Quantity = serviceDto.Quantity,
                            ServiceDate = serviceDto.ServiceDate,
                            ServiceStatus = serviceDto.ServiceStatus ?? "Requested",
                            CreatedOn = DateTime.UtcNow
                        };

                        await connection.ExecuteScalarAsync<int>(BookingQuery.InsertBookingService, serviceModel);
                    }
                }

                // 7. Business Logic: Update Room Status
                // We use the same transaction via EF Core
                room.RoomStatus = "Occupied";
                room.ModifiedOn = DateTime.UtcNow;
                room.ModifiedBy = "system";

                // Ensure CreatedOn is UTC to satisfy Npgsql
                if (room.CreatedOn.Kind == DateTimeKind.Unspecified)
                {
                    room.CreatedOn = DateTime.SpecifyKind(room.CreatedOn, DateTimeKind.Utc);
                }

                _context.RoomModel.Update(room); 
                await _context.SaveChangesAsync();

                // Reload booking with related data for response
                var savedBooking = await _context.BookingModel
                    .Include(b => b.Guests)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                response.Success = true;
                response.Message = "Booking created successfully";
                response.Data = savedBooking; // Note: savedBooking might be null if transaction isolation/delay? unlikely awaiting commit.

                return response;
            }
            catch (Exception ex)
            {
                // Log error if needed
                response.Success = false;
                response.Message = $"Error creating booking: {ex.Message}";
                if (ex.InnerException != null)
                {
                    response.Message += $" - Inner: {ex.InnerException.Message}";
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

                if (bookings != null && bookings.Any())
                {
                    // Collect all Guest IDs
                    var guestIds = bookings.SelectMany(b => b.Guests).Select(g => g.GuestId).ToList();

                    if (guestIds.Any())
                    {
                        // Fetch all documents for these guests
                        var documents = await _context.DocumentModel
                            .Where(d => d.EntityType == "GUEST" && guestIds.Contains(d.EntityId))
                            .ToListAsync();

                        // Map documents to guests
                        foreach (var booking in bookings)
                        {
                            foreach (var guest in booking.Guests)
                            {
                                guest.Documents = documents.Where(d => d.EntityId == guest.GuestId).ToList();
                            }
                        }
                    }
                }

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

                if (booking.Guests != null && booking.Guests.Any())
                {
                    var guestIds = booking.Guests.Select(g => g.GuestId).ToList();
                    var documents = await _context.DocumentModel
                        .Where(d => d.EntityType == "GUEST" && guestIds.Contains(d.EntityId))
                        .ToListAsync();

                    foreach (var guest in booking.Guests)
                    {
                        guest.Documents = documents.Where(d => d.EntityId == guest.GuestId).ToList();
                    }
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

                if (guest.Booking.Guests != null && guest.Booking.Guests.Any())
                {
                    var guestIds = guest.Booking.Guests.Select(g => g.GuestId).ToList();
                    var documents = await _context.DocumentModel
                        .Where(d => d.EntityType == "GUEST" && guestIds.Contains(d.EntityId))
                        .ToListAsync();

                    foreach (var g in guest.Booking.Guests)
                    {
                        g.Documents = documents.Where(d => d.EntityId == g.GuestId).ToList();
                    }
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

                if (guest.Booking.Guests != null && guest.Booking.Guests.Any())
                {
                    var guestIds = guest.Booking.Guests.Select(g => g.GuestId).ToList();
                    var documents = await _context.DocumentModel
                        .Where(d => d.EntityType == "GUEST" && guestIds.Contains(d.EntityId))
                        .ToListAsync();

                    foreach (var g in guest.Booking.Guests)
                    {
                        g.Documents = documents.Where(d => d.EntityId == g.GuestId).ToList();
                    }
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

        // Read - Get Booking By Room
        public async Task<Response<BookingModel?>> GetBookingByRoomAsync(int roomId, string roomNumber)
        {
            var response = new Response<BookingModel?>();
            try
            {
                var booking = await _context.BookingModel
                    .Include(b => b.Room)
                    .Include(b => b.Guests)
                    .Where(b => b.RoomId == roomId && b.RoomNumber == roomNumber)
                    .OrderByDescending(b => b.CreatedOn)
                    .FirstOrDefaultAsync();

                if (booking == null)
                {
                    response.Success = false;
                    response.Message = "Booking not found using RoomId and RoomNumber";
                    response.Data = null;
                    return response;
                }

                if (booking.Guests != null && booking.Guests.Any())
                {
                    var guestIds = booking.Guests.Select(g => g.GuestId).ToList();
                    var documents = await _context.DocumentModel
                        .Where(d => d.EntityType == "GUEST" && guestIds.Contains(d.EntityId))
                        .ToListAsync();

                    foreach (var g in booking.Guests)
                    {
                        g.Documents = documents.Where(d => d.EntityId == g.GuestId).ToList();
                    }
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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = await _context.BookingModel
                    .Include(b => b.Guests)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    return false;
                }

                // Business Logic: Reset Room Status to Available
                if (booking.RoomId > 0)
                {
                    var room = await _context.RoomModel.FindAsync(booking.RoomId);
                    if (room != null)
                    {
                        room.RoomStatus = "Available";
                        _context.RoomModel.Update(room);
                    }
                }

                // Delete guests first 
                // Cascade delete might handle this but manual removal is safe explicit logic
                _context.GuestDetailsModel.RemoveRange(booking.Guests);
                
                // Delete booking
                _context.BookingModel.Remove(booking);
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
        public async Task<Response<BookingDetailDto?>> GetBookingDetailsAsync(int bookingId)
        {
            var response = new Response<BookingDetailDto?>();
            try
            {
                var bookingDetail = await _context.BookingModel
                    .Where(b => b.BookingId == bookingId)
                    .Select(b => new BookingDetailDto
                    {
                        BookingId = b.BookingId,
                        Status = b.BookingStatus,
                        Room = new RoomDto
                        {
                            RoomNumber = b.Room.RoomNumber,
                            Price = b.Room.Price
                        },
                        Guests = b.Guests.Select(g => new GuestDto
                        {
                            GuestId = g.GuestId,
                            Name = g.GuestName,
                            Primary = g.IsPrimary
                        }).ToList(),
                        Services = _context.BookingServiceModel
                            .Where(bs => bs.BookingId == b.BookingId)
                            .Select(bs => new ServiceDto
                            {
                                Name = bs.ServiceName,
                                Qty = bs.Quantity,
                                Price = bs.Price
                            }).ToList(),
                        // Calculate total amount: Room Price (per day?) + Services
                        // Assuming Room Price is per night. Need logic for nights.
                        // User requirement says: "totalAmount": 5500. 
                        // Let's implement basic calculation: (Room Price * Nights) + Service Costs
                        // Note: CheckOutDate - CheckInDate gives duration.
                        TotalAmount = (decimal)((b.CheckOutDate - b.CheckInDate).Days == 0 ? 1 : (b.CheckOutDate - b.CheckInDate).Days) * b.Room.Price 
                                      + _context.BookingServiceModel
                                          .Where(bs => bs.BookingId == b.BookingId)
                                          .Sum(bs => bs.Quantity * bs.Price)
                    })
                    .FirstOrDefaultAsync();

                if (bookingDetail == null)
                {
                    response.Success = false;
                    response.Message = "Booking not found";
                    return response;
                }

                response.Success = true;
                response.Data = bookingDetail;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
        }
    }
}

