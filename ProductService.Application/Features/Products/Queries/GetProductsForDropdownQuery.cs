namespace ProductService.Application.Features.Products.Queries;
public record GetProductsForDropdownQuery : IRequest<IEnumerable<DropdownDto>>;

