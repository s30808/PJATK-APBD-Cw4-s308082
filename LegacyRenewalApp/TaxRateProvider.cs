namespace LegacyRenewalApp;

public interface TaxRateProvider
{
    decimal GetTaxRate(string country);
}