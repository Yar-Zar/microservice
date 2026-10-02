using Shared.GrpcContracts.Payment;
using Shared.GrpcContracts.Product;

namespace ProductService.Infrastructure.Repositories;
public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;
    private readonly QueryHelper _queryHelper;
    public ProductRepository(ProductDbContext context, QueryHelper queryHelper)
    {
        _context = context;
        _queryHelper = queryHelper;

    }
    public async Task AddAsync(Product entity)
    {
        await _context.Products.AddAsync(entity);

    }
    public Task DeleteAsync(Product entity)
    {
        _context.Products.Remove(entity);
        return Task.CompletedTask;
    }
    public async Task<Product?> GetAsync(Expression<Func<Product, bool>> predicate, CancellationToken ct)
    {
        return await _context.Products.FirstOrDefaultAsync(predicate, ct);
    }
    public async Task<bool> CheckExistsAsync(Expression<Func<Product, bool>> predicate)
    {
        return await _context.Products.AnyAsync(predicate);
    }
    public async Task<Product> GetByIdAsync(int id, CancellationToken ct)
    {
        return (await _context.Products.FindAsync(id, ct));
    }
    public async Task<IEnumerable<DropdownDto>> GetProductsForDropdownAsync(CancellationToken ct)
    {
        return await _context.Products
            .AsNoTracking()
            .Select(x => new DropdownDto(x.Id, x.Name))
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>(AppFilter filter, CancellationToken ct)
    {
        var builder = new SqlBuilder();

        // 1. Safe Sorting 
        var allowedSortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "name", "Name" },
        { "price", "Price" },
        { "stock", "Stock" },
        { "createdat", "CreatedAt" }
    };

        // Default column is Name
        string sortColumn = "Name";
        if (!string.IsNullOrEmpty(filter.SortColumn) && allowedSortColumns.TryGetValue(filter.SortColumn, out var matchedColumn))
        {
            sortColumn = matchedColumn;
        }

        // Sort direction must be ASC or DESC
        string sortDirection = "ASC";
        if (!string.IsNullOrEmpty(filter.SortDirection) &&
            filter.SortDirection.Equals("DESC", StringComparison.OrdinalIgnoreCase))
        {
            sortDirection = "DESC";
        }

        builder.OrderBy($"{sortColumn} {sortDirection}");

        // 2. Query Construction
        var sql = @"SELECT COUNT(*) OVER() AS TotalCount, * 
                FROM Products 
                /**where**/ 
                /**orderby**/";

        if (filter.IsPageSize == true)
        {
            sql += " OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
        }

        var selector = builder.AddTemplate(sql);

        // 3. Filter Conditions
        if (filter.FromDate.HasValue)
            builder.Where("CreatedAt >= @FromDate", new { filter.FromDate });

        if (filter.ToDate.HasValue)
            builder.Where("CreatedAt <= @ToDate", new { filter.ToDate });

        if (!string.IsNullOrEmpty(filter.SearchInput))
        {
            builder.Where("(Name LIKE @SearchTerm OR description LIKE @SearchTerm)", new { SearchTerm = $"%{filter.SearchInput}%" });
        }

        // 4. Safe Parameters & Pagination Limits
        var parameters = new DynamicParameters(selector.Parameters);

        if (filter.IsPageSize == true)
        {
            int pageNo = Math.Max(1, filter.CurrentPageNo ?? 1);
            int pageSize = Math.Clamp(filter.CurrentRowLimit ?? 10, 1, 100);

            parameters.Add("Offset", (pageNo - 1) * pageSize);
            parameters.Add("Limit", pageSize);
        }

        return await _queryHelper.QueryAsync<T>(selector.RawSql, parameters, ct);
    }

    public async Task<List<ProductResponseDto>> GetByIdsAsync(IEnumerable<int> productIds, CancellationToken ct)
    {

        var products = await _context.Products
     .Where(p => productIds.Contains(p.Id))
     .Select(x => new ProductResponseDto
     {
         Id = x.Id,
         Name = x.Name,
         Price =(double) x.Price
     })
     .ToListAsync(ct);

        return products;
    }
    //public async Task<IEnumerable<T>> GetDataAsync<T>(System.Linq.Expressions.Expression<Func<Product, T>> selector)
    //{
    //    return await _context.Products
    //        .AsNoTracking()
    //        .Select(selector)
    //        .ToListAsync();
    //}
}

