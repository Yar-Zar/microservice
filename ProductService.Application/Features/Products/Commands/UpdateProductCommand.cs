namespace ProductService.Application.Features.Products.Commands;
public record UpdateProductCommand(int Id, ProductDTO Product) : IRequest<ResponseModel>;

