using System.Collections.Generic;

namespace LegacyRenewalApp;

public class DefaultDiscountEvaluator : DiscountEvaluator
{
    private readonly IEnumerable<DiscountPolicy> _policies;

    public DefaultDiscountEvaluator(IEnumerable<DiscountPolicy> policies) => _policies = policies;

    public (decimal TotalDiscount, string Notes) CalculateDiscount(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
    {
        decimal totalDiscount = 0;
        string notes = string.Empty;
        

        foreach (var policy in _policies)
        {
            
            var result = policy.Calculate(customer, plan, seatCount, baseAmount, useLoyaltyPoints);
            totalDiscount += result.Discount;
            notes += result.Note;
            
        }
        

        return (totalDiscount, notes);
    }
}