namespace ProductService.Infrastructure.Common;
public class QueryHelper
{
    private readonly string _conStr;
    private readonly int _timeout;

    public QueryHelper(IConfiguration config)
    {
        _conStr = config.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(config), "Connection string 'DefaultConnection' not found.");

        var timeoutVal = config.GetSection("DbTimeout").Value;
        _timeout = timeoutVal != null ? int.Parse(timeoutVal) : 3600;
    }

    // 1. Get DataTable (ADO.NET with SqlParameter)
    public async Task<DataTable> GetDataTableAsync(string sSQL, SqlParameter[]? para, CancellationToken cancellationToken)
    {
        using (var newCon = new SqlConnection(_conStr))
        using (var cmd = new SqlCommand(sSQL, newCon))
        {
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = _timeout;

            if (para != null && para.Length > 0)
                cmd.Parameters.AddRange(para);

            await newCon.OpenAsync(cancellationToken);

            using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
            {
                var dt = new DataTable();
                dt.Load(reader);
                return dt;
            }
        }
    }

    // 2. Query Single or Default (Dapper - MSSQL)
    public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        using var con = new SqlConnection(_conStr);
        return await con.QueryFirstOrDefaultAsync<T>(
            new CommandDefinition(sql, param, commandTimeout: _timeout, cancellationToken: ct)
        );
    }

    // 3. SELECT Query (Multiple rows - Dapper - MSSQL)
    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        using var con = new SqlConnection(_conStr);
        return await con.QueryAsync<T>(
            new CommandDefinition(sql, param, commandTimeout: _timeout, cancellationToken: ct)
        );
    }

    // 4. Data Scalar (e.g., Count, Sum, Max - Dapper - MSSQL)
    public async Task<T?> ExecuteScalarAsync<T>(string sql, object? param = null, CancellationToken ct = default)
    {
        using var con = new SqlConnection(_conStr);
        return await con.ExecuteScalarAsync<T>(
            new CommandDefinition(sql, param, commandTimeout: _timeout, cancellationToken: ct)
        );
    }

    // 5. INSERT, UPDATE, DELETE (Dapper - MSSQL)
    public async Task<int> ExecuteAsync(string sql, object? param = null, CancellationToken ct = default)
    {
        using var con = new SqlConnection(_conStr);
        return await con.ExecuteAsync(
            new CommandDefinition(sql, param, commandTimeout: _timeout, cancellationToken: ct)
        );
    }

    // 6. Stored Procedure (Dapper - MSSQL)
    public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(string procName, object? param = null, CancellationToken ct = default)
    {
        using var con = new SqlConnection(_conStr);
        var command = new CommandDefinition(
            procName,
            param,
            commandType: CommandType.StoredProcedure,
            commandTimeout: _timeout,
            cancellationToken: ct
        );
        return await con.QueryAsync<T>(command);
    }

    // 7. Search Filter Helper (Dapper SqlBuilder အတွက်)
    public void ApplySearchFilter(SqlBuilder builder, string searchInput, string[] columns)
    {
        if (string.IsNullOrWhiteSpace(searchInput) || columns.Length == 0) return;

        var searchConditions = columns.Select(col => $"{col} LIKE @SearchTerm");
        var combinedCondition = "(" + string.Join(" OR ", searchConditions) + ")";

        builder.Where(combinedCondition, new { SearchTerm = $"%{searchInput}%" });
    }
}
