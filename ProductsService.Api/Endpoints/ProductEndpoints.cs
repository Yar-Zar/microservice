namespace ProductsService.Api.Endpoints;
public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/products")
                       .WithTags("Products") 
                      // .RequireAuthorization()
                       ; 

        group.MapPost("/", CreateProduct);
        group.MapPut("/{id:int}", UpdateProduct);
        group.MapGet("/dropdown", GetProductsForDropdown);
        group.MapGet("/", GetProducts);

        return group;
    }

    //Create Product Endpoint
    private static async Task<IResult> CreateProduct(CreateProductCommand command, ISender sender, CancellationToken ct)
        => Results.Ok(await sender.Send(command, ct));


    //Update Product Endpoint
    private static async Task<IResult> UpdateProduct(int id, [FromBody] ProductDTO productDto, ISender sender, CancellationToken ct)
    {
        var command = new UpdateProductCommand(id, productDto);
        var response = await sender.Send(command, ct);
        return Results.Ok(response);
    }

    // Get Products For Dropdown Endpoint
    private static async Task<IResult> GetProductsForDropdown(ISender sender, CancellationToken ct)
    {
        var query = new GetProductsForDropdownQuery();
        var result = await sender.Send(query, ct);
        return Results.Ok(result);
    }
    private static async Task<IResult> GetProducts([AsParameters] AppFilter filter, ISender sender, CancellationToken ct)
    {
        var query = new GetProductsQuery(filter);
        var result = await sender.Send(query, ct);
        return Results.Ok(result);
    }
}

