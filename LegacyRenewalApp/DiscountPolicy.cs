namespace LegacyRenewalApp;

public interface DiscountPolicy
{
    (decimal Discount, string Note) Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints);
}