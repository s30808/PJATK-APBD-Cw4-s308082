namespace LegacyRenewalApp;

public interface PlanCatalog
{
    SubscriptionPlan GetByCode(string code);
}