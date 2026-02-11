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
                ""ActualCheckOutTime""
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
                @ActualCheckOutTime
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
                ""IdProof"", 
                ""IdProofImagePath"", 
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
                8, 
                8, 
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
                ""Price"",
                ""Quantity"", 
                ""ServiceDate"", 
                ""ServiceStatus"", 
                ""CreatedOn""
            ) 
            VALUES 
            (
                @BookingId, 
                @ServiceId, 
                @ServiceName,
                @Price,
                @Quantity, 
                @ServiceDate, 
                @ServiceStatus, 
                @CreatedOn
            ) 
            RETURNING ""BookingServiceId"";";
    }
}
