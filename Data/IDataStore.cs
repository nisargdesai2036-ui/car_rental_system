using wad_project.Models;

namespace wad_project.Data;

/// <summary>
/// Abstraction for data access in Phase 1.
/// In Phase 2, this is seamlessly swapped with Entity Framework Core DbContext.
/// </summary>
public interface IDataStore
{
    List<User> Users { get; }
    List<Vehicle> Vehicles { get; }
    List<Booking> Bookings { get; }
    List<Payment> Payments { get; }
    List<PromoCode> PromoCodes { get; }
    List<Review> Reviews { get; }
    List<Notification> Notifications { get; }
    List<PricingRule> PricingRules { get; }

    int NextUserId();
    int NextVehicleId();
    int NextBookingId();
    int NextPaymentId();
    int NextPromoCodeId();
    int NextReviewId();
    int NextNotificationId();
    int NextPricingRuleId();

    void Reset();
}
