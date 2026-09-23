using Microsoft.EntityFrameworkCore;
using wad_project.Models;

namespace wad_project.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // 1. Seed Users (1 Admin, 2 Customers) if table is empty
        if (!await context.Users.AnyAsync())
        {
            var admin = new User
            {
                FullName = "Admin DriveEase",
                Email = "admin@driveease.com",
                MobileNumber = "9876543210",
                DrivingLicenseNumber = "DL-ADMIN-0001",
                Password = "1234", // Simple password without hashing
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
                Password = "1234", // Simple password without hashing
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
                Password = "1234", // Simple password without hashing
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            };

            await context.Users.AddRangeAsync(admin, customer1, customer2);
            await context.SaveChangesAsync();
        }

        // 2. Seed Vehicles if none exist
        if (!await context.Vehicles.AnyAsync())
        {
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
        }

        // 3. Seed Promo Codes if none exist
        if (!await context.PromoCodes.AnyAsync())
        {
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

            await context.PromoCodes.AddRangeAsync(promoWelcome, promoFlat);
            await context.SaveChangesAsync();
        }

        // 4. Seed Dynamic Pricing Rules if none exist
        if (!await context.PricingRules.AnyAsync())
        {
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
        }
    }
}
