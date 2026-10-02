using Shared.GrpcContracts.Product;

namespace ProductService.Application.Interfaces;
public interface IProductRepository
{
    public Task AddAsync(Product entity);
    public Task DeleteAsync(Product entity);
    public Task<Product?> GetAsync(Expression<Func<Product, bool>> predicate, CancellationToken ct);
    public Task<bool> CheckExistsAsync(Expression<Func<Product, bool>> predicate);
    public Task<Product> GetByIdAsync(int id, CancellationToken ct);
    public Task<IEnumerable<DropdownDto>> GetProductsForDropdownAsync(CancellationToken ct);
    public Task<IEnumerable<T>> GetAllAsync<T>(AppFilter filter, CancellationToken ct);
    public Task<List<ProductResponseDto>> GetByIdsAsync(IEnumerable<int> productIds, CancellationToken ct);
}

