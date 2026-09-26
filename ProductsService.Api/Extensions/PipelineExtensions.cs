namespace ProductsService.Api.Extensions;
public static class PipelineExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<GlobalExceptionHandler>();
        object value = app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API V1");
                options.RoutePrefix = "swagger";
            });

            //app.MapScalarApiReference("/scalar", options =>
            //{
            //    options.WithTitle("Product API")
            //           .WithTheme(ScalarTheme.DeepSpace)
            //           .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            //           .WithSidebar(true)
            //           .WithSearchHotKey("k")
            //           .WithLayout(ScalarLayout.Modern);

            //});
        }

        app.MapHealthChecks("/health");
        app.UseHttpsRedirection();

        //app.UseAuthentication();
        //app.UseAuthorization();

        return app;
    }
};
