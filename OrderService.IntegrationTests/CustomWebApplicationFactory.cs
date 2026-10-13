using OrderService.Infrastructure.Common.CacheProvider;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace OrderService.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:alpine")
        .Build();

    private Respawner _respawner = default!;
    private DbConnection _dbConnection = default!;

    public async Task InitializeAsync()
    {
        // Test Container နှစ်ခုစလုံးကို တစ်ပြိုင်နက် စတင်ခြင်း
        await Task.WhenAll(_dbContainer.StartAsync(), _redisContainer.StartAsync());

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
        await _redisContainer.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testSettings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString(),
                ["ConnectionStrings:Redis"] = _redisContainer.GetConnectionString()
            };

            config.AddInMemoryCollection(testSettings);
        });

        builder.ConfigureServices(services =>
        {
            // --- 1. SQL Server Configuration ---
            var dbDescriptor = services.SingleOrDefault(
                x => x.ServiceType == typeof(DbContextOptions<OrderDbContext>));
            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            services.AddDbContext<OrderDbContext>(options =>
            {
                options.UseSqlServer(_dbContainer.GetConnectionString());
            });

            // --- 2. Real Redis Container & Provider Configuration ---
            services.RemoveAll<IConnectionMultiplexer>();
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString()));

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = _redisContainer.GetConnectionString();
            });

            // Mock အစား Real Redis Hash Provider ကို အသုံးပြုခြင်း
            services.RemoveAll<IRedisHashProvider>();
            services.AddSingleton<IRedisHashProvider, RedisHashProvider>();

            // --- 3. gRPC Mocks ---
            services.RemoveAll<PaymentGrpcServiceClient>();
            var paymentGrpcMock = new Mock<PaymentGrpcServiceClient>();
            paymentGrpcMock
                .Setup(client => client.GetPaymentsByOrderIdsAsync(
                    It.IsAny<RequestOrderIdList>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(new AsyncUnaryCall<PaymentListResponse>(
                    Task.FromResult(new PaymentListResponse
                    {
                        Items =
                        {
                            new PaymentResponseDto
                            {
                                Id = 1,
                                OrderId = "ORD-001",
                                Amount = 150.0,
                                Currency = "USD",
                                PaymentStatus = "Completed"
                            }
                        }
                    }),
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { }));

            services.AddSingleton(paymentGrpcMock.Object);

            services.RemoveAll<ProductGrpcServiceClient>();
            var productGrpcMock = new Mock<ProductGrpcServiceClient>();
            productGrpcMock
                .Setup(client => client.GetProductsByIdsAsync(
                    It.IsAny<RequestProductIds>(),
                    It.IsAny<Metadata>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
                .Returns(new AsyncUnaryCall<ProductListResponse>(
                    Task.FromResult(new ProductListResponse
                    {
                        Items =
                        {
                            new ProductResponseDto
                            {
                                Id = 1,
                                Name = "Sample Product",
                                Price = 75.0
                            }
                        }
                    }),
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { }));

            services.AddSingleton(productGrpcMock.Object);

            // --- 4. Database Schema Creation ---
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
