using System;
using System.Collections.Generic;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly CustomerStore _customerStore;
        private readonly PlanCatalog _planCatalog;
        private readonly DiscountEvaluator _discountEvaluator;
        private readonly SurchargeEstimator _surchargeEstimator;
        private readonly TaxRateProvider _taxRateProvider;
        private readonly PaymentSystem _paymentSystem;

        public SubscriptionRenewalService() : this(
            new CustomerRepository(), 
            new SubscriptionPlanRepository(),
            new DefaultDiscountEvaluator(new List<DiscountPolicy> {
                new CustomerTierDiscountPolicy(),
                new SeniorityDiscountPolicy(),
                new VolumeDiscountPolicy(),
                new RewardPointsDiscountPolicy()
            }),
            new DefaultSurchargeEstimator(),
            new DefaultTaxRateProvider(),
            new PaymentSystemAdapter())
        {
        }

        public SubscriptionRenewalService(
            CustomerStore customerStore,
            PlanCatalog planCatalog,
            DiscountEvaluator discountEvaluator,
            SurchargeEstimator surchargeEstimator,
            TaxRateProvider taxRateProvider,
            PaymentSystem paymentSystem)
        {
            _customerStore = customerStore;
            _planCatalog = planCatalog;
            _discountEvaluator = discountEvaluator;
            _surchargeEstimator = surchargeEstimator;
            _taxRateProvider = taxRateProvider;
            _paymentSystem = paymentSystem;
        }

        public RenewalInvoice CreateRenewalInvoice(
            int customerId, string planCode, int seatCount, 
            string paymentMethod, bool includePremiumSupport, bool useLoyaltyPoints)
        {
            ValidateInputs(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();

            var customer = _customerStore.GetById(customerId);
            var plan = _planCatalog.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            
            var (discountAmount, notes) = _discountEvaluator.CalculateDiscount(customer, plan, seatCount, baseAmount, useLoyaltyPoints);

            decimal subtotal = baseAmount - discountAmount;
            if (subtotal < 300m)
            {
                subtotal = 300m;
                notes += "minimum discounted subtotal applied; ";
            }

            var (supportFee, supportNote) = _surchargeEstimator.CalculateSupportFee(normalizedPlanCode, includePremiumSupport);
            notes += supportNote;

            var (paymentFee, paymentNote) = _surchargeEstimator.CalculatePaymentFee(normalizedPaymentMethod, subtotal + supportFee);
            notes += paymentNote;

            decimal taxRate = _taxRateProvider.GetTaxRate(customer.Country);
            decimal taxBase = subtotal + supportFee + paymentFee;
            decimal taxAmount = taxBase * taxRate;
            decimal finalAmount = taxBase + taxAmount;

            if (finalAmount < 500m)
            {
                finalAmount = 500m;
                notes += "minimum invoice amount applied; ";
            }

            var invoice = BuildInvoice(customerId, normalizedPlanCode, normalizedPaymentMethod, seatCount, 
                                       baseAmount, discountAmount, supportFee, paymentFee, taxAmount, finalAmount, notes, customer.FullName);

            _paymentSystem.SaveInvoice(invoice);
            SendConfirmationEmail(customer, normalizedPlanCode, invoice.FinalAmount);

            return invoice;
        }

        private static void ValidateInputs(int customerId, string planCode, int seatCount, string paymentMethod)
        {
            if (customerId <= 0) throw new ArgumentException("Customer id must be positive");
            if (string.IsNullOrWhiteSpace(planCode)) throw new ArgumentException("Plan code is required");
            if (seatCount <= 0) throw new ArgumentException("Seat count must be positive");
            if (string.IsNullOrWhiteSpace(paymentMethod)) throw new ArgumentException("Payment method is required");
        }

        private RenewalInvoice BuildInvoice(int customerId, string planCode, string paymentMethod, int seatCount, 
            decimal baseAmount, decimal discountAmount, decimal supportFee, decimal paymentFee, decimal taxAmount, decimal finalAmount, string notes, string customerName)
        {
            return new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{planCode}",
                CustomerName = customerName,
                PlanCode = planCode,
                PaymentMethod = paymentMethod,
                SeatCount = seatCount,
                BaseAmount = Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(supportFee, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(paymentFee, 2, MidpointRounding.AwayFromZero),
                TaxAmount = Math.Round(taxAmount, 2, MidpointRounding.AwayFromZero),
                FinalAmount = Math.Round(finalAmount, 2, MidpointRounding.AwayFromZero),
                Notes = notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };
        }

        private void SendConfirmationEmail(Customer customer, string planCode, decimal finalAmount)
        {
            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";
                string body = $"Hello {customer.FullName}, your renewal for plan {planCode} " +
                              $"has been prepared. Final amount: {finalAmount:F2}.";
                _paymentSystem.SendEmail(customer.Email, subject, body);
            }
        }
    }
}