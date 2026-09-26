namespace ProductService.Application.Validations;
public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid Product ID.");
        Include(new ProductDTOValidator<UpdateProductCommand>(x => x.Product));
    }
}

