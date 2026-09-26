namespace ProductService.Application.Features.Products.Queries;
public record GetProductsQuery(AppFilter Filter) : IRequest<IEnumerable<ProductDTO>>;

