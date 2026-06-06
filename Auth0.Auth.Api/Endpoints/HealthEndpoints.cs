namespace Auth0.Auth.Api.Endpoints
{
    public static class HealthEndpoints
    {
        public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api");

            group.MapGet("/health", () => Results.Ok(
                new
                {
                    status = "Healthy",
                    Timestamp = DateTimeOffset.UtcNow,
                    Version = "1.0.0"
                }))
                .WithName("HealthCheck")
                .WithTags("Health")
                .AddOpenApiOperationTransformer((operation, context, ct) =>
                {
                    operation.Summary = "Health Check Endpoint";
                    operation.Description = "Returns the health status of the API.";
                    return Task.CompletedTask;
                });
        }
    }
}
