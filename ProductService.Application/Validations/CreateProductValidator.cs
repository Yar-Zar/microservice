namespace ProductService.Application.Validations;
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        Include(new ProductDTOValidator<CreateProductCommand>(x => x.Product));
    }
}

