using Auth0.Auth.Api.Extensions;
using System.Security.Claims;

namespace Auth0.Auth.Api.Endpoints
{
    public static class DataEndpoints
    {
        private static readonly List<object> DataStore = new()
        {
            new { Id = 1, Name = "Item 1", Value = "Value 1" },
            new { Id = 2, Name = "Item 2", Value = "Value 2" },
            new { Id = 3, Name = "Item 3", Value = "Value 3" }
        };

        public static void MapDataEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/data")
                .RequireAuthorization()
                .WithTags("Data");

            // 需要 read:data scope
            group.MapGet("/", (ClaimsPrincipal user) =>
            {
                return Results.Ok(new
                {
                    Message = "数据读取成功",
                    Data = DataStore,
                    Count = DataStore.Count,
                    Timestamp = DateTime.UtcNow
                });
            })
            .RequireAuthorization(policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(ctx =>
                    ctx.User.HasScope("read:data"));
            })
            .WithName("ReadAllData")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "读取所有数据";
                operation.Description = "需要 read:data scope 权限";
                return Task.CompletedTask;
            });

            // 需要 write:data scope
            group.MapPost("/", (object payload, ClaimsPrincipal user) =>
            {
                DataStore.Add(payload);

                return Results.Created($"/api/data/{DataStore.Count}", new
                {
                    Message = "数据写入成功",
                    CreatedBy = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    NewItem = payload,
                    TotalCount = DataStore.Count,
                    Timestamp = DateTime.UtcNow
                });
            })
            .RequireAuthorization(policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(ctx =>
                    ctx.User.HasScope("write:data"));
            })
            .WithName("WriteData")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "写入新数据";
                operation.Description = "需要 write:data scope 权限";
                return Task.CompletedTask;
            });

            // 需要 admin permission
            group.MapDelete("/purge", (ClaimsPrincipal user) =>
            {
                var count = DataStore.Count;
                DataStore.Clear();

                return Results.Ok(new
                {
                    Message = "所有数据已清除",
                    PurgedBy = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    PurgedCount = count,
                    Timestamp = DateTime.UtcNow
                });
            })
            .RequireAuthorization(policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(ctx =>
                    ctx.User.HasScope("admin"));
            })
            .WithName("PurgeData")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "清除所有数据";
                operation.Description = "需要 admin 权限，危险操作";
                return Task.CompletedTask;
            });
        }
    }
}
