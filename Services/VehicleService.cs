using Microsoft.EntityFrameworkCore;
using wad_project.Data;
using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public class VehicleService : IVehicleService
{
    private readonly ApplicationDbContext _context;

    public VehicleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Vehicle>> GetAllVehiclesAsync()
    {
        return await _context.Vehicles
            .Where(v => v.Status != VehicleStatus.UnderMaintenance)
            .OrderBy(v => v.DailyRate)
            .ToListAsync();
    }

    public async Task<List<Vehicle>> SearchAndFilterAsync(VehicleSearchFilterViewModel filter)
    {
        var query = _context.Vehicles
            .Where(v => v.Status != VehicleStatus.UnderMaintenance)
            .AsQueryable();

        // 1. Location search
        if (!string.IsNullOrWhiteSpace(filter.SearchLocation))
        {
            var loc = filter.SearchLocation.Trim().ToLower();
            query = query.Where(v => v.PickupLocation.ToLower().Contains(loc));
        }

        // 2. Vehicle Category (Bike, Hatchback, Sedan, SUV, Luxury)
        if (filter.VehicleType.HasValue)
        {
            query = query.Where(v => v.VehicleType == filter.VehicleType.Value);
        }

        // 3. Price range
        if (filter.MinPrice.HasValue)
        {
            query = query.Where(v => v.DailyRate >= filter.MinPrice.Value);
        }
        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(v => v.DailyRate <= filter.MaxPrice.Value);
        }

        // 4. Fuel & Transmission
        if (filter.FuelType.HasValue)
        {
            query = query.Where(v => v.FuelType == filter.FuelType.Value);
        }
        if (filter.Transmission.HasValue)
        {
            query = query.Where(v => v.Transmission == filter.Transmission.Value);
        }

        // 5. Sorting
        query = filter.SortBy switch
        {
            "price_asc" => query.OrderBy(v => v.DailyRate),
            "price_desc" => query.OrderByDescending(v => v.DailyRate),
            "rating" => query.OrderByDescending(v => v.AverageRating),
            _ => query.OrderBy(v => v.DailyRate)
        };

        return await query.ToListAsync();
    }

    public async Task<Vehicle?> GetVehicleByIdAsync(int id)
    {
        return await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<List<RecommendedVehicleViewModel>> GetRecommendationsAsync(RecommendationRequestViewModel criteria)
    {
        var candidates = await _context.Vehicles
            .Where(v => v.Status == VehicleStatus.Available)
            .ToListAsync();

        var recommendations = new List<RecommendedVehicleViewModel>();

        foreach (var v in candidates)
        {
            // Must be able to accommodate passengers
            if (v.SeatingCapacity < criteria.Passengers)
                continue;

            decimal estimatedCost = v.DailyRate * criteria.DurationDays;

            // Simple rule-based scoring (out of 100)
            double score = 0;
            var reasons = new List<string>();

            // 1. Passenger Capacity Match (30 pts)
            if (v.SeatingCapacity == criteria.Passengers || v.SeatingCapacity == criteria.Passengers + 1)
            {
                score += 30;
                reasons.Add($"Perfect seating capacity ({v.SeatingCapacity} seats)");
            }
            else
            {
                score += 20;
                reasons.Add($"Spacious option ({v.SeatingCapacity} seats)");
            }

            // 2. Budget Fit (30 pts)
            if (estimatedCost <= criteria.Budget)
            {
                score += 30;
                reasons.Add($"Under your budget of Rs.{criteria.Budget:N0}");
            }
            else if (estimatedCost <= criteria.Budget * 1.2m)
            {
                score += 15;
                reasons.Add("Slightly above budget but high value");
            }
            else
            {
                continue; // Skip if well over budget
            }

            // 3. Preferred Vehicle Type (25 pts)
            if (criteria.PreferredType.HasValue)
            {
                if (v.VehicleType == criteria.PreferredType.Value)
                {
                    score += 25;
                    reasons.Add($"Matches preferred category ({v.VehicleType})");
                }
            }
            else
            {
                score += 15;
            }

            // 4. Rating (15 pts)
            score += (double)v.AverageRating * 3.0;

            recommendations.Add(new RecommendedVehicleViewModel
            {
                Vehicle = v,
                Score = Math.Round(score, 1),
                Reason = string.Join(" • ", reasons),
                EstimatedTotalCost = estimatedCost
            });
        }

        return recommendations.OrderByDescending(r => r.Score).ToList();
    }

    public async Task<(bool Success, string Message)> AddVehicleAsync(Vehicle vehicle)
    {
        var plate = vehicle.LicensePlate.Trim().ToLower();

        // Business Rule: Check for duplicate license plate
        if (await _context.Vehicles.AnyAsync(v => v.LicensePlate.ToLower() == plate))
        {
            return (false, "A vehicle with this license plate already exists.");
        }

        vehicle.CreatedAt = DateTime.UtcNow;
        await _context.Vehicles.AddAsync(vehicle);
        await _context.SaveChangesAsync();

        return (true, "Vehicle added successfully.");
    }

    public async Task<bool> UpdateStatusAsync(int vehicleId, VehicleStatus status)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId);
        if (vehicle == null) return false;

        vehicle.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }
}
