namespace LegacyRenewalApp;

public interface SurchargeEstimator
{
    (decimal Fee, string Note) CalculateSupportFee(string planCode, bool includePremiumSupport);
    (decimal Fee, string Note) CalculatePaymentFee(string paymentMethod, decimal amount);
}