using System.ComponentModel.DataAnnotations;
using wad_project.Models;

namespace wad_project.ViewModels;

public class VehicleSearchFilterViewModel
{
    [Display(Name = "Pickup Location")]
    public string? SearchLocation { get; set; }

    [Display(Name = "Vehicle Category")]
    public VehicleType? VehicleType { get; set; } // Bike, Hatchback, Sedan, SUV, Luxury

    [Display(Name = "Min Daily Price")]
    public decimal? MinPrice { get; set; }

    [Display(Name = "Max Daily Price")]
    public decimal? MaxPrice { get; set; }

    [Display(Name = "Fuel Type")]
    public FuelType? FuelType { get; set; }

    [Display(Name = "Transmission")]
    public TransmissionType? Transmission { get; set; }

    [Display(Name = "Sort By")]
    public string? SortBy { get; set; } // "price_asc", "price_desc", "rating"
}

public class RecommendationRequestViewModel
{
    [Range(1, 15, ErrorMessage = "Please specify at least 1 passenger.")]
    [Display(Name = "Number of Passengers")]
    public int Passengers { get; set; } = 2;

    [Range(100, 1000000, ErrorMessage = "Please specify a budget.")]
    [Display(Name = "Budget (Rs.)")]
    public decimal Budget { get; set; } = 1500;

    [Display(Name = "Preferred Vehicle Type")]
    public VehicleType? PreferredType { get; set; }

    [Range(1, 30, ErrorMessage = "Duration must be between 1 and 30 days.")]
    [Display(Name = "Rental Days")]
    public int DurationDays { get; set; } = 1;
}

public class RecommendedVehicleViewModel
{
    public Vehicle Vehicle { get; set; } = new();
    public double Score { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal EstimatedTotalCost { get; set; }
}
