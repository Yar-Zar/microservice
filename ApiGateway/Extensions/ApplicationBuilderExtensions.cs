using Ocelot.Middleware;

namespace ApiGateway.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseGatewayPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // 1. CORS Middleware (Authentication ရှေ့မှာ ရှိရပါမည်)
        app.UseCors("AllowSpecificOrigin");

        // 2. Rate Limiter Middleware
        //app.UseRateLimiter();

        // 3. Output Caching Middleware
        //app.UseOutputCache();

        // 4. Custom Correlation ID Middleware
        app.Use(async (context, next) =>
        {
            var correlationIdHeader = "X-Correlation-ID";
            if (!context.Request.Headers.ContainsKey(correlationIdHeader))
            {
                context.Request.Headers[correlationIdHeader] = Guid.NewGuid().ToString();
            }

            context.Response.OnStarting(() =>
            {
                context.Response.Headers[correlationIdHeader] = context.Request.Headers[correlationIdHeader];
                return Task.CompletedTask;
            });

            await next();
        });

        // 5. Authentication & Authorization Middlewares
        //app.UseAuthentication();
        //app.UseAuthorization();

        // 6. Map YARP Reverse Proxy Middleware
        app.MapReverseProxy();
        //app.UseOcelot().Wait();

        return app;
    }
}