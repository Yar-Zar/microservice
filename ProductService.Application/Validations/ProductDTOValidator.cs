namespace ProductService.Application.Validations;
public class ProductDTOValidator<T> : AbstractValidator<T> where T : class
{
    public ProductDTOValidator(Func<T, ProductDTO> selector)
    {
        RuleFor(x => selector(x).Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(150).WithMessage("Product name must not exceed 150 characters.");

        RuleFor(x => selector(x).Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => selector(x).Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => selector(x).Description != null);

        RuleFor(x => selector(x).Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.");
    }
}

