using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using wad_project.Data;
using wad_project.DTOs;
using wad_project.Models;

namespace wad_project.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromoCodesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PromoCodesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PromoCode>>>> GetAllPromoCodes([FromQuery] bool onlyActive = false)
    {
        var query = _context.PromoCodes.AsQueryable();
        if (onlyActive)
        {
            var now = DateTime.UtcNow;
            query = query.Where(p => p.IsActive && p.ValidFrom <= now && p.ValidTo >= now);
        }

        var codes = await query.OrderByDescending(p => p.ValidTo).ToListAsync();
        return Ok(ApiResponse<List<PromoCode>>.Ok(codes));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PromoCode>>> CreatePromoCode([FromBody] CreatePromoCodeDto dto)
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<PromoCode>.Fail("Invalid promo code data."));
        }

        var codeUpper = dto.Code.Trim().ToUpperInvariant();
        if (await _context.PromoCodes.AnyAsync(p => p.Code.ToUpper() == codeUpper))
        {
            return BadRequest(ApiResponse<PromoCode>.Fail($"Promo code '{codeUpper}' already exists."));
        }

        var promo = new PromoCode
        {
            Code = codeUpper,
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            MinBookingAmount = dto.MinBookingAmount,
            ValidFrom = dto.ValidFrom.ToUniversalTime(),
            ValidTo = dto.ValidTo.ToUniversalTime(),
            IsActive = dto.IsActive
        };

        await _context.PromoCodes.AddAsync(promo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAllPromoCodes), new { id = promo.Id }, ApiResponse<PromoCode>.Ok(promo, "Promo code created successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<PromoCode>>> UpdatePromoCode(int id, [FromBody] CreatePromoCodeDto dto)
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var promo = await _context.PromoCodes.FindAsync(id);
        if (promo == null)
        {
            return NotFound(ApiResponse<PromoCode>.Fail($"Promo code with ID {id} not found."));
        }

        promo.Code = dto.Code.Trim().ToUpperInvariant();
        promo.DiscountType = dto.DiscountType;
        promo.DiscountValue = dto.DiscountValue;
        promo.MinBookingAmount = dto.MinBookingAmount;
        promo.ValidFrom = dto.ValidFrom.ToUniversalTime();
        promo.ValidTo = dto.ValidTo.ToUniversalTime();
        promo.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return Ok(ApiResponse<PromoCode>.Ok(promo, "Promo code updated successfully."));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeletePromoCode(int id)
    {
        var authCheck = await EnsureAdminAsync();
        if (authCheck != null) return authCheck;

        var promo = await _context.PromoCodes.FindAsync(id);
        if (promo == null)
        {
            return NotFound(ApiResponse.Fail($"Promo code with ID {id} not found."));
        }

        _context.PromoCodes.Remove(promo);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok("Promo code deleted successfully."));
    }

    private async Task<ActionResult?> EnsureAdminAsync()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst(ClaimTypes.Name)?.Value
                    ?? User.Identity?.Name;

        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized(ApiResponse.Fail("Authentication required."));
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        if (user == null || user.Role != UserRole.Admin)
        {
            return StatusCode(403, ApiResponse.Fail("Only administrators can manage promotional codes."));
        }

        return null;
    }
}
