using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;

namespace ApiGateway.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddGatewayServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. CORS Policy Configuration (Frontend တွေက API Gateway ကို လှမ်းခေါ်ခွင့်ပြုရန်)
        var allowedOrigins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
                              ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        // 2. JWT Authentication Configuration
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Key"] ?? "DefaultSecretKey1234567890";
        var key = Encoding.ASCII.GetBytes(secretKey);

        //services.AddAuthentication(options =>
        //{
        //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //})
        //.AddJwtBearer(options =>
        //{
        //    options.RequireHttpsMetadata = false;
        //    options.SaveToken = true;
        //    options.TokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuerSigningKey = true,
        //        IssuerSigningKey = new SymmetricSecurityKey(key),
        //        ValidateIssuer = true,
        //        ValidIssuer = jwtSettings["Issuer"],
        //        ValidateAudience = true,
        //        ValidAudience = jwtSettings["Audience"],
        //        ValidateLifetime = true
        //    };
        //});

        //// 3. Authorization
        //services.AddAuthorization();

        // 4. Add YARP Reverse Proxy Services
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"));
        //services.AddOcelot(configuration);
        // 5. Add Caching & Rate Limiting 
        services.AddOutputCache(options =>
        {
            options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(30)));
            options.AddPolicy("ProductCache", builder => builder.Expire(TimeSpan.FromMinutes(5)));
        });

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("fixed", opt =>
            {
                opt.PermitLimit = 3;
                opt.Window = TimeSpan.FromSeconds(10);
                opt.QueueLimit = 2;
            });
            options.AddFixedWindowLimiter("strict", cfg =>
            {
                cfg.PermitLimit = 10;
                cfg.Window = TimeSpan.FromSeconds(10);
                cfg.QueueLimit = 0;
            });
        });

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}