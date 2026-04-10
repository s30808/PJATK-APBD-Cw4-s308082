using System;

namespace LegacyRenewalApp;

public class DefaultSurchargeEstimator : SurchargeEstimator
{
    public (decimal Fee, string Note) CalculateSupportFee(string planCode, bool includePremiumSupport)
    {
        if (!includePremiumSupport) return (0m, string.Empty);

        decimal fee = planCode switch
        {
            "START" => 250m,
            "PRO" => 400m,
            "ENTERPRISE" => 700m,
            _ => 0m
        };
        return (fee, "premium support included; ");
    }

    public (decimal Fee, string Note) CalculatePaymentFee(string paymentMethod, decimal amount)
    {
        return paymentMethod switch
        {
            
            "CARD" => (amount * 0.02m, "card payment fee; "),
            "BANK_TRANSFER" => (amount * 0.01m, "bank transfer fee; "),
            "PAYPAL" => (amount * 0.035m, "paypal fee; "),
            "INVOICE" => (0m, "invoice payment; "),
            _ => throw new ArgumentException("Unsupported payment method")
        };
    }
}