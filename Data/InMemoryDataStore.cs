using wad_project.Models;

namespace wad_project.Data;

/// <summary>
/// Thread-safe in-memory data store with realistic seed data (including Cars and Bikes) for Phase 1.
/// </summary>
public class InMemoryDataStore : IDataStore
{
    private readonly object _lock = new();

    public List<User> Users { get; private set; } = new();
    public List<Vehicle> Vehicles { get; private set; } = new();
    public List<Booking> Bookings { get; private set; } = new();
    public List<Payment> Payments { get; private set; } = new();
    public List<PromoCode> PromoCodes { get; private set; } = new();
    public List<Review> Reviews { get; private set; } = new();
    public List<Notification> Notifications { get; private set; } = new();
    public List<PricingRule> PricingRules { get; private set; } = new();

    private int _userIdSeq;
    private int _vehicleIdSeq;
    private int _bookingIdSeq;
    private int _paymentIdSeq;
    private int _promoCodeIdSeq;
    private int _reviewIdSeq;
    private int _notificationIdSeq;
    private int _pricingRuleIdSeq;

    public InMemoryDataStore()
    {
        SeedData();
    }

    public int NextUserId() => Interlocked.Increment(ref _userIdSeq);
    public int NextVehicleId() => Interlocked.Increment(ref _vehicleIdSeq);
    public int NextBookingId() => Interlocked.Increment(ref _bookingIdSeq);
    public int NextPaymentId() => Interlocked.Increment(ref _paymentIdSeq);
    public int NextPromoCodeId() => Interlocked.Increment(ref _promoCodeIdSeq);
    public int NextReviewId() => Interlocked.Increment(ref _reviewIdSeq);
    public int NextNotificationId() => Interlocked.Increment(ref _notificationIdSeq);
    public int NextPricingRuleId() => Interlocked.Increment(ref _pricingRuleIdSeq);

    public void Reset()
    {
        lock (_lock)
        {
            Users.Clear();
            Vehicles.Clear();
            Bookings.Clear();
            Payments.Clear();
            PromoCodes.Clear();
            Reviews.Clear();
            Notifications.Clear();
            PricingRules.Clear();
            SeedData();
        }
    }

    private void SeedData()
    {
        _userIdSeq = 0;
        _vehicleIdSeq = 0;
        _bookingIdSeq = 0;
        _paymentIdSeq = 0;
        _promoCodeIdSeq = 0;
        _reviewIdSeq = 0;
        _notificationIdSeq = 0;
        _pricingRuleIdSeq = 0;

        // 1. Seed Users (1 Admin, 2 Customers)
        var admin = new User
        {
            Id = NextUserId(),
            FullName = "Admin DriveEase",
            Email = "admin@driveease.com",
            MobileNumber = "9876543210",
            DrivingLicenseNumber = "DL-ADMIN-0001",
            Password = "Admin@123",
            Role = UserRole.Administrator,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };

        var customer1 = new User
        {
            Id = NextUserId(),
            FullName = "Rahul Sharma",
            Email = "rahul@gmail.com",
            MobileNumber = "9823456780",
            DrivingLicenseNumber = "MH12-2021-00892",
            Password = "User@123",
            Role = UserRole.Customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-2)
        };

