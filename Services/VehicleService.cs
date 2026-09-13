using wad_project.Data;
using wad_project.Models;
using wad_project.ViewModels;

namespace wad_project.Services;

public class VehicleService : IVehicleService
{
    private readonly IDataStore _dataStore;

    public VehicleService(IDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<List<Vehicle>> GetAllVehiclesAsync()
    {
        lock (_dataStore.Vehicles)
        {
            // Only return active, available vehicles for general browsing
            var list = _dataStore.Vehicles
                .Where(v => v.Status != VehicleStatus.UnderMaintenance)
                .ToList();
            return Task.FromResult(list);
        }
    }

    public Task<List<Vehicle>> SearchAndFilterAsync(VehicleSearchFilterViewModel filter)
    {
        lock (_dataStore.Vehicles)
        {
            var query = _dataStore.Vehicles
                .Where(v => v.Status != VehicleStatus.UnderMaintenance)
                .AsQueryable();

            // 1. Location search
            if (!string.IsNullOrWhiteSpace(filter.SearchLocation))
            {
                var loc = filter.SearchLocation.Trim();
                query = query.Where(v => v.PickupLocation.Contains(loc, StringComparison.OrdinalIgnoreCase));
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
            var results = filter.SortBy switch
            {
                "price_asc" => query.OrderBy(v => v.DailyRate).ToList(),
                "price_desc" => query.OrderByDescending(v => v.DailyRate).ToList(),
                "rating" => query.OrderByDescending(v => v.AverageRating).ToList(),
                _ => query.OrderBy(v => v.DailyRate).ToList()
            };

            return Task.FromResult(results);
        }
    }

    public Task<Vehicle?> GetVehicleByIdAsync(int id)
    {
        lock (_dataStore.Vehicles)
        {
            var vehicle = _dataStore.Vehicles.FirstOrDefault(v => v.Id == id);
            return Task.FromResult(vehicle);
        }
    }

    public Task<List<RecommendedVehicleViewModel>> GetRecommendationsAsync(RecommendationRequestViewModel criteria)
    {
        lock (_dataStore.Vehicles)
        {
            var candidates = _dataStore.Vehicles
                .Where(v => v.Status == VehicleStatus.Available)
                .ToList();

            var recommendations = new List<RecommendedVehicleViewModel>();

            foreach (var v in candidates)
            {
                // Must be able to accommodate passengers (e.g. 2 for bike, 4-5 for car)
                if (v.SeatingCapacity < criteria.Passengers)
                    continue;

                decimal estimatedCost = v.DailyRate * criteria.DurationDays;

                // Simple college rule-based scoring (out of 100)
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

            var sorted = recommendations.OrderByDescending(r => r.Score).ToList();
            return Task.FromResult(sorted);
        }
    }

    public Task<(bool Success, string Message)> AddVehicleAsync(Vehicle vehicle)
    {
        lock (_dataStore.Vehicles)
        {
            // Business Rule: Check for duplicate license plate
            if (_dataStore.Vehicles.Any(v => v.LicensePlate.Equals(vehicle.LicensePlate.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return Task.FromResult((false, "A vehicle with this license plate already exists."));
            }

            vehicle.Id = _dataStore.NextVehicleId();
            vehicle.CreatedAt = DateTime.UtcNow;
            _dataStore.Vehicles.Add(vehicle);

            return Task.FromResult((true, "Vehicle added successfully."));
        }
    }

    public Task<bool> UpdateStatusAsync(int vehicleId, VehicleStatus status)
    {
        lock (_dataStore.Vehicles)
        {
            var vehicle = _dataStore.Vehicles.FirstOrDefault(v => v.Id == vehicleId);
            if (vehicle == null) return Task.FromResult(false);

            vehicle.Status = status;
            return Task.FromResult(true);
        }
    }
}
