namespace Stayvelle.Query
{
    public static class BookingQuery
    {
        public static string InsertBooking = @"
            INSERT INTO ""BookingModel"" 
            (
                ""RoomId"", 
                ""CheckInDate"", 
                ""CheckOutDate"", 
                ""NumberOfGuests"", 
                ""BookingStatus"", 
                ""CreatedBy"", 
                ""CreatedOn"", 
                ""ActualCheckInTime"",
                ""ActualCheckOutTime"",
                ""RoomNumber"",
                ""AccessPin""
            ) 
            VALUES 
            (
                @RoomId, 
                @CheckInDate, 
                @CheckOutDate, 
                @NumberOfGuests, 
                @BookingStatus, 
                @CreatedBy, 
                @CreatedOn, 
                @ActualCheckInTime,
                @ActualCheckOutTime,
                @RoomNumber,
                @AccessPin
            ) 
            RETURNING ""BookingId"";";

        public static string InsertGuest = @"
            INSERT INTO ""GuestDetailsModel"" 
            (
                ""BookingId"", 
                ""GuestName"", 
                ""Age"", 
                ""Gender"", 
                ""GuestPhone"", 
                ""GuestEmail"", 
                ""IsPrimary"", 
                ""CreatedBy"", 
                ""CreatedOn""
            ) 
            VALUES 
            (
                @BookingId, 
                @GuestName, 
                @Age, 
                @Gender, 
                @GuestPhone, 
                @GuestEmail,  
                @IsPrimary, 
                @CreatedBy, 
                @CreatedOn
            ) 
            RETURNING ""GuestId"";";

        public static string InsertDocument = @"
            INSERT INTO ""DocumentModel"" 
            (
                ""EntityType"", 
                ""EntityId"", 
                ""DocumentType"", 
                ""FileName"", 
                ""Description"", 
                ""FilePath"", 
                ""IsPrimary"", 
                ""CreatedBy"", 
                ""CreatedOn""
            ) 
            VALUES 
            (
                @EntityType, 
                @EntityId, 
                @DocumentType, 
                @FileName, 
                @Description, 
                @FilePath, 
                @IsPrimary, 
                @CreatedBy, 
                @CreatedOn
            ) 
            RETURNING ""DocumentId"";";

        public static string InsertBookingService = @"
            INSERT INTO ""BookingServiceModel"" 
            (
                ""BookingId"", 
                ""ServiceId"", 
                ""ServiceName"",
                ""ServiceCategory"",
                ""SubCategory"",
                ""Price"",
                ""Unit"",
                ""Quantity"", 
                ""ServiceDate"", 
                ""ServiceStatus""
            ) 
            VALUES 
            (
                @BookingId, 
                @ServiceId, 
                @ServiceName,
                @ServiceCategory,
                @SubCategory,
                @Price,
                @Unit,
                @Quantity, 
                @ServiceDate, 
                @ServiceStatus
            ) 
            RETURNING ""BookingServiceId"";";
    }
}
