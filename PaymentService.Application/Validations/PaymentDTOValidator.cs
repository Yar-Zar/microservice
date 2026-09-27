namespace PaymentService.Application.Validations;
public class PaymentDTOValidator : AbstractValidator<PaymentDTO>
{
    public PaymentDTOValidator()
    {
        // 1. OrderId Validation
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.")
            .MaximumLength(20).WithMessage("OrderId must not exceed 20 characters.");

        // 2. Amount Validation
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.")
            .PrecisionScale(18, 2, true).WithMessage("Amount must have a maximum of 18 digits in total with up to 2 decimal places.");

        // 3. Currency Validation
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .MaximumLength(10).WithMessage("Currency must not exceed 10 characters.");

        // 4. PaymentStatus Validation
        RuleFor(x => x.PaymentStatus)
            .NotEmpty().WithMessage("PaymentStatus is required.")
            .MaximumLength(50).WithMessage("PaymentStatus must not exceed 50 characters.");

        // 5. TransactionId Validation (Optional)
        RuleFor(x => x.TransactionId)
            .MaximumLength(100).WithMessage("TransactionId must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.TransactionId));

        // 6. CreatedBy Validation (Optional)
        RuleFor(x => x.CreatedBy)
            .MaximumLength(50).WithMessage("CreatedBy must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.CreatedBy));
    }
}

