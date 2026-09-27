namespace OrderService.Infrastructure.Repositories;
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;
    private readonly QueryHelper _queryHelper;
    public OrderRepository(OrderDbContext context, QueryHelper queryHelper)
    {
        _context = context;
        _queryHelper = queryHelper;

    }

    #region Order
    public async Task AddAsync(Domain.Entities.Order entity)
    {
        await _context.Orders.AddAsync(entity);

    }
    public Task DeleteAsync(Domain.Entities.Order entity)
    {
        _context.Orders.Remove(entity);
        return Task.CompletedTask;
    }
    public async Task<Domain.Entities.Order?> GetAsync(Expression<Func<Domain.Entities.Order, bool>> predicate, CancellationToken ct)
    {
        return await _context.Orders.FirstOrDefaultAsync(predicate, ct);
    }
    public async Task<bool> CheckExistsAsync(Expression<Func<Domain.Entities.Order, bool>> predicate)
    {
        return await _context.Orders.AnyAsync(predicate);
    }
   
    public async Task<Domain.Entities.Order> GetByIdAsync(string id, CancellationToken ct)
    {
        return (await _context.Orders.FindAsync(id, ct));
    }
    public async Task<IEnumerable<T>> GetAllAsync<T>(AppFilter filter, CancellationToken ct)
    {
        var builder = new SqlBuilder();

        // 1. Safe Sorting for Orders
        var allowedSortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "customerid", "CustomerId" },
        { "orderdate", "OrderDate" },
        { "totalamount", "TotalAmount" },
        { "status", "Status" },
        { "createdby", "CreatedBy" }
    };

        // Default column is OrderDate
        string sortColumn = "OrderDate";
        if (!string.IsNullOrEmpty(filter.SortColumn) && allowedSortColumns.TryGetValue(filter.SortColumn, out var matchedColumn))
        {
            sortColumn = matchedColumn;
        }

        // Sort direction must be ASC or DESC (Default DESC for orders - newest first)
        string sortDirection = "DESC";
        if (!string.IsNullOrEmpty(filter.SortDirection) &&
            filter.SortDirection.Equals("ASC", StringComparison.OrdinalIgnoreCase))
        {
            sortDirection = "ASC";
        }

        builder.OrderBy($"{sortColumn} {sortDirection}");

        // 2. Query Construction
        var sql = @"SELECT COUNT(*) OVER() AS TotalCount, * 
                FROM Orders 
                /**where**/ 
                /**orderby**/";

        if (filter.IsPageSize == true)
        {
            sql += " OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
        }

        var selector = builder.AddTemplate(sql);

        // 3. Filter Conditions (Using Order properties)
        if (filter.FromDate.HasValue)
            builder.Where("OrderDate >= @FromDate", new { filter.FromDate });

        if (filter.ToDate.HasValue)
            builder.Where("OrderDate <= @ToDate", new { filter.ToDate });

        if (!string.IsNullOrEmpty(filter.SearchInput))
        {
            builder.Where("(CustomerId LIKE @SearchTerm OR CreatedBy LIKE @SearchTerm)", new { SearchTerm = $"%{filter.SearchInput}%" });
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

    #endregion

    #region OrderItem
    public async Task<OrderItem> GetAsync(Expression<Func<Domain.Entities.OrderItem, bool>> predicate, CancellationToken ct)
    {
        return await _context.OrderItems.FirstOrDefaultAsync(predicate, ct);
    }
    public async Task<IEnumerable<T>> GetItemsByOrderIdAsync<T>(string orderId, CancellationToken ct = default)
    {
        var builder = new SqlBuilder();
        var sql = @"select * from OrderItem /**where**/ /**orderby**/";
        var selector = builder.AddTemplate(sql);
        builder.Where("OrderId = @orderId", new { orderId });
        builder.OrderBy($" Id Desc");
        return await _queryHelper.QueryAsync<T>(selector.RawSql, ct);
    }
    public async Task DeleteItemsByOrderIdAsync(string id, CancellationToken ct)
    {
        var items = await _context.OrderItems.Where(x => x.OrderId == id).ToListAsync(ct);
        _context.OrderItems.RemoveRange(items);
    }
    #endregion

}

