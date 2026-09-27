using System.ComponentModel.DataAnnotations;
using wad_project.Models;

namespace wad_project.DTOs;

public class MakePaymentDto
{
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.UPI;
    public bool SimulateFailure { get; set; } = false;
}

public class PaymentResponseDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime? PaidAt { get; set; }
}
