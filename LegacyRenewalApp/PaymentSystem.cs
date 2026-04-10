namespace LegacyRenewalApp;

public interface PaymentSystem
{ 
    void SaveInvoice(RenewalInvoice invoice);
    void SendEmail(string email, string subject, string body);
    
}