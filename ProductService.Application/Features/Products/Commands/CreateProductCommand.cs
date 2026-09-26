namespace ProductService.Application.Features.Products.Commands;
public record CreateProductCommand(ProductDTO Product) : IRequest<ResponseModel>;

