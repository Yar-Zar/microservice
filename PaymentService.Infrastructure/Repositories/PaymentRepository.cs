namespace PaymentService.Infrastructure.Repositories;
public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _context;
    private readonly QueryHelper _queryHelper;
    public PaymentRepository(PaymentDbContext context, QueryHelper queryHelper)
    {
        _context = context;
        _queryHelper = queryHelper;

    }

    #region Payment
    public async Task AddAsync(Payment entity)
    {
        await _context.Payments.AddAsync(entity);

    }
    public Task DeleteAsync(Payment entity)
    {
        _context.Payments.Remove(entity);
        return Task.CompletedTask;
    }
    public async Task<Payment?> GetAsync(Expression<Func<Payment, bool>> predicate, CancellationToken ct)
    {
        return await _context.Payments.FirstOrDefaultAsync(predicate, ct);
    }
    public async Task<Payment> GetByIdAsync(int id, CancellationToken ct)
    {
        return (await _context.Payments.FindAsync(id, ct));
    }
    public async Task<Payment> GetByOrderIdAsync(string orderId, CancellationToken ct)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId, ct);
        return payment;
    }
    public async Task<IEnumerable<T>> GetAllAsync<T>(AppFilter filter, CancellationToken ct)
    {
        var builder = new SqlBuilder();

        // 1. Safe Sorting for Payments
        var allowedSortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "orderid", "OrderId" },
        { "amount", "Amount" },
        { "currency", "Currency" },
        { "paymentstatus", "PaymentStatus" },
        { "createdat", "CreatedAt" }
    };

        // Default column is CreatedAt (အသစ်ဆုံး ငွေပေးချေမှုများကို ဦးစားပေးရန်)
        string sortColumn = "CreatedAt";
        if (!string.IsNullOrEmpty(filter.SortColumn) && allowedSortColumns.TryGetValue(filter.SortColumn, out var matchedColumn))
        {
            sortColumn = matchedColumn;
        }

        // Sort direction must be ASC or DESC (Default DESC)
        string sortDirection = "DESC";
        if (!string.IsNullOrEmpty(filter.SortDirection) &&
            filter.SortDirection.Equals("ASC", StringComparison.OrdinalIgnoreCase))
        {
            sortDirection = "ASC";
        }

        builder.OrderBy($"{sortColumn} {sortDirection}");

        // 2. Query Construction (Payments Table)
        var sql = @"SELECT COUNT(*) OVER() AS TotalCount, * 
                FROM Payments 
                /**where**/ 
                /**orderby**/";

        if (filter.IsPageSize == true)
        {
            sql += " OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
        }

        var selector = builder.AddTemplate(sql);

        // 3. Filter Conditions (Using Payment properties)
        if (filter.FromDate.HasValue)
            builder.Where("CreatedAt >= @FromDate", new { filter.FromDate });

        if (filter.ToDate.HasValue)
            builder.Where("CreatedAt <= @ToDate", new { filter.ToDate });

        if (!string.IsNullOrEmpty(filter.SearchInput))
        {
            builder.Where("(PaymentStatus LIKE @SearchTerm OR TransactionId LIKE @SearchTerm)", new { SearchTerm = $"%{filter.SearchInput}%" });
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


}

