namespace LegacyRenewalApp;

public interface DiscountEvaluator
{
    (decimal TotalDiscount, string Notes) CalculateDiscount(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints);
}