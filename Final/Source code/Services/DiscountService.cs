using ProjectTest.Models;

namespace ProjectTest.Services;

public class DiscountService
{
    public decimal CalculateDiscount(decimal subtotal, Promotion? promotion)
    {
        if (promotion is null ||
            !promotion.IsActive ||
            subtotal < promotion.MinimumOrderTotal ||
            DateTime.Today < promotion.StartDate.Date ||
            DateTime.Today > promotion.EndDate.Date)
        {
            return 0m;
        }

        var discount = promotion.DiscountType == DiscountType.Percentage
            ? subtotal * Math.Clamp(promotion.DiscountValue, 0m, 100m) / 100m
            : promotion.DiscountValue;

        return Math.Min(subtotal, Math.Max(0m, decimal.Round(discount, 2)));
    }
}
