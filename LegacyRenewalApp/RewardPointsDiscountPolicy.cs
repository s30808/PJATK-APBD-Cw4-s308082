using System;

namespace LegacyRenewalApp;

public class RewardPointsDiscountPolicy : DiscountPolicy
{
    public (decimal Discount, string Note) Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
    {
        if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
        {
            int pointsToUse = Math.Min(customer.LoyaltyPoints, 200);
            return (pointsToUse, $"loyalty points used: {pointsToUse}; ");
        }
        return (0m, string.Empty);
    }
}