        var customer2 = new User
        {
            Id = NextUserId(),
            FullName = "Priya Patel",
            Email = "priya@gmail.com",
            MobileNumber = "9871234560",
            DrivingLicenseNumber = "DL04-2022-00431",
            Password = "User@123",
            Role = UserRole.Customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        Users.AddRange([admin, customer1, customer2]);

        // 2. Seed Vehicles (Bikes and Cars)
        // Bike 1: Royal Enfield
        var royalEnfield = new Vehicle
        {
            Id = NextVehicleId(),
            Brand = "Royal Enfield",
            Model = "Classic 350",
            Year = 2023,
            LicensePlate = "MH12-BK-1111",
            VehicleType = VehicleType.Bike,
            SeatingCapacity = 2,
            FuelType = FuelType.Petrol,
            Transmission = TransmissionType.Manual,
            HourlyRate = 70m,
            DailyRate = 600m,
            PickupLocation = "Downtown Hub",
            ImageUrl = "/images/vehicles/re_classic.png",
            Status = VehicleStatus.Available,
            AverageRating = 4.8m,
            ReviewCount = 14,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };

        // Bike 2: Honda Activa Scooter
        var activa = new Vehicle
        {
            Id = NextVehicleId(),
            Brand = "Honda",
            Model = "Activa 6G",
            Year = 2024,
            LicensePlate = "MH12-BK-2222",
            VehicleType = VehicleType.Bike,
            SeatingCapacity = 2,
            FuelType = FuelType.Petrol,
            Transmission = TransmissionType.Automatic,
            HourlyRate = 50m,
            DailyRate = 450m,
            PickupLocation = "Railway Station",
            ImageUrl = "/images/vehicles/activa.png",
            Status = VehicleStatus.Available,
            AverageRating = 4.7m,
            ReviewCount = 19,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };

        // Car 1: Hatchback
        var swift = new Vehicle
        {
            Id = NextVehicleId(),
            Brand = "Maruti Suzuki",
            Model = "Swift VXi",
            Year = 2023,
            LicensePlate = "MH12-AB-1234",
            VehicleType = VehicleType.Hatchback,
            SeatingCapacity = 5,
            FuelType = FuelType.Petrol,
            Transmission = TransmissionType.Manual,
            HourlyRate = 150m,
            DailyRate = 1800m,
            PickupLocation = "Downtown Hub",
            ImageUrl = "/images/vehicles/swift.png",
            Status = VehicleStatus.Available,
            AverageRating = 4.6m,
            ReviewCount = 12,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };

        // Car 2: Sedan
        var city = new Vehicle
        {
            Id = NextVehicleId(),
            Brand = "Honda",
            Model = "City ZX",
            Year = 2024,
            LicensePlate = "MH12-CD-5678",
            VehicleType = VehicleType.Sedan,
            SeatingCapacity = 5,
            FuelType = FuelType.Petrol,
            Transmission = TransmissionType.Automatic,
            HourlyRate = 250m,
            DailyRate = 3000m,
            PickupLocation = "Airport Terminal 1",
            ImageUrl = "/images/vehicles/city.png",
            Status = VehicleStatus.Available,
            AverageRating = 4.8m,
            ReviewCount = 18,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };

        // Car 3: SUV
        var creta = new Vehicle
        {
            Id = NextVehicleId(),
            Brand = "Hyundai",
            Model = "Creta SX",
            Year = 2023,
            LicensePlate = "MH12-EF-9012",
            VehicleType = VehicleType.SUV,
            SeatingCapacity = 5,
            FuelType = FuelType.Diesel,
            Transmission = TransmissionType.Automatic,
            HourlyRate = 280m,
            DailyRate = 3500m,
            PickupLocation = "Downtown Hub",
            ImageUrl = "/images/vehicles/creta.png",
            Status = VehicleStatus.Available,
            AverageRating = 4.7m,
            ReviewCount = 15,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };

        // Car 4: Off-Road SUV
        var thar = new Vehicle
        {
            Id = NextVehicleId(),
            Brand = "Mahindra",
            Model = "Thar 4x4",
            Year = 2023,
            LicensePlate = "MH12-GH-3456",
            VehicleType = VehicleType.SUV,
            SeatingCapacity = 4,
            FuelType = FuelType.Diesel,
            Transmission = TransmissionType.Manual,
            HourlyRate = 320m,
            DailyRate = 4000m,
            PickupLocation = "Railway Station",
            ImageUrl = "/images/vehicles/thar.png",
            Status = VehicleStatus.Available,
            AverageRating = 4.9m,
            ReviewCount = 20,
            CreatedAt = DateTime.UtcNow.AddMonths(-2)
        };

        // Car 5: Luxury
        var bmw = new Vehicle
        {
            Id = NextVehicleId(),
            Brand = "BMW",
            Model = "3 Series Gran Limousine",
            Year = 2024,
            LicensePlate = "MH12-JK-7890",
            VehicleType = VehicleType.Luxury,
            SeatingCapacity = 5,
            FuelType = FuelType.Petrol,
            Transmission = TransmissionType.Automatic,
            HourlyRate = 600m,
            DailyRate = 8500m,
            PickupLocation = "Airport Terminal 1",
            ImageUrl = "/images/vehicles/bmw3.png",
            Status = VehicleStatus.Available,
            AverageRating = 5.0m,
            ReviewCount = 8,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        // Car 6: Under Maintenance Car
        var maintenanceCar = new Vehicle
        {
            Id = NextVehicleId(),
            Brand = "Tata",
            Model = "Nexon EV",
            Year = 2023,
            LicensePlate = "MH12-MN-4321",
            VehicleType = VehicleType.SUV,
            SeatingCapacity = 5,
            FuelType = FuelType.Electric,
            Transmission = TransmissionType.Automatic,
            HourlyRate = 220m,
            DailyRate = 2800m,
            PickupLocation = "Downtown Hub",
            ImageUrl = "/images/vehicles/nexon_ev.png",
            Status = VehicleStatus.UnderMaintenance,
            AverageRating = 4.4m,
            ReviewCount = 5,
            CreatedAt = DateTime.UtcNow.AddMonths(-2)
        };

        Vehicles.AddRange([royalEnfield, activa, swift, city, creta, thar, bmw, maintenanceCar]);

        // 3. Seed Promo Codes
        var promoWelcome = new PromoCode
        {
            Id = NextPromoCodeId(),
            Code = "WELCOME20",
            DiscountType = DiscountType.Percentage,
            DiscountValue = 20m,
            MinBookingAmount = 1000m,
            ValidFrom = DateTime.UtcNow.AddMonths(-1),
            ValidTo = DateTime.UtcNow.AddMonths(3),
            IsActive = true
        };

        var promoFlat = new PromoCode
        {
            Id = NextPromoCodeId(),
            Code = "FLAT500",
            DiscountType = DiscountType.FlatAmount,
            DiscountValue = 500m,
            MinBookingAmount = 2000m,
            ValidFrom = DateTime.UtcNow.AddMonths(-1),
            ValidTo = DateTime.UtcNow.AddMonths(2),
            IsActive = true
        };

        var promoExpired = new PromoCode
        {
            Id = NextPromoCodeId(),
            Code = "EXPIRED50",
            DiscountType = DiscountType.Percentage,
            DiscountValue = 50m,
            MinBookingAmount = 500m,
            ValidFrom = DateTime.UtcNow.AddMonths(-3),
            ValidTo = DateTime.UtcNow.AddDays(-5), // Expired
            IsActive = true
        };

        PromoCodes.AddRange([promoWelcome, promoFlat, promoExpired]);

        // 4. Seed Dynamic Pricing Rules
        var weekendSurge = new PricingRule
        {
            Id = NextPricingRuleId(),
            Name = "Weekend Demand Surge (+15%)",
            VehicleType = null,
            Multiplier = 1.15m,
            DayOfWeek = DayOfWeek.Saturday,
            IsActive = true
        };
        PricingRules.Add(weekendSurge);

        // 5. Seed Historical Completed Booking (Priya on Honda City)
        var completedBooking = new Booking
        {
            Id = NextBookingId(),
            BookingReference = "DE-202608-1001",
            UserId = customer2.Id,
            User = customer2,
            VehicleId = city.Id,
            Vehicle = city,
            RentalType = RentalType.Daily,
            PickupDateTime = DateTime.UtcNow.AddDays(-10),
            ReturnDateTime = DateTime.UtcNow.AddDays(-8),
            PickupLocation = "Airport Terminal 1",
            RentalDuration = 2,
            BasePrice = 6000m,
            DiscountAmount = 1000m,
            TotalAmount = 5000m,
            PromoCodeId = promoWelcome.Id,
            Status = BookingStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddDays(-12)
        };

        var paymentCompleted = new Payment
        {
            Id = NextPaymentId(),
            BookingId = completedBooking.Id,
            Booking = completedBooking,
            TransactionId = "TXN-9021831",
            PaymentMethod = PaymentMethod.UPI,
            Amount = 5000m,
            Status = PaymentStatus.Successful,
            PaidAt = DateTime.UtcNow.AddDays(-12)
        };
        completedBooking.Payment = paymentCompleted;

        var reviewCompleted = new Review
        {
            Id = NextReviewId(),
            UserId = customer2.Id,
            User = customer2,
            VehicleId = city.Id,
            Vehicle = city,
            BookingId = completedBooking.Id,
            Booking = completedBooking,
            Rating = 5,
            Comment = "Excellent self-drive experience! The Honda City was clean, smooth, and delivered on time.",
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        };
        completedBooking.Review = reviewCompleted;

        // 6. Seed Existing Confirmed Future Booking (Rahul on Maruti Swift)
        var futureBooking = new Booking
        {
            Id = NextBookingId(),
            BookingReference = "DE-202609-1002",
            UserId = customer1.Id,
            User = customer1,
            VehicleId = swift.Id,
            Vehicle = swift,
            RentalType = RentalType.Daily,
            PickupDateTime = DateTime.UtcNow.AddDays(5),
            ReturnDateTime = DateTime.UtcNow.AddDays(7),
            PickupLocation = "Downtown Hub",
            RentalDuration = 2,
            BasePrice = 3600m,
            DiscountAmount = 0m,
            TotalAmount = 3600m,
            Status = BookingStatus.Confirmed,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var futurePayment = new Payment
        {
            Id = NextPaymentId(),
            BookingId = futureBooking.Id,
            Booking = futureBooking,
            TransactionId = "TXN-9082341",
            PaymentMethod = PaymentMethod.Card,
            Amount = 3600m,
            Status = PaymentStatus.Successful,
            PaidAt = DateTime.UtcNow.AddDays(-1)
        };
        futureBooking.Payment = futurePayment;

        Bookings.AddRange([completedBooking, futureBooking]);
        Payments.AddRange([paymentCompleted, futurePayment]);
        Reviews.Add(reviewCompleted);

        // 7. Seed SMS Notification
        var welcomeSms = new Notification
        {
            Id = NextNotificationId(),
            UserId = customer1.Id,
            User = customer1,
            RecipientPhone = customer1.MobileNumber,
            Message = "Welcome to DriveEase, Rahul Sharma! Your self-drive account has been activated.",
            Type = NotificationType.BookingConfirmation,
            IsSent = true,
            SentAt = DateTime.UtcNow.AddDays(-1)
        };
        Notifications.Add(welcomeSms);
    }
}
