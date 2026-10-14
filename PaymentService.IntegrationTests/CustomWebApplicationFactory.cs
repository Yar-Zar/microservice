namespace PaymentService.IntegrationTests;
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private Respawner _respawner = default!;
    private DbConnection _dbConnection = default!;

    public async Task InitializeAsync()
    {
        // 👈 Container ကို အရင်ဆုံး Start လုပ်ပေးရန် လိုအပ်ပါသည်
        await _dbContainer.StartAsync();

        // Respawn အတွက် MS SQL Connection ဖွင့်ခြင်း
        _dbConnection = new SqlConnection(_dbContainer.GetConnectionString());
        await _dbConnection.OpenAsync();

        // Respawner ကို Initialize လုပ်ခြင်း
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" }
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    public new async Task DisposeAsync()
    {
        if (_dbConnection != null)
        {
            await _dbConnection.DisposeAsync();
        }

        await _dbContainer.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testSettings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString()
            };

            config.AddInMemoryCollection(testSettings);
        });

        builder.ConfigureServices(services =>
        {
            // --- 1. SQL Server Configuration ---
            var dbDescriptor = services.SingleOrDefault(
                x => x.ServiceType == typeof(DbContextOptions<PaymentDbContext>));
            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            services.AddDbContext<PaymentDbContext>(options =>
            {
                options.UseSqlServer(_dbContainer.GetConnectionString());
            });

            // --- 2. gRPC Mocks ---
            services.RemoveAll<OrderGrpcServiceClient>();
            var orderGrpcMock = new Mock<OrderGrpcServiceClient>();
            orderGrpcMock.SetupUpdateOrderStatus();
            services.AddSingleton(orderGrpcMock.Object);

            // --- 3. Database Schema Creation ---
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
