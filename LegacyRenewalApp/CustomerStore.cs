namespace LegacyRenewalApp;

public interface CustomerStore
{
    Customer GetById(int customerId);
}