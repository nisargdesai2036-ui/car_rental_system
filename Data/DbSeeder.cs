using Microsoft.EntityFrameworkCore;
using wad_project.Models;

namespace wad_project.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Avoid duplicate seeding
        if (await context.Users.AnyAsync())
        {
            return;
        }

        // 1. Seed Users (1 Admin, 2 Customers)
        var admin = new User
        {
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
            FullName = "Priya Patel",
            Email = "priya@gmail.com",
            MobileNumber = "9871234560",
            DrivingLicenseNumber = "DL04-2022-00431",
            Password = "User@123",
            Role = UserRole.Customer,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        await context.Users.AddRangeAsync(admin, customer1, customer2);
        await context.SaveChangesAsync();

        // 2. Seed Vehicles (Bikes, Hatchbacks, Sedans, SUVs, Luxury)
        var vehicles = new List<Vehicle>
        {
            new()
            {
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
            },
            new()
            {
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
            },
            new()
            {
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
            },
            new()
            {
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
            },
            new()
            {
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
            },
            new()
            {
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
            },
            new()
            {
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
            },
            new()
            {
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
            }
        };

        await context.Vehicles.AddRangeAsync(vehicles);
        await context.SaveChangesAsync();

        // 3. Seed Promo Codes
        var promoWelcome = new PromoCode
        {
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
            Code = "EXPIRED50",
            DiscountType = DiscountType.Percentage,
            DiscountValue = 50m,
            MinBookingAmount = 500m,
            ValidFrom = DateTime.UtcNow.AddMonths(-3),
            ValidTo = DateTime.UtcNow.AddDays(-5),
            IsActive = true
        };

        await context.PromoCodes.AddRangeAsync(promoWelcome, promoFlat, promoExpired);

        // 4. Seed Dynamic Pricing Rules
        var weekendSurge = new PricingRule
        {
            Name = "Weekend Demand Surge (+15%)",
            VehicleType = null,
            Multiplier = 1.15m,
            DayOfWeek = DayOfWeek.Saturday,
            IsActive = true
        };

        await context.PricingRules.AddAsync(weekendSurge);
        await context.SaveChangesAsync();

        // 5. Seed Historical Completed Booking (Priya on Honda City)
        var cityVehicle = vehicles.First(v => v.Model.Contains("City"));
        var completedBooking = new Booking
        {
            BookingReference = "DE-202608-1001",
            UserId = customer2.Id,
            VehicleId = cityVehicle.Id,
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

        await context.Bookings.AddAsync(completedBooking);
        await context.SaveChangesAsync();

        var paymentCompleted = new Payment
        {
            BookingId = completedBooking.Id,
            TransactionId = "TXN-9021831",
            PaymentMethod = PaymentMethod.UPI,
            Amount = 5000m,
            Status = PaymentStatus.Successful,
            PaidAt = DateTime.UtcNow.AddDays(-12)
        };

        var reviewCompleted = new Review
        {
            UserId = customer2.Id,
            VehicleId = cityVehicle.Id,
            BookingId = completedBooking.Id,
            Rating = 5,
            Comment = "Excellent self-drive experience! The Honda City was clean, smooth, and delivered on time.",
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        };

        await context.Payments.AddAsync(paymentCompleted);
        await context.Reviews.AddAsync(reviewCompleted);

        // 6. Seed Existing Confirmed Future Booking (Rahul on Maruti Swift)
        var swiftVehicle = vehicles.First(v => v.Model.Contains("Swift"));
        var futureBooking = new Booking
        {
            BookingReference = "DE-202609-1002",
            UserId = customer1.Id,
            VehicleId = swiftVehicle.Id,
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

        await context.Bookings.AddAsync(futureBooking);
        await context.SaveChangesAsync();

        var futurePayment = new Payment
        {
            BookingId = futureBooking.Id,
            TransactionId = "TXN-9082341",
            PaymentMethod = PaymentMethod.Card,
            Amount = 3600m,
            Status = PaymentStatus.Successful,
            PaidAt = DateTime.UtcNow.AddDays(-1)
        };

        await context.Payments.AddAsync(futurePayment);

        // 7. Seed Notification
        var welcomeSms = new Notification
        {
            UserId = customer1.Id,
            RecipientPhone = customer1.MobileNumber,
            Message = "Welcome to DriveEase, Rahul Sharma! Your self-drive account has been activated.",
            Type = NotificationType.BookingConfirmation,
            IsSent = true,
            SentAt = DateTime.UtcNow.AddDays(-1)
        };

        await context.Notifications.AddAsync(welcomeSms);
        await context.SaveChangesAsync();
    }
}
