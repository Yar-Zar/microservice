namespace PaymentService.Application.Validations;
public class UpdatePaymentDTOValidator : AbstractValidator<PaymentDTO>
{
    public UpdatePaymentDTOValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID is required.");
        Include(new PaymentDTOValidator());
    }
}

