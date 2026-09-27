namespace wad_project.Models;

public enum UserRole
{
    Customer = 1,
    Admin = 2,
    Owner = 3
}

public enum VerificationStatus
{
    Pending = 1,
    Verified = 2,
    Rejected = 3
}

public enum RentalType
{
    Hourly = 1,
    Daily = 2
}

public enum VehicleType
{
    Bike = 1,       // Bike / Two-wheeler option
    Hatchback = 2,
    Sedan = 3,
    SUV = 4,
    Luxury = 5
}

public enum FuelType
{
    Petrol = 1,
    Diesel = 2,
    Electric = 3
}

public enum TransmissionType
{
    Manual = 1,
    Automatic = 2
}

public enum VehicleStatus
{
    Pending = 1,
    Approved = 2,
    Available = 2, // alias for Approved
    Booked = 3,
    UnderMaintenance = 4,
    Maintenance = 4, // alias
    Rejected = 5,
    Inactive = 6
}

public enum BookingStatus
{
    Pending = 1,
    Confirmed = 2,
    Completed = 3,
    Cancelled = 4
}

public enum PaymentStatus
{
    Pending = 1,
    Successful = 2,
    Failed = 3
}

public enum PaymentMethod
{
    Card = 1,
    UPI = 2,
    NetBanking = 3,
    Wallet = 4
}

public enum DiscountType
{
    Percentage = 1,
    FlatAmount = 2
}

public enum NotificationType
{
    BookingConfirmation = 1,
    BookingCancelled = 2,
    PaymentAlert = 3
}
